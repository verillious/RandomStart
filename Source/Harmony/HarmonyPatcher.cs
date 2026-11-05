using RimWorld;
using Verse;
using HarmonyLib;

namespace RandomStartMod
{
    [StaticConstructorOnStartup]
    public static class RandomStartPatcher
    {
        static RandomStartPatcher()
        {
            var harmony = new Harmony("com.bogknight.RandomStart");
            harmony.PatchAll();
            Mod mod = LoadedModManager.GetMod<RandomStartMod>();
            Util.LogMessage($"{mod.Content.ModMetaData.ModVersion}");
        }
    }
}
