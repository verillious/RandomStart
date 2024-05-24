﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RandomStartMod
{
    public static class Util
    {
        public static void LogMessage(string message)
        {
            Log.Message($"[{"RandomStartMod.Title".Translate()}] {message}");
        }

        public static bool IsModRunning(string modName)
        {
            return LoadedModManager.RunningMods.Count((ModContentPack m) => m.Name == modName) > 0;
        }
    }
}
