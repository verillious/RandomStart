using RimWorld;
using Verse;
using HarmonyLib;
using System.Linq;

namespace RandomStartMod
{
    [StaticConstructorOnStartup]
    public static class RandomStart
    {
        static RandomStart()
        {
            var harmony = new Harmony("com.bogknight.RandomStart");
            harmony.PatchAll();
            Mod mod = LoadedModManager.ModHandles.First((Mod m) => m.Content.Name == "Random Start");
            Log.Message($"[{"RandomStartMod.Title".Translate()}] {mod.Content.ModMetaData.ModVersion}");
        }
    }
}
