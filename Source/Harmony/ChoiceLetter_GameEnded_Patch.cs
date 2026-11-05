using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RandomStartMod
{
    [HarmonyPatch(typeof(ChoiceLetter_GameEnded), nameof(ChoiceLetter_GameEnded.Choices), MethodType.Getter)]
    public static class ChoiceLetter_GameEnded_Patch
    {
        private sealed class RandomStartChoice : DiaOption
        {
            internal RandomStartChoice() : base("RandomStartMod.Title".Translate())
            {
                resolveTree = true;
                action = () => LongEventHandler.QueueLongEvent(RandomScenario.SetupForRandomPlay,
                    "GeneratingMap", false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
            }
        }

        [HarmonyPostfix]
        public static IEnumerable<DiaOption> Postfix(IEnumerable<DiaOption> __result) =>
            AdditiveOptions.AppendOnce(__result, option => option is RandomStartChoice, () => new RandomStartChoice());
    }
}
