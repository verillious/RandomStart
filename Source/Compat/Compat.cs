using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
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
            WorldGenRules.RulesOverrider.subcount = settings.myLittlePlanetSubcount;
        }
    }

    public static class RealisticPlanetsCompat
    {
        public static void DoWorldTypeSelectionButton(Listing_Standard listingStandard)
        {
            List<string> worldTypeNames = new List<string>() {
            "Barren",
            "Very Dry",
            "Dry",
            "Vanilla",
            "Earthlike",
            "Islands",
            "Waterworld"
        };
            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();

            listingStandard.Label("Planets.WorldPresets".Translate());
            int currentWorldType = settings.realisticPlanetsWorldType;

            if (currentWorldType >= worldTypeNames.Count)
            {
                currentWorldType = 3;
                settings.realisticPlanetsWorldType = 3;
            }

            if (listingStandard.ButtonText(worldTypeNames[currentWorldType]))
            {
                List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                for (int i = 0; i < worldTypeNames.Count; i++)
                {
                    int worldType = i;
                    FloatMenuOption floatMenuOption = new FloatMenuOption(worldTypeNames[i], delegate
                    {
                        settings.realisticPlanetsWorldType = worldType;
                    });
                    floatMenuOptions.Add(floatMenuOption);
                }
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
        }

        public static void GenerateRealisticPlanetWorld(float planetCoverage, string seedString, OverallRainfall overallRainfall, OverallTemperature overallTemperature, OverallPopulation population, List<FactionDef> factions = null, float pollution = 0f, int worldType = -1)
        {
            Util.LogMessage("[RealisticPlanetsCompat] Generating Realistic Planets World");
            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();

            if (ModsConfig.IsActive("Oblitus.MyLittlePlanet"))
            {
                Util.LogMessage("[RealisticPlanetsCompat] My Little Planet is running");
                Planets_Code.Planets_GameComponent.subcount = settings.myLittlePlanetSubcount;
            }
            else
            {
                Planets_Code.Planets_GameComponent.subcount = 10;
            }

            Planets_Code.Planets_GameComponent.axialTilt = Planets_Code.Planets_Random.GetRandomAxialTilt();
            if (worldType != -1)
            {
                Planets_Code.Planets_GameComponent.worldType = (Planets_Code.WorldType)worldType;
            }
            else
            {
                Planets_Code.Planets_GameComponent.worldType = Planets_Code.Planets_Random.GetRandomWorldType();
            }
            Planets_Code.Controller.Settings.randomPlanet = true;

            Planets_Code.RainfallModifier rainfallMod = Planets_Code.RainfallModifier.Little;

            if (overallRainfall == OverallRainfall.LittleBitLess)
                rainfallMod = Planets_Code.RainfallModifier.LittleBitLess;

            if (overallRainfall == OverallRainfall.Normal)
                rainfallMod = Planets_Code.RainfallModifier.Normal;

            if (overallRainfall == OverallRainfall.LittleBitMore)
                rainfallMod = Planets_Code.RainfallModifier.LittleBitMore;

            if (overallRainfall == OverallRainfall.High)
                rainfallMod = Planets_Code.RainfallModifier.High;

            if (overallRainfall == OverallRainfall.AlmostNone)
                rainfallMod = Planets_Code.RainfallModifier.Little;

            if (overallRainfall == OverallRainfall.VeryHigh)
                rainfallMod = Planets_Code.RainfallModifier.High;

            OverallRainfall rainfall = Planets_Code.RainfallModifierUtility.GetModifiedRainfall(Planets_Code.Planets_GameComponent.worldType, rainfallMod);

            Planets_Code.Planets_TemperatureTuning.SetSeasonalCurve();
            Find.GameInitData.ResetWorldRelatedMapInitData();

            Current.Game.World = WorldGenerator.GenerateWorld(planetCoverage, seedString, rainfall, overallTemperature, population, factions, pollution);

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
}
