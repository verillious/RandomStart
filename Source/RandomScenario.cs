using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Verse;

namespace RandomStartMod
{
    public static class RandomScenario
    {
        public static void SetupForRandomPlay()
        {

            Util.LogMessage($"Randomising Scenario");
            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();

            Current.ProgramState = ProgramState.Entry;
            Current.Game = new Game();
            Current.Game.InitData = new GameInitData();

            ScenarioDef chosenScenario = ScenarioDefOf.Crashlanded;
            List<ScenarioDef> possibleScenarios = new List<ScenarioDef>();

            foreach (ScenarioDef item in DefDatabase<ScenarioDef>.AllDefs)
            {
                if (!settings.disabledScenarios.Contains(item.defName) && item.scenario.showInUI)
                {
                    possibleScenarios.Add(item);
                }
            }

            if (possibleScenarios.Count > 0)
            {
                int scenarioIndex = Rand.Range(0, possibleScenarios.Count);
                chosenScenario = possibleScenarios[scenarioIndex];
            }
            else
            {
                settings.disabledScenarios.Remove(chosenScenario.defName);
            }


            Current.Game.Scenario = chosenScenario.scenario;

            Util.LogMessage($"Starting {Current.Game.Scenario}");
            DifficultyDef chosenDifficultyDef = DefDatabase<DifficultyDef>.AllDefs.First((DifficultyDef d) => d.defName == settings.difficulty);
            if (chosenDifficultyDef.isCustom)
            {
                chosenDifficultyDef.threatScale = settings.threatScale;
                chosenDifficultyDef.allowBigThreats = settings.allowBigThreats;
                chosenDifficultyDef.allowIntroThreats = settings.allowIntroThreats;
                chosenDifficultyDef.allowCaveHives = settings.allowCaveHives;
                chosenDifficultyDef.peacefulTemples = settings.peacefulTemples;
                chosenDifficultyDef.allowViolentQuests = settings.allowViolentQuests;
                chosenDifficultyDef.predatorsHuntHumanlikes = settings.predatorsHuntHumanlikes;
                chosenDifficultyDef.scariaRotChance = settings.scariaRotChance;
                chosenDifficultyDef.colonistMoodOffset = settings.colonistMoodOffset;
                chosenDifficultyDef.tradePriceFactorLoss = settings.tradePriceFactorLoss;
                chosenDifficultyDef.cropYieldFactor = settings.cropYieldFactor;
                chosenDifficultyDef.mineYieldFactor = settings.mineYieldFactor;
                chosenDifficultyDef.butcherYieldFactor = settings.butcherYieldFactor;
                chosenDifficultyDef.researchSpeedFactor = settings.researchSpeedFactor;
                chosenDifficultyDef.diseaseIntervalFactor = settings.diseaseIntervalFactor;
                chosenDifficultyDef.enemyReproductionRateFactor = settings.enemyReproductionRateFactor;
                chosenDifficultyDef.playerPawnInfectionChanceFactor = settings.playerPawnInfectionChanceFactor;
                chosenDifficultyDef.manhunterChanceOnDamageFactor = settings.manhunterChanceOnDamageFactor;
                chosenDifficultyDef.deepDrillInfestationChanceFactor = settings.deepDrillInfestationChanceFactor;
                chosenDifficultyDef.wastepackInfestationChanceFactor = settings.wastepackInfestationChanceFactor;
                chosenDifficultyDef.foodPoisonChanceFactor = settings.foodPoisonChanceFactor;
                chosenDifficultyDef.maintenanceCostFactor = settings.maintenanceCostFactor;
                chosenDifficultyDef.enemyDeathOnDownedChanceFactor = settings.enemyDeathOnDownedChanceFactor;
                chosenDifficultyDef.adaptationGrowthRateFactorOverZero = settings.adaptationGrowthRateFactorOverZero;
                chosenDifficultyDef.adaptationEffectFactor = settings.adaptationEffectFactor;
                chosenDifficultyDef.questRewardValueFactor = settings.questRewardValueFactor;
                chosenDifficultyDef.raidLootPointsFactor = settings.raidLootPointsFactor;
                chosenDifficultyDef.allowTraps = settings.allowTraps;
                chosenDifficultyDef.allowTurrets = settings.allowTurrets;
                chosenDifficultyDef.allowMortars = settings.allowMortars;
                chosenDifficultyDef.classicMortars = settings.classicMortars;
                chosenDifficultyDef.allowExtremeWeatherIncidents = settings.allowExtremeWeatherIncidents;
                chosenDifficultyDef.fixedWealthMode = settings.fixedWealthMode;
                chosenDifficultyDef.lowPopConversionBoost = settings.lowPopConversionBoost;
                chosenDifficultyDef.minThreatPointsRangeCeiling = settings.minThreatPointsRangeCeiling;
                chosenDifficultyDef.babiesAreHealthy = settings.babiesAreHealthy;
                chosenDifficultyDef.noBabiesOrChildren = settings.noBabiesOrChildren;
                chosenDifficultyDef.childAgingRate = settings.childAgingRate;
                chosenDifficultyDef.adultAgingRate = settings.adultAgingRate;
                chosenDifficultyDef.unwaveringPrisoners = settings.unwaveringPrisoners;
                chosenDifficultyDef.childRaidersAllowed = settings.childRaidersAllowed;
                chosenDifficultyDef.anomalyThreatsInactiveFraction = settings.anomalyThreatsInactiveFraction;
                chosenDifficultyDef.anomalyThreatsActiveFraction = settings.anomalyThreatsActiveFraction;
                chosenDifficultyDef.studyEfficiencyFactor = settings.studyEfficiencyFactor;
            }

            StorytellerDef chosenStoryteller = StorytellerDefOf.Cassandra;
            List<StorytellerDef> possibleStorytellers = new List<StorytellerDef>();

            foreach (StorytellerDef item in DefDatabase<StorytellerDef>.AllDefs)
            {
                if (!settings.disabledStorytellers.Contains(item.defName) && item.listVisible)
                {
                    possibleStorytellers.Add(item);
                }
            }

            if (possibleStorytellers.Count > 0)
            {
                int storytellerIndex = Rand.Range(0, possibleStorytellers.Count);
                chosenStoryteller = possibleStorytellers[storytellerIndex];
            }
            else
            {
                settings.disabledStorytellers.Remove(chosenStoryteller.defName);
            }

            chosenStoryteller.tutorialMode = false;

            Current.Game.storyteller = new Storyteller(chosenStoryteller, chosenDifficultyDef);

            if (chosenDifficultyDef.isCustom)
            {
                Difficulty difficulty = Current.Game.storyteller.difficulty;
                difficulty.fixedWealthTimeFactor = settings.fixedWealthTimeFactor;
                difficulty.friendlyFireChanceFactor = settings.friendlyFireChanceFactor;
                difficulty.allowInstantKillChance = settings.allowInstantKillChance;
            }

            if (ModsConfig.AnomalyActive)
            {
                Difficulty difficulty = Current.Game.storyteller.difficulty;
                AnomalyPlaystyleDef chosenAnomalyPlaystyleDef = DefDatabase<AnomalyPlaystyleDef>.GetNamed(settings.anomalyPlaystyle);
                if (chosenAnomalyPlaystyleDef.overrideThreatFraction)
                {
                    difficulty.overrideAnomalyThreatsFraction = settings.overrideAnomalyThreatsFraction;
                }
                else
                {
                    difficulty.overrideAnomalyThreatsFraction = null;
                }
                difficulty.anomalyThreatsInactiveFraction = settings.anomalyThreatsInactiveFraction;
                difficulty.anomalyThreatsActiveFraction = settings.anomalyThreatsActiveFraction;
                difficulty.studyEfficiencyFactor = settings.studyEfficiencyFactor;
                difficulty.AnomalyPlaystyleDef = chosenAnomalyPlaystyleDef;
            }

            OverallRainfall rainfall = (OverallRainfall)settings.rainfall;
            if (settings.randomiseRainfall)
                rainfall = (OverallRainfall)Rand.Range(0, 6);

            OverallTemperature temperature = (OverallTemperature)settings.temperature;
            if (settings.randomiseTemperature)
                temperature = (OverallTemperature)Rand.Range(0, 6);

            OverallPopulation population = (OverallPopulation)settings.population;
            if (settings.randomisePopulation)
                population = (OverallPopulation)Rand.Range(0, 6);

            float pollution = settings.pollution;
            if (settings.randomisePollution)
                pollution = Rand.Range(0f, 1f);

            List<FactionDef> worldFactions = new List<FactionDef>();

            IEnumerable<FactionDef> allFactionDefs = DefDatabase<FactionDef>.AllDefsListForReading.Where((FactionDef x) => x.isPlayer);

            foreach (string factionDefName in settings.factionsAlwaysAdd)
            {
                FactionDef faction = DefDatabase<FactionDef>.GetNamed(factionDefName, false);
                if (faction == null)
                    continue;
                worldFactions.Add(faction);
            }
            if (settings.factionsRandomlyAdd.Count > 0)
            {
                int factionCount = worldFactions.Count((FactionDef x) => !x.hidden);
                int diff = 11 - factionCount;
                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                    {
                        FactionDef faction = DefDatabase<FactionDef>.GetNamed(settings.factionsRandomlyAdd[Rand.Range(0, settings.factionsRandomlyAdd.Count)], false);
                        if (faction == null)
                            continue;
                        worldFactions.Add(faction);
                    }
                }

            }

