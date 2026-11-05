using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RandomStartMod
{
    [HarmonyPatch(typeof(Scenario), nameof(Scenario.PreConfigure))]
    internal static class NativeScenarioConfiguration_Patch
    {
        [HarmonyPrefix]
        static void Prefix(Scenario __instance, bool __runOriginal, out bool __state) =>
            __state = __runOriginal && NativeStartupController.Attach(__instance);

        [HarmonyPostfix]
        static void Postfix(bool __state, bool __runOriginal)
        {
            if (__state && !__runOriginal)
                throw new InvalidOperationException("Another patch replaced scenario PreConfigure; native startup requires investigation.");
        }
    }

    [HarmonyPatch(typeof(WindowStack), nameof(WindowStack.Add))]
    internal static class NativePageOpened_Patch
    {
        [HarmonyPostfix]
        static void Postfix(Window __0, bool __runOriginal)
        {
            if (!__runOriginal) return;
            var controller = RandomStartData.State?.NativeStartup;
            if (controller == null) return;
            try { controller.WindowOpened(__0); }
            catch (Exception error) { NativeStartupController.Fail(error); }
        }
    }

    [HarmonyPatch(typeof(WindowStack), nameof(WindowStack.WindowStackOnGUI))]
    internal static class NativePageAdvance_Patch
    {
        [HarmonyPostfix]
        static void Postfix(bool __runOriginal)
        {
            if (!__runOriginal) return;
            var controller = RandomStartData.State?.NativeStartup;
            if (controller == null) return;
            try
            {
                controller.AfterWindowsDrawn();
                controller.RefreshProgress();
            }
            catch (Exception error) { NativeStartupController.Fail(error); }
        }
    }
}
