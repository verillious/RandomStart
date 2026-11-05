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
    public static class VECoreCompat
    {
        private static readonly OptionalModApi Api = new OptionalModApi("Vanilla Expanded Framework", message => Log.Warning(message));

        public static void SetupForKCSG()
        {
            ScenPart part = Current.Game.Scenario.AllParts.FirstOrDefault(
                candidate => candidate.def.defName == "VFEC_AddStartingStructure");
            if (part == null)
                return;

            bool initialized = Api.TryRun(() =>
            {
                Type utility = OptionalModApi.RequireType("KCSG.PrepareCarefully_Util");
                FieldInfo cacheField = OptionalModApi.RequireField(utility, "pcScenariosSave", true, typeof(IDictionary));
                FieldInfo choicesField = OptionalModApi.RequireField(part.GetType(), "chooseFrom", false, typeof(IList));
                FieldInfo nearCenterField = OptionalModApi.RequireField(part.GetType(), "nearMapCenter", false, typeof(bool));
                var cache = (IDictionary)cacheField.GetValue(null);
                var choices = (IList)choicesField.GetValue(part);
                bool nearCenter = (bool)nearCenterField.GetValue(part);
                if (cache == null)
                    throw new InvalidOperationException("KCSG scenario cache is not initialized.");

                // Resolve and validate all members before changing the external cache.
                object chosen = choices != null && choices.Count > 0
                    ? choices[Rand.Range(0, choices.Count)] : null;
                cache.Clear();
                if (chosen != null)
                    cache.Add(chosen, nearCenter);
            });
            if (!initialized)
                throw new InvalidOperationException("Random Start cannot initialize this starting-structure scenario with the installed Vanilla Expanded Framework. Choose another scenario or update the integration.");
        }
    }

    public static class VFEECompat
    {
        public static void EnsureScenarioFactions(List<FactionDef> factions)
        {
            if (factions.ContainsAny((FactionDef f) => f.defName == "VFEE_Deserters"))
                return;

            if (
                Current.Game.Scenario.AllParts.Count(
                    (ScenPart s) => s.def.defName == "VFEE_SpawnRaid"
                ) > 0
            )
            {
                Util.LogMessage("[VFEECompat] Adding Mandatory Empire, Deserter factions");
                FactionDef empireFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First(
                    (FactionDef f) => f.defName == "Empire"
                );
                if (empireFactionDef != null)
                {
                    factions.Add(empireFactionDef);
                }
                FactionDef deserterFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First(
                    (FactionDef f) => f.defName == "VFEE_Deserters"
                );
                if (deserterFactionDef != null)
                {
                    factions.Add(deserterFactionDef);
                }
            }
        }
    }

    public static class VFEDCompat
    {
        public static void EnsureScenarioFactions(List<FactionDef> factions)
        {
            if (factions.ContainsAny((FactionDef f) => f.defName == "VFEE_Deserters"))
                return;

            if (
                Current.Game.Scenario.AllParts.Count(
                    (ScenPart s) => s.def.defName == "VFED_StartDeserting"
                ) > 0
            )
            {
                Util.LogMessage("[VFEDCompat] Adding Mandatory Deserter, Empire factions");
                FactionDef empireFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First(
                    (FactionDef f) => f.defName == "Empire"
                );
                if (empireFactionDef != null)
                {
                    factions.Add(empireFactionDef);
                }
                FactionDef deserterFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First(
                    (FactionDef f) => f.defName == "VFEE_Deserters"
                );
                if (deserterFactionDef != null)
                {
                    factions.Add(deserterFactionDef);
                }
            }
        }
    }

    public static class SOS2Compat
    {
        private static readonly OptionalModApi Api = new OptionalModApi("Save Our Ship 2", message => Log.Warning(message));

        public static void SetupForStartInSpace()
        {
            // Ordinary scenarios can continue without SOS2 hooks; a space-start
            // scenario must not proceed in a partially initialized state.
            bool requiresInitialization = Find.Scenario.AllParts.Any(part =>
                IsSpaceStart(part.GetType()));
            if (!requiresInitialization)
                return;

            bool initialized = Api.TryRun(() =>
            {
                Type loadShip = OptionalModApi.RequireType("SaveOurShip2.ScenPart_LoadShip");
                Type startInSpace = OptionalModApi.RequireType("SaveOurShip2.ScenPart_StartInSpace");
                MethodInfo hasFilename = OptionalModApi.RequireMethod(loadShip, "HasValidFilename", typeof(bool));
                MethodInfo loadInit = OptionalModApi.RequireMethod(loadShip, "DoEarlyInit", typeof(void));
                MethodInfo spaceInit = OptionalModApi.RequireMethod(startInSpace, "DoEarlyInit", typeof(void));
                foreach (ScenPart part in Find.Scenario.AllParts)
                {
                    if (loadShip.IsInstanceOfType(part) && (bool)hasFilename.Invoke(part, null))
                        loadInit.Invoke(part, null);
                    else if (startInSpace.IsInstanceOfType(part))
                        spaceInit.Invoke(part, null);
                }
            });
            if (!initialized)
                throw new InvalidOperationException("Random Start cannot initialize this space-start scenario with the installed Save Our Ship 2. Choose another scenario or update the integration.");
        }

        private static bool IsSpaceStart(Type type)
        {
            for (; type != null; type = type.BaseType)
                if (type.FullName == "SaveOurShip2.ScenPart_LoadShip"
                    || type.FullName == "SaveOurShip2.ScenPart_StartInSpace")
                    return true;
            return false;
        }
    }

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

    public static class RealisticPlanetsCompat
    {
        private static Assembly realisticPlanetsAssembly;
        private static Type worldPresetUtilityType;
        private static Type worldPresetType;
        private static Type planetsGameComponentType;
        private static Type worldTypeType;
        private static Type axialTiltType;
        private static Type rainfallModifierType;
        private static Type planetsCreateWorldParamsType;
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
                    rainfallModifierType = realisticPlanetsAssembly.GetType("Planets_Code.WorldGen.RainfallModifier");
                    planetsCreateWorldParamsType = realisticPlanetsAssembly.GetType("Planets_Code.Core.Planets_CreateWorldParams");
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
            planetsCreateWorldParamsType != null;

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

        public static void GenerateRealisticPlanetWorld(
            float planetCoverage,
            string seedString,
            OverallRainfall overallRainfall,
            OverallTemperature overallTemperature,
            OverallPopulation population,
            LandmarkDensity landmarkDensity,
            List<FactionDef> factions = null,
            float pollution = 0f,
            int oceanType = 3,
            int axialTilt = 2,
            string worldTypeName = "Planets.Vanilla",
            bool randomiseWorld = false
        )
        {
            if (!IsAvailable)
            {
                Log.Warning("[RandomStart] [RealisticPlanetsCompat] Realistic Planets not available, falling back to standard world generation");
                // Fall back to standard RimWorld world generation (synchronous)
                Current.Game.World = WorldGenerator.GenerateWorld(planetCoverage, seedString, overallRainfall, overallTemperature, population, landmarkDensity, factions, pollution);
                return;
            }

            try
            {
                Util.LogMessage("[RealisticPlanetsCompat] Starting Realistic Planets world generation");
                RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();

                // Handle My Little Planet compatibility
                if (ModsConfig.IsActive("Oblitus.MyLittlePlanet") || ModsConfig.IsActive("Oblitus.MyLittlePlanet_Steam"))
                {
                    Util.LogMessage("[RealisticPlanetsCompat] Set My Little Planet");
                    var subcountField = planetsGameComponentType.GetField("subcount", BindingFlags.Public | BindingFlags.Static);
                    subcountField?.SetValue(null, settings.myLittlePlanetSubcount);
                }
                else
                {
                    var subcountField = planetsGameComponentType.GetField("subcount", BindingFlags.Public | BindingFlags.Static);
                    subcountField?.SetValue(null, 10);
                }

                // Set up world generation parameters
                var worldPresetField = planetsGameComponentType.GetField("worldPreset", BindingFlags.Public | BindingFlags.Static);
                worldPresetField?.SetValue(null, worldTypeName);

                if (worldTypeType != null)
                {
                    var worldTypeField = planetsGameComponentType.GetField("worldType", BindingFlags.Public | BindingFlags.Static);
                    var worldTypeValue = Enum.ToObject(worldTypeType, oceanType);
                    worldTypeField?.SetValue(null, worldTypeValue);
                }

                if (axialTiltType != null)
                {
                    var axialTiltField = planetsGameComponentType.GetField("axialTilt", BindingFlags.Public | BindingFlags.Static);
                    var axialTiltValue = Enum.ToObject(axialTiltType, axialTilt);
                    axialTiltField?.SetValue(null, axialTiltValue);
                }

                Util.LogMessage("[RealisticPlanetsCompat] Parameters configured, generating world synchronously");
                
                // Generate world synchronously - Realistic Planets should hook into this process
                Current.Game.World = WorldGenerator.GenerateWorld(planetCoverage, seedString, overallRainfall, overallTemperature, population, landmarkDensity, factions, pollution);
                
                Util.LogMessage("[RealisticPlanetsCompat] World generation completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"[RandomStart] [RealisticPlanetsCompat] Failed to generate realistic planet world: {ex.Message}\nStack trace: {ex.StackTrace}");
                // Fall back to standard world generation (synchronous)
                Current.Game.World = WorldGenerator.GenerateWorld(planetCoverage, seedString, overallRainfall, overallTemperature, population, landmarkDensity, factions, pollution);
            }
        }
    }

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

    public static class PrepareModeratelyCompat
    {
        private static Type pawnFilterType;
        private static Type pawnFilterListerType;
        private static Assembly prepareModeratelyAssembly;

        static PrepareModeratelyCompat()
        {
            try
            {
                prepareModeratelyAssembly = AppDomain
                    .CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "PrepareModerately");

                if (prepareModeratelyAssembly != null)
                {
                    pawnFilterType = prepareModeratelyAssembly.GetType(
                        "Lakuna.PrepareModerately.Filter.PawnFilter"
                    );
                    pawnFilterListerType = prepareModeratelyAssembly.GetType(
                        "Lakuna.PrepareModerately.Filter.PawnFilterLister"
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] [PrepareModeratelyCompat] Failed to initialize PrepareModerately compatibility: {ex.Message}"
                );
            }
        }

        public static bool IsAvailable =>
            prepareModeratelyAssembly != null && pawnFilterType != null;

        public static List<string> GetAvailableFilterNames()
        {
            if (!IsAvailable || pawnFilterListerType == null)
                return new List<string>();

            try
            {
                var allMethod = pawnFilterListerType.GetMethod(
                    "All",
                    BindingFlags.Public | BindingFlags.Static
                );
                if (allMethod == null)
                    return new List<string>();

                var filters = allMethod.Invoke(null, null) as IEnumerable;
                var filterNames = new List<string>();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        var nameProperty = pawnFilterType.GetProperty("Name");
                        if (
                            nameProperty?.GetValue(filter) is string name
                            && !string.IsNullOrEmpty(name)
                        )
                        {
                            filterNames.Add(name);
                        }
                    }
                }

                return filterNames;
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] [PrepareModeratelyCompat] Failed to get available filter names: {ex.Message}"
                );
                return new List<string>();
            }
        }

        public static object GetFilterByName(string filterName)
        {
            if (!IsAvailable || string.IsNullOrEmpty(filterName))
                return null;

            try
            {
                var allMethod = pawnFilterListerType.GetMethod(
                    "All",
                    BindingFlags.Public | BindingFlags.Static
                );
                if (allMethod == null)
                    return null;

                var filters = allMethod.Invoke(null, null) as IEnumerable;
                if (filters == null)
                    return null;

                foreach (var filter in filters)
                {
                    var nameProperty = pawnFilterType.GetProperty("Name");
                    if (nameProperty?.GetValue(filter) is string name && name == filterName)
                    {
                        return filter;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] [PrepareModeratelyCompat] Failed to get filter by name '{filterName}': {ex.Message}"
                );
            }

            return null;
        }

        public static string GetFilterDescription(string filterName)
        {
            if (!IsAvailable || string.IsNullOrEmpty(filterName))
                return "";

            try
            {
                var filter = GetFilterByName(filterName);
                if (filter != null)
                {
                    // Try to get Description property first
                    var descriptionProperty = pawnFilterType.GetProperty("Description");
                    if (
                        descriptionProperty?.GetValue(filter) is string description
                        && !string.IsNullOrEmpty(description)
                    )
                    {
                        return description;
                    }

                    // Fallback to Summary property if Description is empty
                    var summaryProperty = pawnFilterType.GetProperty("Summary");
                    if (
                        summaryProperty?.GetValue(filter) is string summary
                        && !string.IsNullOrEmpty(summary)
                    )
                    {
                        return summary;
                    }

                    // If both are empty, return a default message
                    return "No description available";
                }
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] [PrepareModeratelyCompat] Failed to get filter description for '{filterName}': {ex.Message}"
                );
            }

            return "";
        }

        public static void SetCurrentFilter(string filterName)
        {
            if (!IsAvailable || string.IsNullOrEmpty(filterName))
                return;

            try
            {
                var filter = GetFilterByName(filterName);
                if (filter != null)
                {
                    var currentProperty = pawnFilterType.GetProperty(
                        "Current",
                        BindingFlags.Public | BindingFlags.Static
                    );
                    currentProperty?.SetValue(null, filter);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] Failed to set current filter to '{filterName}': {ex.Message}"
                );
            }
        }

        public static string GetCurrentFilterName()
        {
            if (!IsAvailable)
                return "";

            try
            {
                var currentProperty = pawnFilterType.GetProperty(
                    "Current",
                    BindingFlags.Public | BindingFlags.Static
                );
                var currentFilter = currentProperty?.GetValue(null);

                if (currentFilter != null)
                {
                    var nameProperty = pawnFilterType.GetProperty("Name");
                    return nameProperty?.GetValue(currentFilter) as string ?? "";
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RandomStart] Failed to get current filter name: {ex.Message}");
            }

            return "";
        }

        public static void RandomiseStartingPawns()
        {
            if (!IsAvailable)
                return;

            try
            {
                // Check if Find.GameInitData exists
                if (Find.GameInitData == null)
                {
                    Log.Warning(
                        "[RandomStart] Find.GameInitData is null, cannot randomise starting pawns"
                    );
                    return;
                }

                // Check if startingAndOptionalPawns exists
                if (Find.GameInitData.startingAndOptionalPawns == null)
                {
                    Log.Warning(
                        "[RandomStart] Find.GameInitData.startingAndOptionalPawns is null, cannot randomise starting pawns"
                    );
                    return;
                }

                // Get the current filter using reflection
                var currentProperty = pawnFilterType.GetProperty(
                    "Current",
                    BindingFlags.Public | BindingFlags.Static
                );

                if (currentProperty == null)
                {
                    Log.Warning("[RandomStart] Could not find Current property on PawnFilter type");
                    return;
                }

                var currentFilter = currentProperty.GetValue(null);

                if (currentFilter == null)
                {
                    Log.Warning("[RandomStart] PrepareModerately current filter is null");
                    return;
                }
                else
                {
                    var nameProperty = pawnFilterType.GetProperty("Name");
                    var filterName = nameProperty?.GetValue(currentFilter) as string ?? "Unknown";
                    Util.LogMessage($"Using PrepareModerately filter: {filterName}");
                }

                // Get the Matches method from the current filter
                var matchesMethod = pawnFilterType.GetMethod(
                    "Matches",
                    new Type[] { typeof(Pawn) }
                );

                if (matchesMethod == null)
                {
                    Log.Warning("[RandomStart] Could not find Matches method on PawnFilter type");
                    return;
                }

                Util.LogMessage(
                    $"Starting pawn randomization with {Find.GameInitData.startingAndOptionalPawns.Count} pawns"
                );

                for (int i = 0; i < Find.GameInitData.startingAndOptionalPawns.Count; i++)
                {
                    int attemptCount = 0;
                    while (
                        (bool)
                            matchesMethod.Invoke(
                                currentFilter,
                                new object[] { Find.GameInitData.startingAndOptionalPawns[i] }
                            ) == false
                    )
                    {
                        StartingPawnUtility.RegenerateStartingPawnInPlace(i);
                        attemptCount++;
                        if (attemptCount >= 250)
                        {
                            Log.Warning(
                                $"[RandomStart] Pawn at index {i} did not match filter after 250 attempts, moving to next pawn"
                            );
                            break;
                        }
                    }
                }

                Util.LogMessage("Completed pawn randomization with PrepareModerately filter");
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] Failed to randomise starting pawns with PrepareModerately filter: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
            }
        }
    }
}
