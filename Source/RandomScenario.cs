using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using static RimWorld.PsychicRitualRoleDef;

namespace RandomStartMod
{
    public static class RandomScenario
    {
        public static void SetupForRandomPlay()
        {
            Util.LogMessage($"Randomising Scenario");
            RandomStartData.startedFromRandom = true;
            RandomStartSettings settings = LoadedModManager
                .GetMod<RandomStartMod>()
                .GetSettings<RandomStartSettings>();

            Current.ProgramState = ProgramState.Entry;
            Current.Game = new Game();
            Current.Game.InitData = new GameInitData();
            SetupRandomScenario(settings);
            SetupRandomDifficulty(settings);
            SetupRandomPlanet(settings);

            Find.GameInitData.ChooseRandomStartingTile();

            Util.LogMessage($"Starting in {Find.WorldGrid[Find.GameInitData.startingTile].biome.label.CapitalizeFirst()}");

            Season startingSeason = (Season)settings.startingSeason;
            if (settings.randomiseSeason)
                startingSeason = (Season)Rand.Range(1, 6);

            Find.GameInitData.startingSeason = startingSeason;
            Find.GameInitData.mapSize = settings.mapSize;

            if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") || ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core_Steam"))
            {
                Compat.VECoreCompat.SetupForKCSG();
            }

            if (ModsConfig.IdeologyActive)
            {
                SetupRandomIdeology(settings);
            }

            foreach (ScenPart allPart in Find.Scenario.AllParts)
            {
                allPart.PostIdeoChosen();
            }

            if (ModsConfig.BiotechActive)
            {
                SetupRandomGenes(settings);
            }

            Find.GameInitData.startedFromEntry = true;

            //Settlement playerSettlement;

            //List<Settlement> settlements = Find.WorldObjects.Settlements;
            //for (int i = 0; i < settlements.Count; i++)
            //{
            //    if (settlements[i].Faction == Faction.OfPlayer)
            //    {
            //        playerSettlement = settlements[i];
            //        playerSettlement.MapGeneratorDef.genSteps.Clear();
            //        break;
            //    }
            //}

            if (settings.randomiseFactionGoodwill)
            {
                Util.LogMessage("Randomising Faction Goodwill");
                foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
                {
                    item.RemoveAllRelations();
                    foreach (Faction item2 in Find.FactionManager.AllFactionsListForReading)
                    {
                        if (item != item2)
                        {
                            if (item.RelationWith(item2, allowNull: true) == null)
                            {
                                int initialGoodwill = GetInitialGoodwill(item, item2);

                                FactionRelationKind kind = ((initialGoodwill > -10) ? ((initialGoodwill < 75) ? FactionRelationKind.Neutral : FactionRelationKind.Ally) : FactionRelationKind.Hostile);
                                FactionRelation factionRelation = new FactionRelation();
                                factionRelation.other = item2;
                                factionRelation.baseGoodwill = initialGoodwill;
                                factionRelation.kind = kind;
                                item.relations.Add(factionRelation);
                                FactionRelation factionRelation2 = new FactionRelation();
                                factionRelation2.other = item;
                                factionRelation2.baseGoodwill = initialGoodwill;
                                factionRelation2.kind = kind;
                                item2.relations.Add(factionRelation2);
                            }

                            int RoundNum(int num)
                            {
                                int rem = num % 10;
                                return rem >= 5 ? (num - rem + 10) : (num - rem);
                            }

                            int GetInitialGoodwill(Faction a, Faction b)
                            {
                                if (a.def.permanentEnemy || b.def.permanentEnemy)
                                {
                                    return -100;
                                }
                                if ((a.def.permanentEnemyToEveryoneExceptPlayer && !b.IsPlayer) || (b.def.permanentEnemyToEveryoneExceptPlayer && !a.IsPlayer))
                                {
                                    return -100;
                                }
                                if (a.def.permanentEnemyToEveryoneExcept != null && !a.def.permanentEnemyToEveryoneExcept.Contains(b.def))
                                {
                                    return -100;
                                }
                                if (b.def.permanentEnemyToEveryoneExcept != null && !b.def.permanentEnemyToEveryoneExcept.Contains(a.def))
                                {
                                    return -100;
                                }
                                return RoundNum(Rand.Range(-100, 101));
                            }
                        }
                    }
                }
            }
            PageUtility.InitGameStart();

