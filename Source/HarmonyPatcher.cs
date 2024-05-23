using HarmonyLib;
using Verse;

namespace RandomStartMod
{
    [StaticConstructorOnStartup]
    public static class RandomStart
    {
        static RandomStart()
        {
            var harmony = new Harmony("com.bogknight.RandomStart");
            harmony.PatchAll();
            Log.Message($"[{"RandomStartMod.Title".Translate()}] 1.0.0");
        }
    }
}
