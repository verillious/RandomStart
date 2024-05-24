using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

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
            }

            foreach (ListableOption opt in optList)
            {
                if (opt.action != null)
                {
                    ListableOption newOption = new ListableOption(
                    "Random".Translate(),
                    delegate
                    {
                        LongEventHandler.QueueLongEvent(delegate
                            {
                                RandomScenario.SetupForRandomPlay();
                            }, "GeneratingMap", doAsynchronously: false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
                    },
                    null
                    );
                    optList.Insert(0, newOption);
                    break;
                }
            }
        }
    }
}
