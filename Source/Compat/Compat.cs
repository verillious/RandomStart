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

            if (settings.realisticPlanetsUseWordType && Rand.Range(0f, 1f) < settings.realisticPlanetsUseWordTypeChance)
            {
                Planets_Code.Presets.WorldPreset[] array = Planets_Code.Presets.WorldPresetUtility.WorldPresets.Where((Planets_Code.Presets.WorldPreset p) => p.Name != "Planets.Custom").ToArray();
                Planets_Code.Presets.WorldPreset worldPreset = array[Rand.Range(0, array.Length)];
                Util.LogMessage($"[RealisticPlanetsCompat] Using WorldPreset: {worldPreset.Name}");
                Planets_Code.Core.Planets_GameComponent.worldPreset = worldPreset.Name;
                Planets_Code.Core.Planets_GameComponent.worldType = worldPreset.WorldType;
                Planets_Code.Core.Planets_GameComponent.axialTilt = worldPreset.AxialTilt;
                planet.RainfallMod = worldPreset.RainfallModifier;
                planet.temperature = worldPreset.Temperature;
                planet.population = worldPreset.Population;
                planet.pollution = pollution;
            }
            else if (randomiseWorld)
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
        public static void SetMod()
        {
            Lakuna.PrepareModerately.Patches.PagePatch.Instance = new Page_ConfigureStartingPawns();
            Lakuna.PrepareModerately.Patches.RandomizePatch.IsActivelyRolling = true;
        }
    }
}
