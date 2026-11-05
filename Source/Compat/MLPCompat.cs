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

    public static class MLPCompat
    {
        public static void DrawMLPSlider(Rect rect)
        {
            RandomStartSettings settings = LoadedModManager
                .GetMod<RandomStartMod>()
                .GetSettings<RandomStartSettings>();
            settings.myLittlePlanetSubcount = Mathf.RoundToInt(
                Widgets.HorizontalSlider(
                    rect,
                    settings.myLittlePlanetSubcount,
                    6f,
                    10f,
                    middleAlignment: true,
                    null,
                    "MLPWorldTiny".Translate(),
                    "MLPWorldDefault".Translate(),
                    1f
                )
            );
            PlanetLayerSettingsDefOf.Surface.settings.subdivisions =
                settings.myLittlePlanetSubcount;
        }
    }

}