            if (Util.IsModRunning("Vanilla Factions Expanded - Empire"))
            {
                Compat.VFEECompat.EnsureScenarioFactions(worldFactions);
            }

            if (Util.IsModRunning("Vanilla Factions Expanded - Deserters"))
            {
                Compat.VFEDCompat.EnsureScenarioFactions(worldFactions);
            }

            if (Util.IsModRunning("Save Our Ship 2") && Util.IsScenarioFromMod("Save Our Ship 2"))
            {
                Compat.SOS2Compat.SetupForStartInSpace();
            }

            Current.Game.World = WorldGenerator.GenerateWorld(settings.planetCoverage, GenText.RandomSeedString(), rainfall, temperature, population, worldFactions, pollution);

            Find.GameInitData.ChooseRandomStartingTile();

            Season startingSeason = (Season)settings.startingSeason;
            if (settings.randomiseSeason)
                startingSeason = (Season)Rand.Range(1, 6);

            Find.GameInitData.startingSeason = startingSeason;
            Find.GameInitData.mapSize = settings.mapSize;
            Find.GameInitData.permadeath = settings.permadeath;

            if (Util.IsModRunning("Vanilla Expanded Framework"))
            {
                Compat.VECoreCompat.SetupForKCSG();
            }

            Find.Scenario.PostIdeoChosen();
            if (ModsConfig.BiotechActive)
            {
                if (settings.enableRandomXenotypes)
                {
                    foreach (Pawn p in Find.GameInitData.startingAndOptionalPawns)
                    {
                        XenotypeDef xenotype = DefDatabase<XenotypeDef>.AllDefsListForReading.RandomElement();
                        p.genes.SetXenotype(xenotype);
                    }
                }

                if (settings.enableRandomCustomXenotypes)
                {
                    foreach (Pawn p in Find.GameInitData.startingAndOptionalPawns)
                    {
                        List<GeneDef> selectedGenes = new List<GeneDef>();
                        for (int i = 0; i < Rand.Range(10, 30); i++)
                        {
                            selectedGenes.Add(DefDatabase<GeneDef>.AllDefsListForReading.RandomElement());
                        }
                        if (selectedGenes.Count > 0)
                        {
                            p.genes.xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(selectedGenes);
                            p.genes.iconDef = DefDatabase<XenotypeIconDef>.GetRandom();

                            foreach (GeneDef geneDef in selectedGenes)
                            {
                                Gene gene = GeneMaker.MakeGene(geneDef, p);
                                p.genes.AddGene(gene, Rand.Bool);
                                p.genes.OverrideAllConflicting(gene);
                            }
                            foreach (Gene gene in p.genes.GenesListForReading)
                            {
                                if (gene.Overridden)
                                {
                                    p.genes.RemoveGene(gene);
                                }
                            }
                        }
                    }
                }
            }

            Find.GameInitData.startedFromEntry = true;
            if (ModsConfig.IdeologyActive && settings.disableIdeo)
            {
                Find.IdeoManager.classicMode = true;
                IdeoGenerationParms genParms = new IdeoGenerationParms(Find.FactionManager.OfPlayer.def);
                if (!DefDatabase<CultureDef>.AllDefs.Where((CultureDef x) => Find.FactionManager.OfPlayer.def.allowedCultures.Contains(x)).TryRandomElement(out var result))
                {
                    result = DefDatabase<CultureDef>.AllDefs.RandomElement();
                }
                Ideo classicIdeo = IdeoGenerator.GenerateClassicIdeo(result, genParms, noExpansionIdeo: false);
                foreach (Faction allFaction in Find.FactionManager.AllFactions)
                {
                    if (allFaction.ideos != null)
                    {
                        allFaction.ideos.RemoveAll();
                        allFaction.ideos.SetPrimary(classicIdeo);
                    }
                }
                Find.IdeoManager.RemoveUnusedStartingIdeos();
                Find.Scenario.PostIdeoChosen();
            }

            PageUtility.InitGameStart();
        }
    }
}
