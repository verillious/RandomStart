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
        public static void SetupForKCSG()
        {
            Util.LogMessage("[VECoreCompat] Setting up for KCSG AddStartingStructure");
            if (
                Current
                    .Game.Scenario.AllParts.ToList()
                    .Any((ScenPart s) => s.def.defName == "VFEC_AddStartingStructure")
            )
            {
                KCSG.PrepareCarefully_Util.pcScenariosSave.Clear();
                KCSG.ScenPart_AddStartingStructure scenPart_AddStartingStructure =
                    (KCSG.ScenPart_AddStartingStructure)
                        Current
                            .Game.Scenario.AllParts.ToList()
                            .Find((ScenPart s) => s.def.defName == "VFEC_AddStartingStructure");
                List<KCSG.StructureLayoutDef> chooseFrom = scenPart_AddStartingStructure.chooseFrom;
                if (chooseFrom != null && chooseFrom.Count > 0)
                {
                    KCSG.PrepareCarefully_Util.pcScenariosSave.Add(
                        scenPart_AddStartingStructure.chooseFrom.RandomElement(),
                        scenPart_AddStartingStructure.nearMapCenter
                    );
                }
            }
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
        public static void SetupForStartInSpace()
        {
            Util.LogMessage("[SOS2Compat] Running DoEarlyInit on SOS2 ScenParts");
            foreach (ScenPart part in Find.Scenario.AllParts)
            {
                if (part is SaveOurShip2.ScenPart_LoadShip p && p.HasValidFilename())
                {
                    p.DoEarlyInit();
                }
                else if (part is SaveOurShip2.ScenPart_StartInSpace s)
                {
                    s.DoEarlyInit();
                }
            }
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
        public static void CreatePOIs()
        {
            RandomStartSettings settings = LoadedModManager
                .GetMod<RandomStartMod>()
                .GetSettings<RandomStartSettings>();

            Util.LogMessage("[RealRuinsCompat] Generating POIs");
            RealRuins.Page_PlanetaryRuinsLoader page = new RealRuins.Page_PlanetaryRuinsLoader();
            page.mode = RealRuins.RuinsPageMode.FullAuto;
            page.downloadLimit = RealRuins
                .RealRuins_ModSettings
                .planetaryRuinsOptions
                .downloadLimit;
            page.transferLimit = RealRuins
                .RealRuins_ModSettings
                .planetaryRuinsOptions
                .transferLimit;
            page.abandonedPercentage = (int)
                RealRuins.RealRuins_ModSettings.planetaryRuinsOptions.abandonedLocations;
            page.aggressiveDiscard = RealRuins
                .RealRuins_ModSettings
                .planetaryRuinsOptions
                .excludePlainRuins;
            page.biomeStrict = settings.realRuinsBiomeFilter;
            page.StartLoadingList();
            page.Close(false);
        }
    }

    public static class NoPauseCompat
    {
        public static void SetupForNoPause()
        {
            Util.LogMessage("[NoPauseCompat] Setting up for no pause challenge");
            RandomStartSettings settings = LoadedModManager
                .GetMod<RandomStartMod>()
                .GetSettings<RandomStartSettings>();
            NoPauseChallenge.Main.noPauseEnabled = settings.noPauseEnabled;
            NoPauseChallenge.Main.halfSpeedActive = settings.noPauseHalfSpeedEnabled;
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
