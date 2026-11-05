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

    public static class RealRuinsCompat
    {
        private static readonly OptionalModApi Api = new OptionalModApi("Real Ruins", message => Log.Warning(message));

        public static void CreatePOIs()
        {
            Api.TryRun(() =>
            {
                Type pageType = OptionalModApi.RequireType("RealRuins.Page_PlanetaryRuinsLoader");
                Type settingsType = OptionalModApi.RequireType("RealRuins.RealRuins_ModSettings");
                FieldInfo optionsField = OptionalModApi.RequireField(settingsType, "planetaryRuinsOptions", true);
                object options = optionsField.GetValue(null);
                if (options == null || !typeof(Window).IsAssignableFrom(pageType))
                    throw new InvalidOperationException("Real Ruins planetary options or page are unavailable.");

                FieldInfo mode = OptionalModApi.RequireField(pageType, "mode", false);
                if (!mode.FieldType.IsEnum)
                    throw new InvalidOperationException("Real Ruins page mode is no longer an enum.");
                object fullAuto = Enum.Parse(mode.FieldType, "FullAuto");
                FieldInfo download = OptionalModApi.RequireField(pageType, "downloadLimit", false, typeof(int));
                FieldInfo transfer = OptionalModApi.RequireField(pageType, "transferLimit", false, typeof(int));
                FieldInfo abandoned = OptionalModApi.RequireField(pageType, "abandonedPercentage", false, typeof(int));
                FieldInfo discard = OptionalModApi.RequireField(pageType, "aggressiveDiscard", false, typeof(bool));
                FieldInfo biome = OptionalModApi.RequireField(pageType, "biomeStrict", false, typeof(bool));
                MethodInfo start = OptionalModApi.RequireMethod(pageType, "StartLoadingList", typeof(void));
                int downloadLimit = (int)OptionalModApi.RequireField(options.GetType(), "downloadLimit", false, typeof(int)).GetValue(options);
                int transferLimit = (int)OptionalModApi.RequireField(options.GetType(), "transferLimit", false, typeof(int)).GetValue(options);
                int abandonedPercentage = (int)Convert.ToSingle(OptionalModApi.RequireField(options.GetType(), "abandonedLocations", false).GetValue(options));
                bool aggressiveDiscard = (bool)OptionalModApi.RequireField(options.GetType(), "excludePlainRuins", false, typeof(bool)).GetValue(options);
                RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();

                var page = (Window)Activator.CreateInstance(pageType);
                mode.SetValue(page, fullAuto);
                download.SetValue(page, downloadLimit);
                transfer.SetValue(page, transferLimit);
                abandoned.SetValue(page, abandonedPercentage);
                discard.SetValue(page, aggressiveDiscard);
                biome.SetValue(page, settings.realRuinsBiomeFilter);
                start.Invoke(page, null);
                page.Close(false);
            });
        }
    }

}
