using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RandomStartMod.Compat
{

    public static class NoPauseCompat
    {
        private static readonly OptionalModApi Api = new OptionalModApi("No Pause Challenge", message => Log.Warning(message));

        public static void SetupForNoPause()
        {
            Api.TryRun(() =>
            {
                Type main = OptionalModApi.RequireType("NoPauseChallenge.Main");
                FieldInfo enabled = OptionalModApi.RequireField(main, "noPauseEnabled", true, typeof(bool));
                FieldInfo halfSpeed = OptionalModApi.RequireField(main, "halfSpeedActive", true, typeof(bool));
                RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();
                enabled.SetValue(null, settings.noPauseEnabled);
                halfSpeed.SetValue(null, settings.noPauseHalfSpeedEnabled);
            });
        }
    }

}
