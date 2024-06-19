using HarmonyLib;
using RimWorld;
using Verse;

namespace RandomStartMod
{
    [HarmonyPatch(typeof(ChoiceLetter), "OpenLetter")]
    public class ChoiceLetter_GameEnded_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ChoiceLetter __instance)
        {
            if (__instance.GetType() != typeof(ChoiceLetter_GameEnded))
            {
                return true;
            }
            DiaNode diaNode = new DiaNode(__instance.text);
            diaNode.options.AddRange(__instance.Choices);

            DiaOption randomStartOption = new DiaOption("RandomStartMod.Title".Translate());
            randomStartOption.action = delegate
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    RandomScenario.SetupForRandomPlay();
                }, "GeneratingMap", doAsynchronously: false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
            };
            randomStartOption.resolveTree = true;

            Util.LogMessage("Patching GameEnded letter");
            diaNode.options.Add(randomStartOption);
            Dialog_NodeTreeWithFactionInfo window = new Dialog_NodeTreeWithFactionInfo(diaNode, __instance.relatedFaction, delayInteractivity: false, __instance.radioMode, __instance.title);
            Find.WindowStack.Add(window);
            return false;
        }

    }
}
