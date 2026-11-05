using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RandomStartMod
{
    [HarmonyPatch(typeof(OptionListingUtility), nameof(OptionListingUtility.DrawOptionListing))]
    public static class OptionListingUtility_Patch
    {
        private sealed class RandomStartOption : ListableOption
        {
            public RandomStartOption(Action action) : base("Random".Translate(), action, null) { }
        }

        [HarmonyPrefix]
        public static void Prefix(List<ListableOption> optList)
        {
            if (Current.ProgramState != ProgramState.Entry || optList == null) return;
            AdditiveOptions.InsertBefore(optList, option => option.label == "NewColony".Translate(),
                option => option is RandomStartOption, () => new RandomStartOption(() =>
                {
                    RandomStartMod mod = LoadedModManager.GetMod<RandomStartMod>();
                    if (!Input.GetMouseButtonUp(0))
                        Find.WindowStack.Add(new Dialog_ModSettings(mod));
                    else if (mod.CheckFirstTimeDialog())
                        LongEventHandler.QueueLongEvent(RandomScenario.SetupForRandomPlay,
                            "GeneratingMap", false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
                }));
        }
    }
}
