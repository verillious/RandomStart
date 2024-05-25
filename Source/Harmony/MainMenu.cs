using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using Verse.Steam;
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
            }

            foreach (ListableOption opt in optList)
            {
                if (opt.action != null)
                {
                    ListableOption newOption = new ListableOption(
                    "Random".Translate(),
                    delegate
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            LongEventHandler.QueueLongEvent(delegate
                            {
                                RandomScenario.SetupForRandomPlay();
                            }, "GeneratingMap", doAsynchronously: false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
                        }
                        else
                        {
                            Mod randomStartMod = LoadedModManager.ModHandles.First((Mod m) => m.Content.Name == "Random Start");
                            Find.WindowStack.Add(new Dialog_ModSettings(randomStartMod));
                        }
                    },
                    null
                    );
                    optList.Insert(0, newOption);
                    break;
                }
            }
        }
    }

    //[HarmonyPatch(typeof(ScenPart), "PostGameStart")]
    //public class ScenarioPart_Patch
    //{
    //    [HarmonyPostfix]
    //    public static void Postfix(ScenPart __instance)
    //    {
    //        Util.LogMessage(__instance.def.defName);
    //    }
    //}
}
