using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RandomStartMod
{
    public class RandomStartSettings : ModSettings
    {
        public bool openedSettings = false;

        public string difficulty = "Rough";
        public int mapSize = 250;
        public float planetCoverage = 0.3f;

        public bool permadeath = false;

        public bool randomiseWorldSeed = true;
        public string worldSeed = "village";

        public IntRange randomiseRainfallRange = new IntRange(1, 3);
        public IntRange randomiseTemperatureRange = new IntRange(2, 4);
        public IntRange randomisePopulationRange = new IntRange(2, 4);
        public IntRange randomiseLandmarkDensityRange = new IntRange(2, 4);
        public FloatRange randomisePollutionRange = new FloatRange(0.05f, 0.25f);

        public bool randomiseRainfall = true;
        public bool randomiseTemperature = true;
        public bool randomisePopulation = true;
        public bool randomiseLandmarkDensity = true;
        public bool randomisePollution = true;
        public bool randomiseSeason = true;
        public bool disableIdeo = false;
        public bool overrideIdeo = false;
        public string customIdeoOverrideFile = null;

        public int rainfall = 2;
        public int temperature = 3;
        public int population = 3;
        public int landmarkDensity = 3;
        public float pollution = 0.05f;
        public int startingSeason = 2;

        // Starting Tile Settings
        public bool filterStartingBiome = false;
        public List<string> allowedBiomes = new List<string>();

        public bool filterStartingHilliness = false;
        public List<int> allowedHilliness = new List<int>();

        // Starting Colonist Settings
        public bool startingPawnForceViolence = true;

        public IntRange randomFactionRange = new IntRange(5, 15);
        public int minimumFactions = 5;
        public int maximumFactions = 15;
        public bool uniqueFactions = false;
        public bool randomiseFactionGoodwill = false;
        public List<string> factionsExcludeFromReputationRandomization = new List<string>();

        public bool createRandomScenario = false;
        public bool enableCustomScenarios = true;
        public bool enableSteamWorkshopScenarios = true;

        public List<string> disabledStorytellers = new List<string>();
        public List<string> disabledScenarios = new List<string>() { "tutorial" };
        public List<string> factionsAlwaysAdd = new List<string>()
        {
            "Ancients",
            "AncientsHostile",
            "Mechanoid",
            "Insect",
            "Empire",
            "HoraxCult",
            "Entities",
            "Salvagers",
            "TradersGuild",
            "VFEE_Deserters",
        };
        public List<string> factionsRandomlyAdd = new List<string>()
        {
            "OutlanderCivil",
            "OutlanderRough",
            "TribeCivil",
            "TribeRough",
            "TribeSavage",
            "Pirate",
            "Empire",
            "CannibalPirate",
            "NudistTribe",
            "TribeCannibal",
            "TribeRoughNeanderthal",
            "PirateYttakin",
            "TribeSavageImpid",
            "OutlanderRoughPig",
            "PirateWaster",
        };

        public string anomalyPlaystyle = "Standard";

        // Custom difficulty
        public bool allowBigThreats = true;
        public bool allowIntroThreats = true;
        public bool allowCaveHives = true;
        public bool peacefulTemples;
        public bool allowViolentQuests = true;
        public bool predatorsHuntHumanlikes = true;
        public bool babiesAreHealthy;
        public bool noBabiesOrChildren;
        public bool allowTraps = true;
        public bool allowTurrets = true;
        public bool allowMortars = true;
        public bool classicMortars;
        public bool allowExtremeWeatherIncidents = true;
        public bool fixedWealthMode;
        public bool unwaveringPrisoners = true;
        public bool childRaidersAllowed = false;
        public bool childShamblersAllowed = false;
        public float threatScale = 1f;
        public float scariaRotChance;
        public float colonistMoodOffset;
        public float tradePriceFactorLoss;
        public float cropYieldFactor = 1f;
        public float mineYieldFactor = 1f;
        public float butcherYieldFactor = 1f;
        public float researchSpeedFactor = 1f;
        public float diseaseIntervalFactor = 1f;
        public float enemyReproductionRateFactor = 1f;
        public float playerPawnInfectionChanceFactor = 1f;
        public float manhunterChanceOnDamageFactor = 1f;
        public float deepDrillInfestationChanceFactor = 1f;
        public float wastepackInfestationChanceFactor = 1f;
        public float foodPoisonChanceFactor = 1f;
        public float maintenanceCostFactor = 1f;
        public float enemyDeathOnDownedChanceFactor = 1f;
        public float adaptationGrowthRateFactorOverZero = 1f;
        public float adaptationEffectFactor = 1f;
        public float questRewardValueFactor = 1f;
        public float raidLootPointsFactor = 1f;
        public float lowPopConversionBoost = 3f;
        public float minThreatPointsRangeCeiling = 70f;
        public float childAgingRate = 4f;
        public float adultAgingRate = 1f;
        public float anomalyThreatsInactiveFraction = 0.08f;
        public float anomalyThreatsActiveFraction = 0.3f;
        public float overrideAnomalyThreatsFraction = 0.15f;
        public float studyEfficiencyFactor = 1f;
        public float friendlyFireChanceFactor = 0.4f;
        public float allowInstantKillChance = 0f;
        public float fixedWealthTimeFactor = 1f;

        //Feature Flags
        public bool enableRandomXenotypes = false;
        public bool enableRandomCustomXenotypes = false;
        public IntRange randomGeneRange = new IntRange(5, 15);
        public bool enableMetabolicEfficiencyMinimum = false;
        public int minimumMetabolicEfficiency = 0;
        public bool respectFactionXenotypes = true;
        public bool fluidIdeo = false;
        public IntRange randomMemeRange = new IntRange(1, 4);
        public List<string> forcedMemes = new List<string>();
        public List<string> disallowedMemes = new List<string>();
        public List<string> disallowedPrecepts = new List<string>();
        public bool removeStartingResearch = false;
        public bool addRandomResearch = false;
        public IntRange randomResearchRange = new IntRange(5, 15);
        public int randomResearchTechLevelLimit = 4;
        public bool doRandomResearchPrerequisites = true;
        public bool removeStartingItems = false;
        public bool addRandomItems = false;
        public IntRange randomItemRange = new IntRange(5, 15);
        public int randomItemTechLevelLimit = 4;
        public bool enableMarketValueLimit = false;
        public int randomItemTotalMarketValueLimit = 10000;

        //Compat
        public int myLittlePlanetSubcount = 10;

        public bool realisticPlanetsUseWordType = false;
        public float realisticPlanetsUseWordTypeChance = 0.5f;

        public string realisticPlanetsWorldType = "Planets.Vanilla";
        public bool randomiseRealisticPlanets = false;

        public int realisticPlanetsOceanType = 3;
        public bool randomiseOceanType = true;
        public IntRange randomiseOceanTypeRange = new IntRange(2, 4);

        public int realisticPlanetsAxialTilt = 2;
        public bool randomiseAxialTilt = true;
        public IntRange randomiseAxialTiltRange = new IntRange(1, 3);
        public bool enableAutoRealRuins = true;
        public bool realRuinsBiomeFilter = false;
        public bool noPauseEnabled = true;
        public bool noPauseHalfSpeedEnabled = false;

        public bool randomisePawnName = true;
        public string PawnFirstName;
        public string PawnNickName;
        public string PawnLastName;
        public bool randomisePawnAge = false;
        public IntRange randomisePawnAgeRange = new IntRange(20, 40);
        public bool randomisePawnSex = true;
        public int PawnSex = 0;
        public bool PawnNotDisabledWorkTags = false;

        public bool limitStartingTileTemperature = false;
        public FloatRange limitStartingTileTemperatureRange = new FloatRange(-10f, 30f);

        public override void ExposeData()
        {
            // Ensure lists are properly initialized before serialization
            if (factionsExcludeFromReputationRandomization == null)
                factionsExcludeFromReputationRandomization = new List<string>();

            Scribe_Values.Look(ref openedSettings, "openedSettings", false);
            Scribe_Values.Look(ref difficulty, "difficulty", "Rough");
            Scribe_Values.Look(ref mapSize, "mapSize", 250);
            Scribe_Values.Look(ref planetCoverage, "planetCoverage", 0.3f);
            Scribe_Values.Look(ref permadeath, "permadeath", false);
            Scribe_Values.Look(ref randomiseRainfallRange, "randomiseRainfallRange", new IntRange(2, 4));
            Scribe_Values.Look(ref randomiseTemperatureRange, "randomiseTemperatureRange", new IntRange(2, 4));
            Scribe_Values.Look(ref randomisePopulationRange, "randomisePopulationRange", new IntRange(2, 4));
            Scribe_Values.Look(ref randomisePollutionRange, "randomisePollutionRange", new FloatRange(0.05f, 0.25f));
            Scribe_Values.Look(ref randomiseRainfall, "randomiseRainfall", true);
            Scribe_Values.Look(ref randomiseTemperature, "randomiseTemperature", true);
            Scribe_Values.Look(ref randomisePopulation, "randomisePopulation", true);
            Scribe_Values.Look(ref randomisePollution, "randomisePollution", true);
            Scribe_Values.Look(ref randomiseSeason, "randomiseSeason", false);
            Scribe_Values.Look(ref randomiseWorldSeed, "randomiseWorldSeed", true);
            Scribe_Values.Look(ref worldSeed, "worldSeed", "village");
            Scribe_Values.Look(ref createRandomScenario, "createRandomScenario", false);
            Scribe_Values.Look(ref enableCustomScenarios, "enableCustomScenarios", true);
            Scribe_Values.Look(
                ref enableSteamWorkshopScenarios,
                "enableSteamWorkshopScenarios",
                true
            );

            Scribe_Values.Look(ref rainfall, "rainfall", 2);
            Scribe_Values.Look(ref temperature, "temperature", 3);
            Scribe_Values.Look(ref population, "population", 3);
            Scribe_Values.Look(ref landmarkDensity, "landmarkDensity", 3);
            Scribe_Values.Look(ref pollution, "pollution", 0.05f);
            Scribe_Values.Look(ref startingSeason, "startingSeason", 2);

            // Starting Tile Settings
            Scribe_Values.Look(ref filterStartingBiome, "filterStartingBiome", false);
            Scribe_Collections.Look(ref allowedBiomes, "allowedBiomes", LookMode.Value, new List<string>());
            Scribe_Values.Look(ref filterStartingHilliness, "filterStartingHilliness", false);
            Scribe_Collections.Look(ref allowedHilliness, "allowedHilliness", LookMode.Value, new List<int>());

            // Starting Colonist Settings
            Scribe_Values.Look(ref startingPawnForceViolence, "startingPawnForceViolence", false);

            Scribe_Collections.Look(
                ref disabledStorytellers,
                "disabledStorytellers",
                LookMode.Value,
                new List<string>()
            );
            Scribe_Collections.Look(
                ref disabledScenarios,
                "disabledScenarios",
                LookMode.Value,
                new List<string>() { "tutorial" }
            );
            Scribe_Collections.Look(
                ref factionsAlwaysAdd,
                "factionsAlwaysAdd",
                LookMode.Value,
                new List<string>()
                {
                    "Ancients",
                    "AncientsHostile",
                    "Mechanoid",
                    "Insect",
                    "Empire",
                    "HoraxCult",
                    "Entities",
                    "VFEE_Deserters",
                }
            );
            Scribe_Collections.Look(
                ref factionsRandomlyAdd,
                "factionsRandomlyAdd",
                LookMode.Value,
                new List<string>()
                {
                    "OutlanderCivil",
                    "OutlanderRough",
                    "TribeCivil",
                    "TribeRough",
                    "TribeSavage",
                    "Pirate",
                    "Empire",
                    "CannibalPirate",
                    "NudistTribe",
                    "TribeCannibal",
                    "TribeRoughNeanderthal",
                    "PirateYttakin",
                    "TribeSavageImpid",
                    "OutlanderRoughPig",
                    "PirateWaster",
                }
            );
            Scribe_Values.Look(ref randomiseFactionGoodwill, "randomiseFactionGoodwill", false);
            Scribe_Collections.Look(ref factionsExcludeFromReputationRandomization, "factionsExcludeFromReputationRandomization", LookMode.Value, new List<string>());

            Scribe_Values.Look(ref randomFactionRange, "randomFactionRange", new IntRange(5, 15));
            Scribe_Values.Look(ref uniqueFactions, "uniqueFactions", false);

            Scribe_Values.Look(ref anomalyPlaystyle, "anomalyPlaystyle", "Standard");
            Scribe_Values.Look(
                ref anomalyThreatsInactiveFraction,
                "anomalyThreatsInactiveFraction",
                0.08f
            );
            Scribe_Values.Look(
                ref anomalyThreatsActiveFraction,
                "anomalyThreatsActiveFraction",
                0.3f
            );
            Scribe_Values.Look(ref studyEfficiencyFactor, "studyEfficiencyFactor", 0.05f);
            Scribe_Values.Look(
                ref overrideAnomalyThreatsFraction,
                "overrideAnomalyThreatsFraction",
                0.15f
            );

            Scribe_Values.Look(ref allowBigThreats, "allowBigThreats", true);
            Scribe_Values.Look(ref allowIntroThreats, "allowIntroThreats", true);
            Scribe_Values.Look(ref allowCaveHives, "allowCaveHives", true);
            Scribe_Values.Look(ref peacefulTemples, "peacefulTemples", false);
            Scribe_Values.Look(ref allowViolentQuests, "allowViolentQuests", true);
            Scribe_Values.Look(ref predatorsHuntHumanlikes, "predatorsHuntHumanlikes", true);
            Scribe_Values.Look(ref babiesAreHealthy, "babiesAreHealthy", false);
            Scribe_Values.Look(ref noBabiesOrChildren, "noBabiesOrChildren", false);
            Scribe_Values.Look(ref allowTraps, "allowTraps", true);
            Scribe_Values.Look(ref allowTurrets, "allowTurrets", true);
            Scribe_Values.Look(ref allowMortars, "allowMortars", true);
            Scribe_Values.Look(ref classicMortars, "classicMortars", false);
            Scribe_Values.Look(ref allowExtremeWeatherIncidents, "allowExtremeWeatherIncidents", true);
            Scribe_Values.Look(ref fixedWealthMode, "fixedWealthMode", false);
            Scribe_Values.Look(ref unwaveringPrisoners, "unwaveringPrisoners", true);
            Scribe_Values.Look(ref childRaidersAllowed, "childRaidersAllowed", false);

            Scribe_Values.Look(ref threatScale, "threatScale", 1f);
            Scribe_Values.Look(ref scariaRotChance, "scariaRotChance", 0f);
            Scribe_Values.Look(ref colonistMoodOffset, "colonistMoodOffset", 0f);
            Scribe_Values.Look(ref tradePriceFactorLoss, "tradePriceFactorLoss", 0f);
            Scribe_Values.Look(ref cropYieldFactor, "cropYieldFactor", 1f);
            Scribe_Values.Look(ref mineYieldFactor, "mineYieldFactor", 1f);
            Scribe_Values.Look(ref butcherYieldFactor, "butcherYieldFactor", 1f);
            Scribe_Values.Look(ref researchSpeedFactor, "researchSpeedFactor", 1f);
            Scribe_Values.Look(ref diseaseIntervalFactor, "diseaseIntervalFactor", 1f);
            Scribe_Values.Look(ref enemyReproductionRateFactor, "enemyReproductionRateFactor", 1f);
            Scribe_Values.Look(ref playerPawnInfectionChanceFactor, "playerPawnInfectionChanceFactor", 1f);
            Scribe_Values.Look(ref manhunterChanceOnDamageFactor, "manhunterChanceOnDamageFactor", 1f);
            Scribe_Values.Look(ref deepDrillInfestationChanceFactor, "deepDrillInfestationChanceFactor", 1f);
            Scribe_Values.Look(ref wastepackInfestationChanceFactor, "wastepackInfestationChanceFactor", 1f);
            Scribe_Values.Look(ref foodPoisonChanceFactor, "foodPoisonChanceFactor", 1f);
            Scribe_Values.Look(ref maintenanceCostFactor, "maintenanceCostFactor", 1f);
            Scribe_Values.Look(ref enemyDeathOnDownedChanceFactor, "enemyDeathOnDownedChanceFactor", 1f);
            Scribe_Values.Look(ref adaptationGrowthRateFactorOverZero, "adaptationGrowthRateFactorOverZero", 1f);
            Scribe_Values.Look(ref adaptationEffectFactor, "adaptationEffectFactor", 1f);
            Scribe_Values.Look(ref questRewardValueFactor, "questRewardValueFactor", 1f);
            Scribe_Values.Look(ref raidLootPointsFactor, "raidLootPointsFactor", 1f);
            Scribe_Values.Look(ref lowPopConversionBoost, "lowPopConversionBoost", 3f);
            Scribe_Values.Look(ref minThreatPointsRangeCeiling, "minThreatPointsRangeCeiling", 70f);
            Scribe_Values.Look(ref childAgingRate, "childAgingRate", 4f);
            Scribe_Values.Look(ref adultAgingRate, "adultAgingRate", 1f);
            Scribe_Values.Look(ref anomalyThreatsInactiveFraction, "anomalyThreatsInactiveFraction", 0.08f);
            Scribe_Values.Look(ref anomalyThreatsActiveFraction, "anomalyThreatsActiveFraction", 0.3f);
            Scribe_Values.Look(ref studyEfficiencyFactor, "studyEfficiencyFactor", 1f);

            Scribe_Values.Look(ref enableRandomXenotypes, "enableRandomXenotypes", false);
            Scribe_Values.Look(ref enableRandomCustomXenotypes, "enableRandomCustomXenotypes", false);
            Scribe_Values.Look(ref randomGeneRange, "randomGeneRange", new IntRange(5, 15));
            Scribe_Values.Look(ref enableMetabolicEfficiencyMinimum, "enableMetabolicEfficiencyMinimum", false);
            Scribe_Values.Look(ref minimumMetabolicEfficiency, "minimumMetabolicEfficiency", 0);
            Scribe_Values.Look(ref respectFactionXenotypes, "respectFactionXenotypes", true);
            Scribe_Values.Look(ref disableIdeo, "disableIdeo", false);
            Scribe_Values.Look(ref fluidIdeo, "fluidIdeo", false);
            Scribe_Values.Look(ref randomMemeRange, "randomMemeRange", new IntRange(1, 4));
            Scribe_Collections.Look(ref forcedMemes, "forcedMemes", LookMode.Value, new List<string>());
            Scribe_Collections.Look(ref disallowedMemes, "disallowedMemes", LookMode.Value, new List<string>());
            Scribe_Collections.Look(ref disallowedPrecepts, "disallowedPrecepts", LookMode.Value, new List<string>());
            Scribe_Values.Look(ref overrideIdeo, "overrideIdeo", false);
            Scribe_Values.Look(ref customIdeoOverrideFile, "customIdeoOverrideFile", null);

            Scribe_Values.Look(ref removeStartingResearch, "removeStartingResearch", false);
            Scribe_Values.Look(ref addRandomResearch, "addRandomResearch", false);
            Scribe_Values.Look(ref randomResearchRange, "randomResearchRange", new IntRange(5, 15));
            Scribe_Values.Look(ref randomResearchTechLevelLimit, "randomResearchTechLevelLimit", 4);
            Scribe_Values.Look(ref doRandomResearchPrerequisites, "doRandomResearchPrerequisites", true);

            Scribe_Values.Look(ref removeStartingItems, "removeStartingItems", false);
            Scribe_Values.Look(ref addRandomItems, "addRandomItems", false);
            Scribe_Values.Look(ref randomItemRange, "randomItemRange", new IntRange(5, 15));
            Scribe_Values.Look(ref randomItemTechLevelLimit, "randomItemTechLevelLimit", 4);
            Scribe_Values.Look(ref enableMarketValueLimit, "enableMarketValueLimit", false);
            Scribe_Values.Look(ref randomItemTotalMarketValueLimit, "randomItemTotalMarketValueLimit", 10000);

            Scribe_Values.Look(ref myLittlePlanetSubcount, "myLittlePlanetSubcount", 10);

            Scribe_Values.Look(ref realisticPlanetsUseWordType, "realisticPlanetsUseWordType", defaultValue: false);
            Scribe_Values.Look(ref realisticPlanetsUseWordTypeChance, "realisticPlanetsUseWordTypeChance", 0.5f);

            Scribe_Values.Look(ref realisticPlanetsWorldType, "realisticPlanetsWorldType", "Planets.Vanilla");
            Scribe_Values.Look(ref randomiseRealisticPlanets, "randomiseRealisticPlanets", defaultValue: false);

            Scribe_Values.Look(ref randomiseOceanTypeRange, "randomiseOceanTypeRange", new IntRange(2, 4));
            Scribe_Values.Look(ref realisticPlanetsOceanType, "realisticPlanetsOceanType", 3);
            Scribe_Values.Look(ref randomiseOceanType, "randomiseOceanType", defaultValue: true);

            Scribe_Values.Look(ref randomiseAxialTiltRange, "randomiseAxialTiltRange", new IntRange(1, 3));
            Scribe_Values.Look(ref randomiseAxialTilt, "randomiseAxialTilt", defaultValue: true);
            Scribe_Values.Look(ref realisticPlanetsAxialTilt, "realisticPlanetsAxialTilt", 2);

            Scribe_Values.Look(ref enableAutoRealRuins, "enableAutoRealRuins", defaultValue: true);
            Scribe_Values.Look(ref realRuinsBiomeFilter, "realRuinsBiomeFilter", defaultValue: false);
            Scribe_Values.Look(ref noPauseEnabled, "noPauseEnabled", defaultValue: true);
            Scribe_Values.Look(ref noPauseHalfSpeedEnabled, "noPauseHalfSpeedEnabled", defaultValue: false);

            Scribe_Values.Look(ref randomisePawnName, "randomisePawnName", defaultValue: true);
            Scribe_Values.Look(ref PawnFirstName, "PawnFirstName", null);
            Scribe_Values.Look(ref PawnNickName, "PawnNickName", null);
            Scribe_Values.Look(ref PawnLastName, "PawnLastName", null);
            Scribe_Values.Look(ref randomisePawnAge, "randomisePawnAge", defaultValue: false);
            Scribe_Values.Look(ref randomisePawnAgeRange, "randomisePawnAgeRange", new IntRange(20, 40));
            Scribe_Values.Look(ref randomisePawnSex, "randomisePawnSex", defaultValue: true);
            Scribe_Values.Look(ref PawnSex, "PawnSex", 0);
            Scribe_Values.Look(ref PawnNotDisabledWorkTags, "PawnNotDisabledWorkTags", defaultValue: false);

            Scribe_Values.Look(ref limitStartingTileTemperature, "limitStartingTileTemperature", defaultValue: false);
            Scribe_Values.Look(ref limitStartingTileTemperatureRange, "limitStartingTileTemperatureRange", new FloatRange(-10f, 30f));

            base.ExposeData();
        }

        public void ResetDifficulty()
        {
            difficulty = "Rough";
            anomalyPlaystyle = "Standard";
            allowBigThreats = true;
            allowIntroThreats = true;
            allowCaveHives = true;
            peacefulTemples = false;
            allowViolentQuests = true;
            predatorsHuntHumanlikes = true;
            babiesAreHealthy = false;
            noBabiesOrChildren = false;
            allowTraps = true;
            allowTurrets = true;
            allowMortars = true;
            classicMortars = false;
            allowExtremeWeatherIncidents = true;
            fixedWealthMode = false;
            unwaveringPrisoners = true;
            childRaidersAllowed = false;
            childShamblersAllowed = false;
            threatScale = 1f;
            scariaRotChance = 0.4f;
            colonistMoodOffset = 0f;
            tradePriceFactorLoss = 0f;
            cropYieldFactor = 1f;
            mineYieldFactor = 1f;
            butcherYieldFactor = 1f;
            researchSpeedFactor = 1f;
            diseaseIntervalFactor = 1f;
            enemyReproductionRateFactor = 1f;
            playerPawnInfectionChanceFactor = 1f;
            manhunterChanceOnDamageFactor = 1f;
            deepDrillInfestationChanceFactor = 1f;
            wastepackInfestationChanceFactor = 1f;
            foodPoisonChanceFactor = 1f;
            maintenanceCostFactor = 1f;
            enemyDeathOnDownedChanceFactor = 1f;
            adaptationGrowthRateFactorOverZero = 1f;
            adaptationEffectFactor = 1f;
            questRewardValueFactor = 1f;
            raidLootPointsFactor = 1f;
            lowPopConversionBoost = 3f;
            minThreatPointsRangeCeiling = 70f;
            childAgingRate = 4f;
            adultAgingRate = 1f;
            anomalyThreatsInactiveFraction = 0.08f;
            anomalyThreatsActiveFraction = 0.3f;
            overrideAnomalyThreatsFraction = 0.15f;
            studyEfficiencyFactor = 1f;
            friendlyFireChanceFactor = 0.4f;
            allowInstantKillChance = 0f;
            fixedWealthTimeFactor = 1f;
            noPauseEnabled = true;
            noPauseHalfSpeedEnabled = false;
        }
        public void ResetPlanet()
        {
            mapSize = 250;
            planetCoverage = 0.3f;
            permadeath = false;
            randomiseWorldSeed = true;
            worldSeed = "village";
            randomiseRainfall = true;
            randomiseTemperature = true;
            randomisePopulation = true;
            randomiseLandmarkDensity = true;
            randomisePollution = true;
            randomiseRainfallRange = new IntRange(1, 3);
            randomiseTemperatureRange = new IntRange(2, 4);
            randomisePopulationRange = new IntRange(2, 4);
            randomiseLandmarkDensityRange = new IntRange(2, 4);
            randomisePollutionRange = new FloatRange(0.05f, 0.25f);
            randomiseSeason = true;
            rainfall = 2;
            temperature = 3;
            population = 3;
            landmarkDensity = 3;
            pollution = 0.05f;
            startingSeason = 2;

            // Starting Tile Settings
            filterStartingBiome = false;
            allowedBiomes = new List<string>();

            myLittlePlanetSubcount = 10;

            realisticPlanetsUseWordType = false;
            realisticPlanetsUseWordTypeChance = 0.5f;
            randomiseRealisticPlanets = false;
            realisticPlanetsWorldType = "Planets.Vanilla";
            realisticPlanetsOceanType = 3;
            randomiseOceanType = true;
            randomiseOceanTypeRange = new IntRange(2, 4);
            realisticPlanetsAxialTilt = 2;
            randomiseAxialTilt = true;
            randomiseAxialTiltRange = new IntRange(1, 3);

            enableAutoRealRuins = true;
            realRuinsBiomeFilter = false;
        }
        public void ResetFactions()
        {
            var allFactions = FactionGenerator.ConfigurableFactions.Where(faction => faction.startingCountAtWorldCreation > 0);
            factionsAlwaysAdd = allFactions.Where(faction => faction.maxConfigurableAtWorldCreation == 1).Select(faction => faction.defName).ToList();

            var randomFactions = allFactions.Where(faction => faction.maxConfigurableAtWorldCreation != 1).ToList();
            using (IEnumerator<FactionDef> enumerator = FactionGenerator.ConfigurableFactions.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    FactionDef faction = enumerator.Current;
                    if (faction.replacesFaction != null)
                    {
                        randomFactions.RemoveAll((FactionDef x) => x == faction.replacesFaction);
                    }
                }
            }
            factionsRandomlyAdd = randomFactions.Select(faction => faction.defName).ToList();

            randomFactionRange = new IntRange(5, 15);
            uniqueFactions = false;
            randomiseFactionGoodwill = false;
            factionsExcludeFromReputationRandomization = new List<string>();
        }
        public void ResetScenarios()
        {
            createRandomScenario = false;
            enableCustomScenarios = true;
            enableSteamWorkshopScenarios = true;
            disabledScenarios = new List<string>() { "tutorial" };
            ResetResearch();
            ResetItems();
        }
        public void ResetStorytellers()
        {
            disabledStorytellers = new List<string>() { "tutorial" };
        }

        public void ResetOptionalFeatures()
        {
            ResetGenes();
            ResetIdeo();
        }

        public void ResetGenes()
        {
            enableRandomXenotypes = false;
            enableRandomCustomXenotypes = false;
            respectFactionXenotypes = true;
            randomGeneRange = new IntRange(5, 15);
            minimumMetabolicEfficiency = 0;
            enableMetabolicEfficiencyMinimum = false;
        }

        public void ResetIdeo()
        {
            disableIdeo = false;
            fluidIdeo = false;
            randomMemeRange = new IntRange(1, 4);
            forcedMemes = new List<string>();
            disallowedMemes = new List<string>();
            disallowedPrecepts = new List<string>();
            overrideIdeo = false;
            customIdeoOverrideFile = null;
        }

        public void ResetResearch()
        {
            removeStartingResearch = false;
            addRandomResearch = false;
            randomResearchRange = new IntRange(5, 15);
            randomResearchTechLevelLimit = 4;
            doRandomResearchPrerequisites = true;
        }

        public void ResetItems()
        {
            removeStartingItems = false;
            addRandomItems = false;
            randomItemRange = new IntRange(5, 15);
            randomItemTechLevelLimit = 4;
            randomItemTotalMarketValueLimit = 10000;
            enableMarketValueLimit = false;
        }

        public void ResetStartingTile()
        {
            filterStartingBiome = false;
            allowedBiomes = new List<string>();
            filterStartingHilliness = false;
            allowedHilliness = new List<int>();
            limitStartingTileTemperature = false;
            limitStartingTileTemperatureRange = new FloatRange(-10f, 30f);
        }

        public void ResetStartingColonists()
        {
            startingPawnForceViolence = false;
            randomisePawnName = true;
            randomisePawnAge = false;
            randomisePawnAgeRange = new IntRange(20, 40);
            randomisePawnSex = true;
            PawnSex = 0;
            PawnNotDisabledWorkTags = false;
        }
    }
}
