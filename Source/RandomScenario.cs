using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            List<Scenario> scenarios = new List<Scenario>();

            scenarios.AddRange(ScenarioLister.AllScenarios().Where((Scenario scenario) => !settings.disabledScenarios.Contains(scenario.name) && scenario.showInUI));
            if (settings.enableCustomScenarios)
                scenarios.AddRange(ScenarioLister.ScenariosInCategory(ScenarioCategory.CustomLocal).Where((Scenario scenario) => !settings.disabledScenarios.Contains(scenario.name) && scenario.showInUI));
            if (settings.enableSteamWorkshopScenarios)
                scenarios.AddRange(ScenarioLister.ScenariosInCategory(ScenarioCategory.SteamWorkshop).Where((Scenario scenario) => !settings.disabledScenarios.Contains(scenario.name) && scenario.showInUI));


            Current.Game.Scenario = scenarios.RandomElement();
            if (Current.Game.Scenario == null)
            {
                string errorString = "[Random Start] Could not find valid Scenario from:";
                foreach (Scenario s in scenarios)
                {
                    errorString += "\n" + "    - " + s.name;
                }
                Log.Error(errorString);
                Current.Game.Scenario = ScenarioDefOf.Crashlanded.scenario;
            }

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

            if (ModsConfig.IsActive("brrainz.nopausechallenge"))
            {
                Compat.NoPauseCompat.SetupForNoPause();
            }

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
                rainfall = (OverallRainfall)settings.randomiseRainfallRange.RandomInRange;

            OverallTemperature temperature = (OverallTemperature)settings.temperature;
            if (settings.randomiseTemperature)
                temperature = (OverallTemperature)settings.randomiseTemperatureRange.RandomInRange;

            OverallPopulation population = (OverallPopulation)settings.population;
            if (settings.randomisePopulation)
                population = (OverallPopulation)settings.randomisePopulationRange.RandomInRange;

            float pollution = settings.pollution;
            if (settings.randomisePollution)
                pollution = settings.randomisePollutionRange.RandomInRange;

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
                List<string> randomFactions = new List<string>(settings.factionsRandomlyAdd);
                for (int i = 0; i < settings.randomFactionRange.RandomInRange; i++)
                {
                    string randomFaction = randomFactions.RandomElement();
                    FactionDef faction = DefDatabase<FactionDef>.GetNamed(randomFaction, false);

                    if (faction == null)
                        continue;
                    worldFactions.Add(faction);
                    if (settings.uniqueFactions)
                    {
                        randomFactions.Remove(randomFaction);
                        if (randomFactions.Count == 0)
                            break;
                    }
                }
            }

            if (ModsConfig.IsActive("OskarPotocki.VFE.Empire"))
            {
                Compat.VFEECompat.EnsureScenarioFactions(worldFactions);
            }

            if (ModsConfig.IsActive("OskarPotocki.VFE.Deserters"))
            {
                Compat.VFEDCompat.EnsureScenarioFactions(worldFactions);
            }

            if (ModsConfig.IsActive("kentington.saveourship2"))
            {
                Compat.SOS2Compat.SetupForStartInSpace();
            }

            if (ModsConfig.IsActive("zvq.RealisticPlanetsContinued"))
            {
                int worldType = settings.realisticPlanetsWorldType;
                if (settings.randomiseRealisticPlanets)
                    worldType = -1;
                Compat.RealisticPlanetsCompat.GenerateRealisticPlanetWorld(settings.planetCoverage, GenText.RandomSeedString(), rainfall, temperature, population, worldFactions, pollution, worldType);
            }
            else
            {
                Current.Game.World = WorldGenerator.GenerateWorld(settings.planetCoverage, GenText.RandomSeedString(), rainfall, temperature, population, worldFactions, pollution);
            }

            Find.GameInitData.ChooseRandomStartingTile();

            Season startingSeason = (Season)settings.startingSeason;
            if (settings.randomiseSeason)
                startingSeason = (Season)Rand.Range(1, 6);

            Find.GameInitData.startingSeason = startingSeason;
            Find.GameInitData.mapSize = settings.mapSize;
            Find.GameInitData.permadeath = settings.permadeath;

            if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
            {
                Compat.VECoreCompat.SetupForKCSG();
            }

            if (ModsConfig.IdeologyActive)
            {
                if (settings.disableIdeo)
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
                    classicIdeo.initialPlayerIdeo = true;
                    Find.IdeoManager.Add(classicIdeo);
                }
                else
                {
                    if (settings.overrideIdeo && File.Exists(settings.customIdeoOverrideFile))
                    {
                        Util.LogMessage("Attempting to load ideo file");
                        Ideo newIdeo = null;
                        PreLoadUtility.CheckVersionAndLoad(settings.customIdeoOverrideFile, ScribeMetaHeaderUtility.ScribeHeaderMode.Ideo, delegate
                        {
                            if (GameDataSaveLoader.TryLoadIdeo(settings.customIdeoOverrideFile, out var ideo))
                            {
                                newIdeo = IdeoGenerator.InitLoadedIdeo(ideo);
                            }
                        });

                        if (newIdeo != null)
                        {
                            Util.LogMessage("Loaded ideo file");
                            Find.FactionManager.OfPlayer.ideos.RemoveAll();
                            Find.FactionManager.OfPlayer.ideos.SetPrimary(newIdeo);
                            Find.IdeoManager.RemoveUnusedStartingIdeos();
                            newIdeo.initialPlayerIdeo = true;
                            Find.IdeoManager.Add(newIdeo);
                        }
                        else
                        {
                            Log.Error($"[Random Start] Couldn't load ideo file {settings.customIdeoOverrideFile.Split('\\').Last()} - was it saved with different mods?");
                        }
                    }
                    if (settings.fluidIdeo)
                    {
                        Ideo playerIdeo = Find.FactionManager.OfPlayer.ideos.AllIdeos.FirstOrDefault();
                        playerIdeo.Fluid = true;
                    }
                }
            }


            Find.Scenario.PostIdeoChosen();
            if (ModsConfig.BiotechActive)
            {
                if (settings.enableRandomXenotypes)
                {
                    foreach (Pawn p in Find.GameInitData.startingAndOptionalPawns)
                    {
                        IEnumerable<XenotypeDef> xenotypes = DefDatabase<XenotypeDef>.AllDefsListForReading;
                        if (settings.respectFactionXenotypes && Find.FactionManager.OfPlayer.def.basicMemberKind.xenotypeSet != null)
                            xenotypes = xenotypes.Where((XenotypeDef x) => Find.FactionManager.OfPlayer.def.basicMemberKind.xenotypeSet.Contains(x));
                        XenotypeDef xenotype = xenotypes.RandomElement();
                        p.genes.SetXenotype(xenotype);
                    }


                }

                if (settings.enableRandomCustomXenotypes)
                {
                    foreach (Pawn p in Find.GameInitData.startingAndOptionalPawns)
                    {
                        List<GeneDef> selectedGenes = new List<GeneDef>();
                        for (int i = 0; i < settings.randomGeneRange.RandomInRange; i++)
                        {
                            selectedGenes.Add(DefDatabase<GeneDef>.AllDefsListForReading.Where((GeneDef g) => g.canGenerateInGeneSet).RandomElement());
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

            PageUtility.InitGameStart();

            if (ModsConfig.IsActive("Woolstrand.RealRuins") && settings.enableAutoRealRuins)
            {
                Compat.RealRuinsCompat.CreatePOIs();
            }
        }
    }
}
