using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RandomStartMod
{
    [HarmonyPatch(typeof(Page_SelectScenario), nameof(Page_SelectScenario.BeginScenarioConfiguration))]
    internal static class OrdinaryScenarioStart_Patch
    {
        [HarmonyPrefix]
        static void Prefix() => RandomStartData.End(Current.Game);
    }

    [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
    internal static class EndRandomStart_Patch
    {
        [HarmonyFinalizer]
        static void Finalizer(Game __instance) => RandomStartData.End(__instance);
    }

    [HarmonyPatch(typeof(Scenario), nameof(Scenario.PostIdeoChosen))]
    internal static class StartingPawnPhase_Patch
    {
        [HarmonyPrefix]
        static void Prefix(Scenario __instance, bool __runOriginal, out IDisposable __state)
        {
            __state = __runOriginal && ReferenceEquals(__instance, Find.Scenario)
                ? RandomStartData.State?.PreparingPawns.Enter(true) : null;
        }
        [HarmonyPostfix]
        static void Postfix(IDisposable __state, bool __runOriginal)
        {
            var state = RandomStartData.State;
            if (__runOriginal && __state != null && state != null) RandomScenario.FinishStartingPawnSetup(state.Settings);
        }
        [HarmonyFinalizer]
        static void Finalizer(IDisposable __state) => __state?.Dispose();
    }

    [HarmonyPatch(typeof(StartingPawnUtility), nameof(StartingPawnUtility.NewGeneratedStartingPawn))]
    internal static class StartingPawnSlot_Patch
    {
        [HarmonyPrefix]
        static void Prefix(bool __runOriginal, out IDisposable __state)
        {
            var state = RandomStartData.State;
            __state = __runOriginal && state != null && state.PreparingPawns.Value ? state.GeneratingPawn.Enter(true) : null;
        }
        [HarmonyFinalizer]
        static void Finalizer(IDisposable __state) => __state?.Dispose();
    }

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
    internal static class StartingPawnRequest_Patch
    {
        [HarmonyPrefix]
        static void Prefix(ref PawnGenerationRequest __0)
        {
            var state = RandomStartData.State;
            if (state == null || !state.GeneratingPawn.Value
                || __0.Context != PawnGenerationContext.PlayerStarter
                || __0.KindDef?.race?.race?.Humanlike != true) return;
            var settings = state.Settings;
            if (state.NativeStartup == null && settings.enablePrepareModeratelyIntegration
                && Compat.PrepareModeratelyCompat.IsAvailable) return;
            bool allowsAdults = (__0.AllowedDevelopmentalStages & DevelopmentalStage.Adult) != 0;
            if (settings.startingPawnForceViolence && allowsAdults) __0.MustBeCapableOfViolence = true;
            // Explicit scenario restrictions take precedence over optional age/sex overrides.
            if (!settings.randomisePawnSex && !__0.FixedGender.HasValue
                && (settings.PawnSex == (int)Gender.Male || settings.PawnSex == (int)Gender.Female))
                __0.FixedGender = (Gender)settings.PawnSex;
            if (!settings.randomisePawnAge && !__0.FixedBiologicalAge.HasValue && allowsAdults)
            {
                float min = settings.randomisePawnAgeRange.min;
                float max = settings.randomisePawnAgeRange.max;
                if (__0.BiologicalAgeRange.HasValue)
                {
                    min = Math.Max(min, __0.BiologicalAgeRange.Value.min);
                    max = Math.Min(max, __0.BiologicalAgeRange.Value.max);
                }
                var excluded = __0.ExcludeBiologicalAgeRange;
                bool fullyExcluded = excluded.HasValue && excluded.Value.min <= min && excluded.Value.max >= max;
                if (min <= max && !fullyExcluded) __0.BiologicalAgeRange = new FloatRange(min, max);
            }
            if (settings.PawnNotDisabledWorkTags && allowsAdults)
            {
                var original = __0.ValidatorPostGear;
                __0.ValidatorPostGear = pawn => (original == null || original(pawn))
                    && pawn.GetDisabledWorkTypes(true).Count == 0;
            }
        }
    }

    [HarmonyPatch(typeof(Scenario), nameof(Scenario.PreMapGenerate))]
    internal static class StartingItemPlan_Patch
    {
        [HarmonyPrefix]
        static void Prefix(Scenario __instance)
        {
            var state = RandomStartData.State;
            if (state == null || state.ItemsPrepared || !ReferenceEquals(__instance, Find.Scenario)) return;
            // Wait for the world and pawns to exist before asking modded item/stat
            // providers for eligible choices. The original callback and arrival run.
            state.ItemsPrepared = true;
            StartingItems.AddRandomItems(__instance, state.Settings);
        }
    }

    [HarmonyPatch(typeof(GameInitData), nameof(GameInitData.PrepForMapGen))]
    internal static class StartingPossessions_Patch
    {
        [HarmonyPostfix]
        static void Postfix(GameInitData __instance)
        {
            if (RandomStartData.State?.Settings.removeStartingItems != true
                || !ReferenceEquals(__instance, Find.GameInitData)) return;
            if (__instance.startingPossessions == null) return;
            foreach (var possessions in __instance.startingPossessions.Values) possessions?.Clear();
        }
    }
}