            if (ModsConfig.IsActive("Woolstrand.RealRuins") || ModsConfig.IsActive("Woolstrand.RealRuins_Steam"))
            {
                if (settings.enableAutoRealRuins)
                {
                    Compat.RealRuinsCompat.CreatePOIs();
                }
            }
        }

        private static void SetupRandomScenario(RandomStartSettings settings)
        {
            if (settings.createRandomScenario)
            {
                Current.Game.Scenario = ScenarioMaker.GenerateNewRandomScenario(GenText.RandomSeedString());
            }
            else
            {
                List<Scenario> scenarios = new List<Scenario>();

                scenarios.AddRange(
                    ScenarioLister
                        .AllScenarios()
                        .Where(
                            (Scenario scenario) =>
                                !settings.disabledScenarios.Contains(scenario.name) && scenario.showInUI
                        )
                );
                if (settings.enableCustomScenarios)
                    scenarios.AddRange(
                        ScenarioLister
                            .ScenariosInCategory(ScenarioCategory.CustomLocal)
                            .Where(
                                (Scenario scenario) =>
                                    !settings.disabledScenarios.Contains(scenario.name)
                                    && scenario.showInUI
                            )
                    );
                if (settings.enableSteamWorkshopScenarios)
                    scenarios.AddRange(
                        ScenarioLister
                            .ScenariosInCategory(ScenarioCategory.SteamWorkshop)
                            .Where(
                                (Scenario scenario) =>
                                    !settings.disabledScenarios.Contains(scenario.name)
                                    && scenario.showInUI
                            )
                    );

                Scenario randomScenario = scenarios.RandomElement();
                if (randomScenario == null)
                {
                    string errorString = "[Random Start] Could not find valid Scenario from:";
                    foreach (Scenario s in scenarios)
                    {
                        errorString += "\n" + "    - " + s.name;
                    }
                    Log.Error(errorString);
                    randomScenario = ScenarioDefOf.Crashlanded.scenario;
                }

                if (settings.removeStartingItems)
                {
                    Util.LogMessage("Removing scattered starting items from Scenario");
                    randomScenario.parts.RemoveAll(x => x is ScenPart_ScatterThings);
                }
                Current.Game.Scenario = randomScenario;
            }
            Util.LogMessage($"Starting {Current.Game.Scenario}");
        }

        private static void SetupRandomDifficulty(RandomStartSettings settings)
        {
            DifficultyDef chosenDifficultyDef = DefDatabase<DifficultyDef>.AllDefs.First(
                (DifficultyDef d) => d.defName == settings.difficulty
            );
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
                chosenDifficultyDef.enemyReproductionRateFactor =
                    settings.enemyReproductionRateFactor;
                chosenDifficultyDef.playerPawnInfectionChanceFactor =
                    settings.playerPawnInfectionChanceFactor;
                chosenDifficultyDef.manhunterChanceOnDamageFactor =
                    settings.manhunterChanceOnDamageFactor;
                chosenDifficultyDef.deepDrillInfestationChanceFactor =
                    settings.deepDrillInfestationChanceFactor;
                chosenDifficultyDef.wastepackInfestationChanceFactor =
                    settings.wastepackInfestationChanceFactor;
                chosenDifficultyDef.foodPoisonChanceFactor = settings.foodPoisonChanceFactor;
                chosenDifficultyDef.maintenanceCostFactor = settings.maintenanceCostFactor;
                chosenDifficultyDef.enemyDeathOnDownedChanceFactor =
                    settings.enemyDeathOnDownedChanceFactor;
                chosenDifficultyDef.adaptationGrowthRateFactorOverZero =
                    settings.adaptationGrowthRateFactorOverZero;
                chosenDifficultyDef.adaptationEffectFactor = settings.adaptationEffectFactor;
                chosenDifficultyDef.questRewardValueFactor = settings.questRewardValueFactor;
                chosenDifficultyDef.raidLootPointsFactor = settings.raidLootPointsFactor;
                chosenDifficultyDef.allowTraps = settings.allowTraps;
                chosenDifficultyDef.allowTurrets = settings.allowTurrets;
                chosenDifficultyDef.allowMortars = settings.allowMortars;
                chosenDifficultyDef.classicMortars = settings.classicMortars;
                chosenDifficultyDef.allowExtremeWeatherIncidents =
                    settings.allowExtremeWeatherIncidents;
                chosenDifficultyDef.fixedWealthMode = settings.fixedWealthMode;
                chosenDifficultyDef.lowPopConversionBoost = settings.lowPopConversionBoost;
                chosenDifficultyDef.minThreatPointsRangeCeiling =
                    settings.minThreatPointsRangeCeiling;
                chosenDifficultyDef.babiesAreHealthy = settings.babiesAreHealthy;
                chosenDifficultyDef.noBabiesOrChildren = settings.noBabiesOrChildren;
                chosenDifficultyDef.childAgingRate = settings.childAgingRate;
                chosenDifficultyDef.adultAgingRate = settings.adultAgingRate;
                chosenDifficultyDef.unwaveringPrisoners = settings.unwaveringPrisoners;
                chosenDifficultyDef.childRaidersAllowed = settings.childRaidersAllowed;
                chosenDifficultyDef.anomalyThreatsInactiveFraction =
                    settings.anomalyThreatsInactiveFraction;
                chosenDifficultyDef.anomalyThreatsActiveFraction =
                    settings.anomalyThreatsActiveFraction;
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

            if (ModsConfig.IsActive("brrainz.nopausechallenge") || ModsConfig.IsActive("brrainz.nopausechallenge_steam"))
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
                AnomalyPlaystyleDef chosenAnomalyPlaystyleDef =
                    DefDatabase<AnomalyPlaystyleDef>.GetNamed(settings.anomalyPlaystyle);
                if (chosenAnomalyPlaystyleDef.overrideThreatFraction)
                {
                    difficulty.overrideAnomalyThreatsFraction =
                        settings.overrideAnomalyThreatsFraction;
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
            Find.GameInitData.permadeath = settings.permadeath;
            Util.LogMessage($"Using {chosenStoryteller.LabelCap}, {chosenDifficultyDef.LabelCap}");
        }

        private static void SetupRandomPlanet(RandomStartSettings settings)
        {

            OverallRainfall rainfall = (OverallRainfall)settings.rainfall;
            if (settings.randomiseRainfall)
                rainfall = (OverallRainfall)settings.randomiseRainfallRange.RandomInRange;

            OverallTemperature temperature = (OverallTemperature)settings.temperature;
            if (settings.randomiseTemperature)
                temperature = (OverallTemperature)settings.randomiseTemperatureRange.RandomInRange;

            OverallPopulation population = (OverallPopulation)settings.population;
            if (settings.randomisePopulation)
                population = (OverallPopulation)settings.randomisePopulationRange.RandomInRange;


            // WAITING FOR ODYSSEY
            //LandmarkDensity landmarkDensity = (LandmarkDensity)settings.landmarkDensity;
            //if (settings.randomiseLandmarkDensity)
            //    landmarkDensity = (LandmarkDensity)settings.randomiseLandmarkDensityRange.RandomInRange;

            float pollution = settings.pollution;
            if (settings.randomisePollution)
                pollution = settings.randomisePollutionRange.RandomInRange;

            List<FactionDef> worldFactions = new List<FactionDef>();

            IEnumerable<FactionDef> allFactionDefs =
                DefDatabase<FactionDef>.AllDefsListForReading.Where((FactionDef x) => x.isPlayer);

            bool needToWriteSettings = false;

            List<string> factionsAlwaysAdd = new List<string>(settings.factionsAlwaysAdd);
            foreach (string factionDefName in factionsAlwaysAdd)
            {
                FactionDef faction = DefDatabase<FactionDef>.GetNamed(factionDefName, false);
                if (faction == null)
                {
                    settings.factionsAlwaysAdd.Remove(factionDefName);
                    Util.LogMessage($"Tried to create invalid Faction {faction}. Removing from list.");
                    needToWriteSettings = true;
                    continue;
                }
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
                    {
                        settings.factionsRandomlyAdd.Remove(randomFaction);
                        randomFactions.Remove(randomFaction);
                        Util.LogMessage($"Tried to create invalid Faction {randomFaction}. Removing from list.");
                        needToWriteSettings = true;
                        if (randomFactions.Count == 0)
                            break;
                        continue;
                    }
                    worldFactions.Add(faction);
                    if (settings.uniqueFactions)
                    {
                        randomFactions.Remove(randomFaction);
                        if (randomFactions.Count == 0)
                            break;
                    }
                }
            }

            if (needToWriteSettings)
            {
                settings.Write();
            }

            if (ModsConfig.IsActive("OskarPotocki.VFE.Empire") || ModsConfig.IsActive("OskarPotocki.VFE.Empire_Steam"))
            {
                Compat.VFEECompat.EnsureScenarioFactions(worldFactions);
            }

            if (ModsConfig.IsActive("OskarPotocki.VFE.Deserters") || ModsConfig.IsActive("OskarPotocki.VFE.Deserters_Steam"))
            {
                Compat.VFEDCompat.EnsureScenarioFactions(worldFactions);
            }

            if (ModsConfig.IsActive("kentington.saveourship2") || ModsConfig.IsActive("kentington.saveourship2_steam"))
            {
                Compat.SOS2Compat.SetupForStartInSpace();
            }

            Util.LogMessage($"Created {worldFactions.Count} factions");

            if (ModsConfig.IsActive("zvq.RealisticPlanetsContinued") || ModsConfig.IsActive("kentington.RealisticPlanetsContinued_Steam"))
            {
                int worldType = settings.realisticPlanetsWorldType;
                if (settings.randomiseRealisticPlanets)
                    worldType = -1;
                Compat.RealisticPlanetsCompat.GenerateRealisticPlanetWorld(
                    settings.planetCoverage,
                    GenText.RandomSeedString(),
                    rainfall,
                    temperature,
                    population,
                    LandmarkDensity.Normal,
                    worldFactions,
                    pollution,
                    worldType
                );
            }
            else
            {
                Current.Game.World = WorldGenerator.GenerateWorld(
                    settings.planetCoverage,
                    GenText.RandomSeedString(),
                    rainfall,
                    temperature,
                    population,
                    LandmarkDensity.Normal,
                    worldFactions,
                    pollution
                );
            }
        }

        private static void SetupRandomIdeology(RandomStartSettings settings)
        {
            if (settings.disableIdeo)
            {
                Find.IdeoManager.classicMode = true;
                IdeoGenerationParms genParms = new IdeoGenerationParms(
                    Find.FactionManager.OfPlayer.def
                );
                if (
                    !DefDatabase<CultureDef>
                        .AllDefs.Where(
                            (CultureDef x) =>
                                Find.FactionManager.OfPlayer.def.allowedCultures.Contains(x)
                        )
                        .TryRandomElement(out var result)
                )
                {
                    result = DefDatabase<CultureDef>.AllDefs.RandomElement();
                }
                Ideo classicIdeo = IdeoGenerator.GenerateClassicIdeo(
                    result,
                    genParms,
                    noExpansionIdeo: false
                );
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
                    PreLoadUtility.CheckVersionAndLoad(
                        settings.customIdeoOverrideFile,
                        ScribeMetaHeaderUtility.ScribeHeaderMode.Ideo,
                        delegate
                        {
                            if (
                                GameDataSaveLoader.TryLoadIdeo(
                                    settings.customIdeoOverrideFile,
                                    out var ideo
                                )
                            )
                            {
                                newIdeo = IdeoGenerator.InitLoadedIdeo(ideo);
                            }
                        }
                    );

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
                        Log.Error(
                            $"[Random Start] Couldn't load ideo file {settings.customIdeoOverrideFile.Split('\\').Last()} - was it saved with different mods?"
                        );
                    }
                }
                if (settings.fluidIdeo)
                {
                    Ideo playerIdeo =
                        Find.FactionManager.OfPlayer.ideos.AllIdeos.FirstOrDefault();
                    playerIdeo.Fluid = true;
                }
            }
        }


        private static void SetupRandomGenes(RandomStartSettings settings)
        {
            if (settings.enableRandomXenotypes)
            {
                foreach (Pawn p in Find.GameInitData.startingAndOptionalPawns)
                {
                    IEnumerable<XenotypeDef> xenotypes =
                        DefDatabase<XenotypeDef>.AllDefsListForReading;
                    if (
                        settings.respectFactionXenotypes
                        && Find.FactionManager.OfPlayer.def.basicMemberKind.xenotypeSet != null
                    )
                        xenotypes = xenotypes.Where(
                            (XenotypeDef x) =>
                                Find.FactionManager.OfPlayer.def.basicMemberKind.xenotypeSet.Contains(
                                    x
                                )
                        );
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
                        selectedGenes.Add(
                            DefDatabase<GeneDef>
                                .AllDefsListForReading.Where(
                                    (GeneDef g) => g.canGenerateInGeneSet
                                )
                                .RandomElement()
                        );
                    }
                    if (selectedGenes.Count > 0)
                    {
                        int metabolismTotal = 0;
                        foreach (GeneDef geneDef in selectedGenes)
                        {
                            if (settings.enableMetabolicEfficiencyMinimum)
                            {
                                if ((metabolismTotal + geneDef.biostatMet) >= settings.minimumMetabolicEfficiency)
                                {
                                    GivePawnGene(p, geneDef, ref metabolismTotal);
                                    metabolismTotal += geneDef.biostatMet;
                                    foreach (Gene gene in p.genes.GenesListForReading)
                                    {
                                        if (gene.Overridden)
                                        {
                                            p.genes.RemoveGene(gene);
                                            metabolismTotal -= gene.def.biostatMet;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                GivePawnGene(p, geneDef, ref metabolismTotal);
                                foreach (Gene gene in p.genes.GenesListForReading)
                                {
                                    if (gene.Overridden)
                                    {
                                        p.genes.RemoveGene(gene);
                                        metabolismTotal -= gene.def.biostatMet;
                                    }
                                }
                            }

                        }


                        p.genes.xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(
                            selectedGenes
                        );
                        p.genes.iconDef = DefDatabase<XenotypeIconDef>.GetRandom();
                    }
                }
            }
        }

        private static void GivePawnGene(Pawn p, GeneDef geneDef, ref int metabolismTotal)
        {
            p.genes.AddGene(geneDef, Rand.Bool);
            Gene gene = GeneMaker.MakeGene(geneDef, p);
            p.genes.OverrideAllConflicting(gene);
            metabolismTotal += geneDef.biostatMet;
            if (geneDef.prerequisite != null)
            {
                GivePawnGene(p, geneDef.prerequisite, ref metabolismTotal);
            }
        }

        private static void SetupRandomResearch(RandomStartSettings settings)
        {
            if (settings.addRandomResearch)
            {
                List<ResearchProjectDef> possibleProjects = DefDatabase<ResearchProjectDef>
                    .AllDefsListForReading.Where(def =>
                        (
                            (int)def.techLevel <= settings.randomResearchTechLevelLimit
                            && def.tab.label != "anomaly"
                        )
                    )
                    .ToList();
                if (possibleProjects.Count > 0)
                {
                    for (int i = 0; i < settings.randomResearchRange.RandomInRange; i++)
                    {
                        ResearchProjectDef projectDef = possibleProjects.RandomElement();
                        possibleProjects.Remove(projectDef);
                        Find.ResearchManager.FinishProject(
                            projectDef,
                            doCompletionDialog: false,
                            null,
                            doCompletionLetter: false
                        );
                    }
                }

            }
        }
    }
}
