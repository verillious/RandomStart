using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RandomStartMod.Compat
{
    public static class VECoreCompat
    {
        public static void SetupForKCSG()
        {
            Util.LogMessage("[VECoreCompat] Setting up for KCSG AddStartingStructure");
            if (Current.Game.Scenario.AllParts.ToList().Any((ScenPart s) => s.def.defName == "VFEC_AddStartingStructure"))
            {
                KCSG.PrepareCarefully_Util.pcScenariosSave.Clear();
                KCSG.ScenPart_AddStartingStructure scenPart_AddStartingStructure = (KCSG.ScenPart_AddStartingStructure)Current.Game.Scenario.AllParts.ToList().Find((ScenPart s) => s.def.defName == "VFEC_AddStartingStructure");
                List<KCSG.StructureLayoutDef> chooseFrom = scenPart_AddStartingStructure.chooseFrom;
                if (chooseFrom != null && chooseFrom.Count > 0)
                {
                    KCSG.PrepareCarefully_Util.pcScenariosSave.Add(scenPart_AddStartingStructure.chooseFrom.RandomElement(), scenPart_AddStartingStructure.nearMapCenter);
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

            if (Current.Game.Scenario.AllParts.Count((ScenPart s) => s.def.defName == "VFEE_SpawnRaid") > 0)
            {
                Util.LogMessage("[VFEECompat] Adding Mandatory Empire, Deserter factions");
                FactionDef empireFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First((FactionDef f) => f.defName == "Empire");
                if (empireFactionDef != null)
                {
                    factions.Add(empireFactionDef);
                }
                FactionDef deserterFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First((FactionDef f) => f.defName == "VFEE_Deserters");
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

            if (Current.Game.Scenario.AllParts.Count((ScenPart s) => s.def.defName == "VFED_StartDeserting") > 0)
            {
                Util.LogMessage("[VFEDCompat] Adding Mandatory Deserter, Empire factions");
                FactionDef empireFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First((FactionDef f) => f.defName == "Empire");
                if (empireFactionDef != null)
                {
                    factions.Add(empireFactionDef);
                }
                FactionDef deserterFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First((FactionDef f) => f.defName == "VFEE_Deserters");
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
            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();
            settings.myLittlePlanetSubcount = Mathf.RoundToInt(Widgets.HorizontalSlider(rect, settings.myLittlePlanetSubcount, 6f, 10f, middleAlignment: true, null, "MLPWorldTiny".Translate(), "MLPWorldDefault".Translate(), 1f));
            PlanetLayerSettingsDefOf.Surface.settings.subdivisions = settings.myLittlePlanetSubcount;
        }
    }

    public static class RealisticPlanetsCompat
    {
        public static void DoWorldTypeSelectionButton(Listing_Standard listingStandard)
        {
            var array = Planets_Code.Presets.WorldPresetUtility.WorldPresets.Where((Planets_Code.Presets.WorldPreset p) => p.Name != "Planets.Custom").ToList();
            List<string> list = Planets_Code.Presets.WorldPresetUtility.WorldPresets.Where((Planets_Code.Presets.WorldPreset p) => p.Name != "Planets.Custom").Select(w => w.Name).ToList();
            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();

            string worldTypeName = settings.realisticPlanetsWorldType;
            if (!list.Contains(worldTypeName))
            {
                worldTypeName = "Planets.Vanilla";
                settings.realisticPlanetsWorldType = worldTypeName;
            }
            if (listingStandard.ButtonText(worldTypeName))
            {
                List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                foreach (Planets_Code.Presets.WorldPreset worldPreset in array)
                {
                    FloatMenuOption item = new FloatMenuOption(worldPreset.Name.Translate(), delegate
                    {
                        settings.realisticPlanetsWorldType = worldPreset.Name;
                        settings.realisticPlanetsOceanType = (int)worldPreset.WorldType;
                        settings.realisticPlanetsAxialTilt = (int)worldPreset.AxialTilt;
                        settings.rainfall = (int)worldPreset.RainfallModifier;
                        settings.temperature = (int)worldPreset.Temperature;
                        settings.population = (int)worldPreset.Population;
                    });
                    floatMenuOptions.Add(item);
                }
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
        }

        public static void GenerateRealisticPlanetWorld(float planetCoverage, string seedString, OverallRainfall overallRainfall, OverallTemperature overallTemperature, OverallPopulation population, LandmarkDensity landmarkDensity, List<FactionDef> factions = null, float pollution = 0f, int oceanType = 3, int axialTilt = 2, string worldTypeName = "Planets.Vanilla", bool randomiseWorld = false)
        {
            Util.LogMessage("[RealisticPlanetsCompat] Set Realistic Planets Start");
            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();
            RealisticPlanetsCreateWorldParams planet = new RealisticPlanetsCreateWorldParams();
            planet.PreOpen();

            if (ModsConfig.IsActive("Oblitus.MyLittlePlanet") || ModsConfig.IsActive("Oblitus.MyLittlePlanet_Steam"))
            {
                Util.LogMessage("[RealisticPlanetsCompat] Set My Little Planet");
                Planets_Code.Core.Planets_GameComponent.subcount = settings.myLittlePlanetSubcount;
            }
            else
            {
                Planets_Code.Core.Planets_GameComponent.subcount = 10;
            }

            if (randomiseWorld)
            {
                planet.Randomize();
            }
            else
            {
                Planets_Code.Core.Planets_GameComponent.worldPreset = worldTypeName;
                Planets_Code.Core.Planets_GameComponent.worldType = (Planets_Code.WorldGen.WorldType)oceanType;
                Planets_Code.Core.Planets_GameComponent.axialTilt = (Planets_Code.WorldGen.AxialTilt)axialTilt;
                planet.RainfallMod = (Planets_Code.WorldGen.RainfallModifier)overallRainfall;
                planet.temperature = overallTemperature;
                planet.population = population;
                planet.pollution = pollution;
            }

            planet.seedString = seedString;
            planet.planetCoverage = planetCoverage;
            planet.landmarkDensity = landmarkDensity;
            planet.factions = factions;

            Util.LogMessage("[RealisticPlanetsCompat] Set Realistic Planets End");
            planet.GenerateWorld();

            Type longEventHandlerType = typeof(LongEventHandler);
            var eventQueueField = longEventHandlerType.GetField("eventQueue", BindingFlags.Static | BindingFlags.NonPublic);
            var actionField = longEventHandlerType.GetNestedType("QueuedLongEvent", BindingFlags.NonPublic).GetField("eventAction", BindingFlags.Instance | BindingFlags.Public);

            object eventQueue = eventQueueField.GetValue(null);
            IEnumerable queueItems = (IEnumerable)eventQueue;

            Util.LogMessage("[RealisticPlanetsCompat] Invoke EventQueue Generating Realistic Planets World");
            foreach (object queuedEvent in queueItems)
            {
                Action eventAction = (Action)actionField.GetValue(queuedEvent);
                eventAction?.Invoke();
            }

            LongEventHandler.ClearQueuedEvents();
            planet = null;
        }

        public class RealisticPlanetsCreateWorldParams : Planets_Code.Core.Planets_CreateWorldParams
        {
            public void GenerateWorld()
            {
                base.CanDoNext();
            }

        }
    }

    public static class RealRuinsCompat
    {
        public static void CreatePOIs()
        {
            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();

            Util.LogMessage("[RealRuinsCompat] Generating POIs");
            RealRuins.Page_PlanetaryRuinsLoader page = new RealRuins.Page_PlanetaryRuinsLoader();
            page.mode = RealRuins.RuinsPageMode.FullAuto;
            page.downloadLimit = RealRuins.RealRuins_ModSettings.planetaryRuinsOptions.downloadLimit;
            page.transferLimit = RealRuins.RealRuins_ModSettings.planetaryRuinsOptions.transferLimit;
            page.abandonedPercentage = (int)RealRuins.RealRuins_ModSettings.planetaryRuinsOptions.abandonedLocations;
            page.aggressiveDiscard = RealRuins.RealRuins_ModSettings.planetaryRuinsOptions.excludePlainRuins;
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
            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();
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
                prepareModeratelyAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "PrepareModerately");
                
                if (prepareModeratelyAssembly != null)
                {
                    pawnFilterType = prepareModeratelyAssembly.GetType("Lakuna.PrepareModerately.Filter.PawnFilter");
                    pawnFilterListerType = prepareModeratelyAssembly.GetType("Lakuna.PrepareModerately.Filter.PawnFilterLister");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RandomStart] Failed to initialize PrepareModerately compatibility: {ex.Message}");
            }
        }

        public static bool IsAvailable => prepareModeratelyAssembly != null && pawnFilterType != null;

        public static void SetMod()
        {
            try
            {
                Lakuna.PrepareModerately.Patches.PagePatch.Instance = new Page_ConfigureStartingPawns();
                Lakuna.PrepareModerately.Patches.RandomizePatch.IsActivelyRolling = true;
            }
            catch (Exception ex)
            {
                Log.Error($"[RandomStart] Failed to set PrepareModerately mod state: {ex.Message}");
            }
        }

        public static List<string> GetAvailableFilterNames()
        {
            if (!IsAvailable || pawnFilterListerType == null)
                return new List<string>();

            try
            {
                var allMethod = pawnFilterListerType.GetMethod("All", BindingFlags.Public | BindingFlags.Static);
                if (allMethod == null) return new List<string>();

                var filters = allMethod.Invoke(null, null) as IEnumerable;
                var filterNames = new List<string>();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        var nameProperty = pawnFilterType.GetProperty("Name");
                        if (nameProperty?.GetValue(filter) is string name && !string.IsNullOrEmpty(name))
                        {
                            filterNames.Add(name);
                        }
                    }
                }

                return filterNames;
            }
            catch (Exception ex)
            {
                Log.Warning($"[RandomStart] Failed to get available filter names: {ex.Message}");
                return new List<string>();
            }
        }

        public static object GetFilterByName(string filterName)
        {
            if (!IsAvailable || string.IsNullOrEmpty(filterName))
                return null;

            try
            {
                var allMethod = pawnFilterListerType.GetMethod("All", BindingFlags.Public | BindingFlags.Static);
                if (allMethod == null) return null;

                var filters = allMethod.Invoke(null, null) as IEnumerable;
                if (filters == null) return null;

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
                Log.Warning($"[RandomStart] Failed to get filter by name '{filterName}': {ex.Message}");
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
                    if (descriptionProperty?.GetValue(filter) is string description && !string.IsNullOrEmpty(description))
                    {
                        return description;
                    }

                    // Fallback to Summary property if Description is empty
                    var summaryProperty = pawnFilterType.GetProperty("Summary");
                    if (summaryProperty?.GetValue(filter) is string summary && !string.IsNullOrEmpty(summary))
                    {
                        return summary;
                    }

                    // If both are empty, return a default message
                    return "No description available";
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RandomStart] Failed to get filter description for '{filterName}': {ex.Message}");
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
                    var currentProperty = pawnFilterType.GetProperty("Current", BindingFlags.Public | BindingFlags.Static);
                    currentProperty?.SetValue(null, filter);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RandomStart] Failed to set current filter to '{filterName}': {ex.Message}");
            }
        }

        public static string GetCurrentFilterName()
        {
            if (!IsAvailable)
                return "";

            try
            {
                var currentProperty = pawnFilterType.GetProperty("Current", BindingFlags.Public | BindingFlags.Static);
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
    }
}
