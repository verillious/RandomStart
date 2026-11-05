using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace RandomStartMod.Compat
{

    public static class RealisticPlanetsCompat
    {
        private static readonly OptionalModApi Api = new OptionalModApi("Realistic Planets", message => Log.Warning(message));
        private static Assembly realisticPlanetsAssembly;
        private static Type worldPresetUtilityType;
        private static Type worldPresetType;
        private static Type planetsGameComponentType;
        private static Type worldTypeType;
        private static Type axialTiltType;
        private static Type worldTypeUtilityType;
        private static Type axialTiltUtilityType;

        static RealisticPlanetsCompat()
        {
            try
            {
                realisticPlanetsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Realistic_Planets_1.6");

                if (realisticPlanetsAssembly != null)
                {
                    worldPresetUtilityType = realisticPlanetsAssembly.GetType("Planets_Code.Presets.WorldPresetUtility");
                    worldPresetType = realisticPlanetsAssembly.GetType("Planets_Code.Presets.WorldPreset");
                    planetsGameComponentType = realisticPlanetsAssembly.GetType("Planets_Code.Core.Planets_GameComponent");
                    worldTypeType = realisticPlanetsAssembly.GetType("Planets_Code.WorldGen.WorldType");
                    axialTiltType = realisticPlanetsAssembly.GetType("Planets_Code.WorldGen.AxialTilt");
                    worldTypeUtilityType = realisticPlanetsAssembly.GetType("Planets_Code.WorldGen.WorldTypeUtility");
                    axialTiltUtilityType = realisticPlanetsAssembly.GetType("Planets_Code.WorldGen.AxialTiltUtility");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RandomStart] [RealisticPlanetsCompat] Failed to initialize Realistic Planets compatibility: {ex.Message}");
            }
        }

        public static bool IsAvailable =>
            realisticPlanetsAssembly != null &&
            worldPresetUtilityType != null &&
            worldPresetType != null && planetsGameComponentType != null;

        public static int GetWorldTypeEnumCount()
        {
            if (!IsAvailable || worldTypeUtilityType == null)
                return 5; // Default fallback count

            try
            {
                var enumValuesCountProperty = worldTypeUtilityType.GetProperty("EnumValuesCount", BindingFlags.Public | BindingFlags.Static);
                if (enumValuesCountProperty != null)
                {
                    return (int)enumValuesCountProperty.GetValue(null);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RandomStart] [RealisticPlanetsCompat] Failed to get WorldType enum count: {ex.Message}");
            }

            return 5; // Default fallback count
        }

        public static int GetAxialTiltEnumCount()
        {
            if (!IsAvailable || axialTiltUtilityType == null)
                return 5; // Default fallback count

            try
            {
                var enumValuesCountProperty = axialTiltUtilityType.GetProperty("EnumValuesCount", BindingFlags.Public | BindingFlags.Static);
                if (enumValuesCountProperty != null)
                {
                    return (int)enumValuesCountProperty.GetValue(null);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RandomStart] [RealisticPlanetsCompat] Failed to get AxialTilt enum count: {ex.Message}");
            }

            return 5; // Default fallback count
        }

        public static void DoWorldTypeSelectionButton(Listing_Standard listingStandard)
        {
            if (!IsAvailable)
                return;

            try
            {
                var worldPresetsProperty = worldPresetUtilityType.GetProperty("WorldPresets", BindingFlags.Public | BindingFlags.Static);
                if (worldPresetsProperty == null)
                    return;

                var worldPresets = worldPresetsProperty.GetValue(null) as IEnumerable;
                if (worldPresets == null)
                    return;

                var nameProperty = worldPresetType.GetProperty("Name");
                if (nameProperty == null)
                    return;

                RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();

                var validPresets = new List<object>();
                var validPresetNames = new List<string>();

                foreach (var preset in worldPresets)
                {
                    var name = nameProperty.GetValue(preset) as string;
                    if (name != null && name != "Planets.Custom")
                    {
                        validPresets.Add(preset);
                        validPresetNames.Add(name);
                    }
                }

                string worldTypeName = settings.realisticPlanetsWorldType;
                if (!validPresetNames.Contains(worldTypeName))
                {
                    worldTypeName = "Planets.Vanilla";
                    settings.realisticPlanetsWorldType = worldTypeName;
                }

                if (listingStandard.ButtonText(worldTypeName))
                {
                    List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();

                    var worldTypeProperty = worldPresetType.GetProperty("WorldType");
                    var axialTiltProperty = worldPresetType.GetProperty("AxialTilt");
                    var rainfallModifierProperty = worldPresetType.GetProperty("RainfallModifier");
                    var temperatureProperty = worldPresetType.GetProperty("Temperature");
                    var populationProperty = worldPresetType.GetProperty("Population");

                    foreach (var preset in validPresets)
                    {
                        var presetName = nameProperty.GetValue(preset) as string;
                        if (presetName != null)
                        {
                            FloatMenuOption item = new FloatMenuOption(
                                presetName.Translate(),
                                delegate
                                {
                                    settings.realisticPlanetsWorldType = presetName;

                                    if (worldTypeProperty != null)
                                    {
                                        var worldTypeEnum = worldTypeProperty.GetValue(preset);
                                        settings.realisticPlanetsOceanType = Convert.ToInt32(worldTypeEnum);
                                    }

                                    if (axialTiltProperty != null)
                                    {
                                        var axialTiltEnum = axialTiltProperty.GetValue(preset);
                                        settings.realisticPlanetsAxialTilt = Convert.ToInt32(axialTiltEnum);
                                    }

                                    if (rainfallModifierProperty != null)
                                    {
                                        var rainfallEnum = rainfallModifierProperty.GetValue(preset);
                                        settings.rainfall = Convert.ToInt32(rainfallEnum);
                                    }

                                    if (temperatureProperty != null)
                                    {
                                        var temperatureEnum = temperatureProperty.GetValue(preset);
                                        settings.temperature = Convert.ToInt32(temperatureEnum);
                                    }

                                    if (populationProperty != null)
                                    {
                                        var populationEnum = populationProperty.GetValue(preset);
                                        settings.population = Convert.ToInt32(populationEnum);
                                    }
                                }
                            );
                            floatMenuOptions.Add(item);
                        }
                    }
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RandomStart] [RealisticPlanetsCompat] Failed in DoWorldTypeSelectionButton: {ex.Message}");
            }
        }

        public static void ApplyWorldSettings(int subdivisions, int oceanType, int axialTilt, string worldTypeName)
        {
            Api.TryRun(() =>
            {
                if (!IsAvailable || planetsGameComponentType == null || worldTypeType == null || axialTiltType == null)
                    throw new InvalidOperationException("Realistic Planets world settings are unavailable.");

                // Resolve the whole contract before changing another mod's state.
                FieldInfo subcount = OptionalModApi.RequireField(planetsGameComponentType, "subcount", true, typeof(int));
                FieldInfo preset = OptionalModApi.RequireField(planetsGameComponentType, "worldPreset", true, typeof(string));
                FieldInfo worldType = OptionalModApi.RequireField(planetsGameComponentType, "worldType", true, worldTypeType);
                FieldInfo tilt = OptionalModApi.RequireField(planetsGameComponentType, "axialTilt", true, axialTiltType);
                object oceanValue = Enum.ToObject(worldTypeType, oceanType);
                object tiltValue = Enum.ToObject(axialTiltType, axialTilt);

                subcount.SetValue(null, subdivisions);
                preset.SetValue(null, worldTypeName);
                worldType.SetValue(null, oceanValue);
                tilt.SetValue(null, tiltValue);
            });
        }
    }
}
