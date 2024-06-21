using RimWorld;
using Verse;
using HarmonyLib;
using System.Linq;

namespace RandomStartMod
{
    [StaticConstructorOnStartup]
    public static class RandomStartPatcher
    {
        static RandomStartPatcher()
        {
            var harmony = new Harmony("com.bogknight.RandomStart");
            harmony.PatchAll();
            Mod mod = LoadedModManager.ModHandles.First((Mod m) => m.Content.Name == "Random Start");
            Util.LogMessage($"{mod.Content.ModMetaData.ModVersion}");
        }
    }
}
