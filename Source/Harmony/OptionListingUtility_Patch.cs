using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Linq;

namespace RandomStartMod
{
    [HarmonyPatch(typeof(OptionListingUtility), "DrawOptionListing")]
    public class OptionListingUtility_Patch
    {
        private static bool justEnteredMainMenu = false;

        [HarmonyPrefix]
        public static void Prefix(ref List<ListableOption> optList)
        {
            if (Current.ProgramState != ProgramState.Entry)
            {
                justEnteredMainMenu = false;
                return;
            }

            if (!justEnteredMainMenu)
            {
                justEnteredMainMenu = true;
                RandomStartData.startedFromRandom = false;
            }

            int index = 0;

            if (ModsConfig.IsActive("phoenix.ContinueButton") || ModsConfig.IsActive("phoenix.ContinueButton_Steam"))
            {
                index = 1;
            }

            foreach (ListableOption opt in optList)
            {
                if (opt.label == "NewColony".Translate())
                {
                    RandomStartMod randomStartMod = LoadedModManager.GetMod<RandomStartMod>();
                    ListableOption newOption = new ListableOption(
                    "Random".Translate(),
                    delegate
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            if (randomStartMod.CheckFirstTimeDialog())
                            {
                                LongEventHandler.QueueLongEvent(delegate
                                {
                                    RandomScenario.SetupForRandomPlay();
                                }, "GeneratingMap", doAsynchronously: false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
                            }

                        }
                        else
                        {
                            Find.WindowStack.Add(new Dialog_ModSettings(randomStartMod));
                        }
                    },
                    null
                    );
                    optList.Insert(index, newOption);
                    break;
                }
            }
        }
    }
}
