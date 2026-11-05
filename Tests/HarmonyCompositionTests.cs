using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RandomStartMod;
using Xunit;

// Exercise the real pinned Harmony runtime on a fake target. RimWorld's stripped
// reference assemblies cannot be loaded as an engine test on this host.
public class HarmonyCompositionTests
{
    private static readonly ScopedValue<bool> InCall = new ScopedValue<bool>();
    private static readonly List<string> Calls = new List<string>();
    private static readonly Exception Failure = new InvalidOperationException("Original failure");
    private static Exception observed;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Target(int value)
    {
        Calls.Add("original");
        if (value < 0) throw Failure;
        return value * 2;
    }

    private static void Enter(out IDisposable __state)
    {
        Calls.Add("enter");
        __state = InCall.Enter(true);
    }

    private static void EnterWhenOriginalRuns(bool __runOriginal, out IDisposable __state)
    {
        __state = null;
        if (__runOriginal) Enter(out __state);
    }

    private static void After(IDisposable __state, bool __runOriginal)
    {
        Calls.Add("after:" + __runOriginal);
        __state?.Dispose();
    }

    private static void Cleanup(IDisposable __state, Exception __exception)
    {
        observed = __exception;
        Calls.Add("cleanup");
        __state?.Dispose();
    }

    private static bool ForeignVeto(ref int __result) { __result = 42; return false; }
    private static void ForeignInput(ref int value) => value++;
    private static void ForeignFailure() => throw Failure;

    private static Harmony Patch(string prefix = nameof(Enter), bool includePostfix = true)
    {
        Calls.Clear(); observed = null;
        var harmony = new Harmony("RandomStart.Tests." + Guid.NewGuid().ToString("N"));
        harmony.Patch(AccessTools.Method(typeof(HarmonyCompositionTests), nameof(Target)),
            prefix: new HarmonyMethod(typeof(HarmonyCompositionTests), prefix),
            postfix: includePostfix ? new HarmonyMethod(typeof(HarmonyCompositionTests), nameof(After)) : null,
            finalizer: new HarmonyMethod(typeof(HarmonyCompositionTests), nameof(Cleanup)));
        return harmony;
    }

    [Fact]
    public void ACallIncludesOtherModsPatchesAndCleansUpAfterSuccess()
    {
        Harmony harmony = Patch();
        try
        {
            harmony.Patch(AccessTools.Method(typeof(HarmonyCompositionTests), nameof(Target)),
                prefix: new HarmonyMethod(typeof(HarmonyCompositionTests), nameof(ForeignInput)));
            Assert.Equal(8, Target(3));
            Assert.Equal(new[] { "enter", "original", "after:True", "cleanup" }, Calls);
            Assert.False(InCall.Value);
            Assert.Null(observed);
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }

    [Fact]
    public void AStateOnlyPrefixStillRunsAfterAnEarlierVeto()
    {
        Harmony harmony = Patch();
        try
        {
            harmony.Patch(AccessTools.Method(typeof(HarmonyCompositionTests), nameof(Target)),
                prefix: new HarmonyMethod(typeof(HarmonyCompositionTests), nameof(ForeignVeto)) { priority = Priority.First });
            Assert.Equal(42, Target(3));
            Assert.Equal(new[] { "enter", "after:False", "cleanup" }, Calls);
            Assert.False(InCall.Value);
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }

    [Fact]
    public void RunOriginalGuardAvoidsEnteringAScopeAfterAnEarlierVeto()
    {
        Harmony harmony = Patch(nameof(EnterWhenOriginalRuns));
        try
        {
            harmony.Patch(AccessTools.Method(typeof(HarmonyCompositionTests), nameof(Target)),
                prefix: new HarmonyMethod(typeof(HarmonyCompositionTests), nameof(ForeignVeto)) { priority = Priority.First });
            Assert.Equal(42, Target(3));
            Assert.Equal(new[] { "after:False", "cleanup" }, Calls);
            Assert.False(InCall.Value);
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }

    [Fact]
    public void FinalizerRestoresScopeAndPreservesAnOriginalException()
    {
        Harmony harmony = Patch();
        try
        {
            Assert.Same(Failure, Assert.Throws<InvalidOperationException>(() => Target(-1)));
            Assert.Same(Failure, observed);
            Assert.Equal(new[] { "enter", "original", "cleanup" }, Calls);
            Assert.False(InCall.Value);
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }

    [Fact]
    public void FinalizerAlsoRestoresScopeWhenAnotherModsPrefixThrows()
    {
        Harmony harmony = Patch();
        try
        {
            harmony.Patch(AccessTools.Method(typeof(HarmonyCompositionTests), nameof(Target)),
                prefix: new HarmonyMethod(typeof(HarmonyCompositionTests), nameof(ForeignFailure)) { priority = Priority.Last });
            Assert.Same(Failure, Assert.Throws<InvalidOperationException>(() => Target(1)));
            Assert.Same(Failure, observed);
            Assert.Equal(new[] { "enter", "cleanup" }, Calls);
            Assert.False(InCall.Value);
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }

    [Fact]
    public void FinalizerAlsoRestoresScopeWhenAnotherModsPostfixThrows()
    {
        Harmony harmony = Patch(includePostfix: false);
        try
        {
            harmony.Patch(AccessTools.Method(typeof(HarmonyCompositionTests), nameof(Target)),
                postfix: new HarmonyMethod(typeof(HarmonyCompositionTests), nameof(ForeignFailure)));
            Assert.Same(Failure, Assert.Throws<InvalidOperationException>(() => Target(1)));
            Assert.Same(Failure, observed);
            Assert.Equal(new[] { "enter", "original", "cleanup" }, Calls);
            Assert.False(InCall.Value);
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }
}
