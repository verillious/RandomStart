﻿using RimWorld;
using Verse;
using HarmonyLib;

namespace RandomStartMod
{
    [StaticConstructorOnStartup]
    public static class RandomStart
    {
        static RandomStart()
        {
            var harmony = new Harmony("com.bogknight.RandomStart");
            harmony.PatchAll();
            Log.Message($"[{"RandomStartMod.Title".Translate()}] Patched successfully!");
        }
    }
}
