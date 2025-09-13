using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using RandomStartMod.Compat;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VEF;
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
            Current.Game = new Game { InitData = new GameInitData() };
            SetupRandomScenario(settings);
            SetupRandomDifficulty(settings);
            SetupRandomPlanet(settings);

            // Handle starting tile filtering
            bool hasFilters =
                (
                    settings.filterStartingBiome
                    && settings.allowedBiomes != null
                    && settings.allowedBiomes.Count > 0
                )
                || (
                    settings.filterStartingHilliness
                    && settings.allowedHilliness != null
                    && settings.allowedHilliness.Count > 0
                );

            if (hasFilters)
            {
                bool foundValidTile = false;
                List<BiomeDef> filterBiomes = new List<BiomeDef>();
                List<Hilliness> filterHilliness = new List<Hilliness>();

                // Setup biome filtering
                if (
                    settings.filterStartingBiome
                    && settings.allowedBiomes != null
                    && settings.allowedBiomes.Count > 0
                )
                {
                    Util.LogMessage("Filtering starting tile by allowed biomes");
                    foreach (string biomeDefName in settings.allowedBiomes)
                    {
                        BiomeDef biomeDef = BiomeDef.Named(biomeDefName);
                        if (biomeDef != null)
                        {
                            filterBiomes.Add(biomeDef);
                        }
                    }
                }

                // Setup hilliness filtering
                if (
                    settings.filterStartingHilliness
                    && settings.allowedHilliness != null
                    && settings.allowedHilliness.Count > 0
                )
                {
                    Util.LogMessage("Filtering starting tile by allowed hilliness");
                    foreach (int hillinessValue in settings.allowedHilliness)
                    {
                        if (hillinessValue >= 1 && hillinessValue <= 4) // Flat, SmallHills, LargeHills, Mountainous
                        {
                            filterHilliness.Add((Hilliness)hillinessValue);
                        }
                    }
                }

                // Find tiles that match all enabled filters
                List<Tile> filteredTiles = Find.WorldGrid.Surface.tiles.FindAll(tile =>
                {
                    // Check biome filter (if enabled)
                    bool biomeMatch =
                        !settings.filterStartingBiome
                        || filterBiomes.Count == 0
                        || filterBiomes.Contains(tile.biome);

                    // Check hilliness filter (if enabled)
                    bool hillinessMatch =
                        !settings.filterStartingHilliness
                        || filterHilliness.Count == 0
                        || filterHilliness.Contains(tile.hilliness);

                    bool temperatureRangeMatch = !settings.limitStartingTileTemperature || (tile.MinTemperature <= settings.limitStartingTileTemperatureRange.min && tile.MaxTemperature <= settings.limitStartingTileTemperatureRange.max);

                    return biomeMatch && hillinessMatch && temperatureRangeMatch;
                });

                foreach (Tile filteredTile in filteredTiles)
                {
                    if (
                        !filteredTile.PrimaryBiome.canBuildBase
                        || !filteredTile.PrimaryBiome.implemented
                        || filteredTile.hilliness == Hilliness.Impassable
                    )
                    {
                        continue;
                    }

                    if (!filteredTile.PrimaryBiome.canAutoChoose)
                    {
                        continue;
                    }

                    PlanetTile filteredPlanetTile = Find.WorldGrid
                        .Surface[Find.WorldGrid.Surface.tiles.IndexOf(filteredTile)]
                        .tile;

                    if (TileFinder.IsValidTileForNewSettlement(filteredPlanetTile))
                    {
                        Find.GameInitData.startingTile = filteredPlanetTile;
                        string biomeInfo = filteredPlanetTile.Tile.biome.label.CapitalizeFirst();
                        string hillinessInfo = filteredTile.hilliness.GetLabelCap();
                        Util.LogMessage(
                            $"Found valid tile for random start: {biomeInfo}, {hillinessInfo}"
                        );
                        foundValidTile = true;
                        break;
                    }
                }

                if (!foundValidTile)
                {
                    Util.LogMessage(
                        "No valid tile found for applied filters. Choosing random tile."
                    );
                    Find.GameInitData.ChooseRandomStartingTile();
                }
            }
            else
            {
                // No filters applied, choose random tile
                Find.GameInitData.ChooseRandomStartingTile();
            }

            Util.LogMessage(
                $"Starting in {Find.WorldGrid[Find.GameInitData.startingTile].biome.label.CapitalizeFirst()}"
            );

            Season startingSeason = (Season)settings.startingSeason;
            if (settings.randomiseSeason)
                startingSeason = (Season)Rand.Range(1, 4);

            Find.GameInitData.startingSeason = startingSeason;
            Find.GameInitData.mapSize = settings.mapSize;

            if (
                ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core")
                || ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core_Steam")
            )
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

            if (settings.startingPawnForceViolence)
            {
                StartingPawnUtility.ClearAllStartingPawns();
                for (int i = 0; i < 10; i++)
                {
                    PawnGenerationRequest request = StartingPawnUtility.DefaultStartingPawnRequest;
                    request.MustBeCapableOfViolence = true;
                    StartingPawnUtility.StartingAndOptionalPawnGenerationRequests.Add(request);
                    StartingPawnUtility.AddNewPawn(i);
                }
            }

            if (!settings.randomisePawnAge || !settings.randomisePawnSex || !settings.randomisePawnName)
            {
                if (ModsConfig.IsActive("Lakuna.PrepareModerately") || ModsConfig.IsActive("Lakuna.PrepareModerately_Steam"))
                {
                    PrepareModeratelyCompat.SetMod();
                }

                Util.LogMessage("Randomizing starting pawn");
                Pawn pawn = Find.GameInitData.startingAndOptionalPawns.FirstOrDefault();
                int randomCount = 0;
                for (randomCount = 0; randomCount < 100; randomCount++)
                {
                    if (settings.PawnNotDisabledWorkTags && pawn.GetDisabledWorkTypes(true).Count > 0)
                    {
                        pawn = Util.RandomizePawn();
                        continue;
                    }
                    if (settings.randomisePawnAge == false && (pawn.ageTracker.AgeBiologicalYears < settings.randomisePawnAgeRange.min || pawn.ageTracker.AgeBiologicalYears > settings.randomisePawnAgeRange.max))
                    {
                        pawn = Util.RandomizePawn();
                        continue;
                    }
                    if (settings.randomisePawnSex == false && pawn.gender != (Gender)settings.PawnSex)
                    {
                        pawn = Util.RandomizePawn();
                        continue;
                    }
                    Util.LogMessage($"Pawn after {randomCount} random");
                    break;
                }
                if (randomCount > 99)
                {
                    Util.LogMessage("After 100 random, none pawn the target criteria");
                }
                if (!settings.randomisePawnName && pawn.Name is NameTriple nameTriple)
                {
                    pawn.Name = new NameTriple(
                        string.IsNullOrWhiteSpace(settings.PawnFirstName) ? nameTriple.First : settings.PawnFirstName,
                        string.IsNullOrWhiteSpace(settings.PawnNickName) ? nameTriple.Nick : settings.PawnNickName,
                        string.IsNullOrWhiteSpace(settings.PawnLastName) ? nameTriple.Last : settings.PawnLastName
                        );
                }
            }

            if (ModsConfig.BiotechActive)
            {
                SetupRandomGenes(settings);
            }

            Find.GameInitData.startedFromEntry = true;

            if (settings.randomiseFactionGoodwill)
            {
                Util.LogMessage("Randomising Faction Goodwill");
                foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
                {
                    // Skip faction if it's excluded from reputation randomization
                    if (settings.factionsExcludeFromReputationRandomization != null &&
                        settings.factionsExcludeFromReputationRandomization.Contains(item.def.defName))
                    {
                        continue;
                    }

                    // Only remove relations for factions that aren't excluded
                    List<FactionRelation> relationsToRemove = new List<FactionRelation>();
                    foreach (FactionRelation relation in item.relations)
                    {
                        if (settings.factionsExcludeFromReputationRandomization == null ||
                            !settings.factionsExcludeFromReputationRandomization.Contains(relation.other.def.defName))
                        {
                            relationsToRemove.Add(relation);
                        }
                    }
                    foreach (FactionRelation relation in relationsToRemove)
                    {
                        item.relations.Remove(relation);
                    }

                    foreach (Faction item2 in Find.FactionManager.AllFactionsListForReading)
                    {
                        if (item != item2)
                        {
                            // Skip if either faction is excluded from reputation randomization
                            if (settings.factionsExcludeFromReputationRandomization != null &&
                                (settings.factionsExcludeFromReputationRandomization.Contains(item.def.defName) ||
                                 settings.factionsExcludeFromReputationRandomization.Contains(item2.def.defName)))
                            {
                                continue;
                            }

                            if (item.RelationWith(item2, allowNull: true) == null)
                            {
                                int initialGoodwill = GetInitialGoodwill(item, item2);

                                FactionRelationKind kind = (
                                    (initialGoodwill > -10)
                                        ? (
                                            (initialGoodwill < 75)
                                                ? FactionRelationKind.Neutral
                                                : FactionRelationKind.Ally
                                        )
                                        : FactionRelationKind.Hostile
                                );
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
                                if (
                                    (a.def.permanentEnemyToEveryoneExceptPlayer && !b.IsPlayer)
                                    || (b.def.permanentEnemyToEveryoneExceptPlayer && !a.IsPlayer)
                                )
                                {
                                    return -100;
                                }
                                if (
                                    a.def.permanentEnemyToEveryoneExcept != null
                                    && !a.def.permanentEnemyToEveryoneExcept.Contains(b.def)
                                )
                                {
                                    return -100;
                                }
                                if (
                                    b.def.permanentEnemyToEveryoneExcept != null
                                    && !b.def.permanentEnemyToEveryoneExcept.Contains(a.def)
                                )
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

            if (
                ModsConfig.IsActive("Woolstrand.RealRuins")
                || ModsConfig.IsActive("Woolstrand.RealRuins_Steam")
            )
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
                Current.Game.Scenario = ScenarioMaker.GenerateNewRandomScenario(
                    GenText.RandomSeedString()
                );
            }
            else
            {
                List<Scenario> scenarios = new List<Scenario>();

                scenarios.AddRange(
                    ScenarioLister
                        .AllScenarios()
                        .Where(
                            (Scenario scenario) =>
                                !settings.disabledScenarios.Contains(scenario.name)
                                && scenario.showInUI
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

            if (
                ModsConfig.IsActive("brrainz.nopausechallenge")
                || ModsConfig.IsActive("brrainz.nopausechallenge_steam")
            )
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

            LandmarkDensity landmarkDensity = (LandmarkDensity)settings.landmarkDensity;
            if (settings.randomiseLandmarkDensity)
                landmarkDensity = (LandmarkDensity)settings.randomiseLandmarkDensityRange.RandomInRange;

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
                    Util.LogMessage(
                        $"Tried to create invalid Faction {faction}. Removing from list."
                    );
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
                        Util.LogMessage(
                            $"Tried to create invalid Faction {randomFaction}. Removing from list."
                        );
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

            if (
                ModsConfig.IsActive("OskarPotocki.VFE.Empire")
                || ModsConfig.IsActive("OskarPotocki.VFE.Empire_Steam")
            )
            {
                Compat.VFEECompat.EnsureScenarioFactions(worldFactions);
            }

            if (
                ModsConfig.IsActive("OskarPotocki.VFE.Deserters")
                || ModsConfig.IsActive("OskarPotocki.VFE.Deserters_Steam")
            )
            {
                Compat.VFEDCompat.EnsureScenarioFactions(worldFactions);
            }

            if (
                ModsConfig.IsActive("kentington.saveourship2")
                || ModsConfig.IsActive("kentington.saveourship2_steam")
            )
            {
                Compat.SOS2Compat.SetupForStartInSpace();
            }

            Util.LogMessage($"Created {worldFactions.Count} factions");

            string worldSeed;
            if (settings.randomiseWorldSeed)
            {
                Util.LogMessage("Randomising world seed");
                worldSeed = GenText.RandomSeedString();
            }
            else
            {
                if (string.IsNullOrEmpty(settings.worldSeed))
                {
                    Util.LogMessage($"World Seed: '{settings.worldSeed}' is invalid");
                    worldSeed = GenText.RandomSeedString();
                    settings.worldSeed = worldSeed;
                }
                else
                {
                    Util.LogMessage("Using custom world seed");
                    worldSeed = settings.worldSeed;
                }
            }

            Util.LogMessage($"Using world seed: '{worldSeed}'");

            if (
                ModsConfig.IsActive("koth.RealisticPlanets1.6")
                || ModsConfig.IsActive("koth.RealisticPlanets1.6_Steam")
            )
            {
                int oceanType = settings.realisticPlanetsOceanType;
                if (settings.randomiseOceanType)
                {
                    oceanType = settings.randomiseOceanTypeRange.RandomInRange;
                }
                int axialTilt = settings.realisticPlanetsAxialTilt;
                if (settings.randomiseAxialTilt)
                {
                    axialTilt = settings.randomiseAxialTiltRange.RandomInRange;
                }
                RealisticPlanetsCompat.GenerateRealisticPlanetWorld(settings.planetCoverage, GenText.RandomSeedString(), rainfall, temperature, population, landmarkDensity, worldFactions, pollution, oceanType, axialTilt, settings.realisticPlanetsWorldType, settings.randomiseRealisticPlanets);
            }
            else
            {
                Current.Game.World = WorldGenerator.GenerateWorld(
                    settings.planetCoverage,
                    worldSeed,
                    rainfall,
                    temperature,
                    population,
                    landmarkDensity,
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
                Ideo newIdeo = null;
                if (settings.overrideIdeo && File.Exists(settings.customIdeoOverrideFile))
                {
                    Util.LogMessage("Attempting to load ideo file");
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
                    }
                    else
                    {
                        Log.Error(
                            $"[Random Start] Couldn't load ideo file {settings.customIdeoOverrideFile.Split('\\').Last()} - was it saved with different mods?"
                        );
                    }
                }
                else
                {
                    // Convert string lists to actual defs
                    List<PreceptDef> disallowedPreceptDefs = null;
                    if (settings.disallowedPrecepts?.Count > 0)
                    {
                        disallowedPreceptDefs = new List<PreceptDef>();
                        foreach (string preceptDefName in settings.disallowedPrecepts)
                        {
                            PreceptDef preceptDef = DefDatabase<PreceptDef>.GetNamed(preceptDefName, false);
                            if (preceptDef != null)
                            {
                                disallowedPreceptDefs.Add(preceptDef);
                            }
                        }
                    }

                    List<MemeDef> disallowedMemeDefs = null;
                    if (settings.disallowedMemes?.Count > 0)
                    {
                        disallowedMemeDefs = new List<MemeDef>();
                        foreach (string memeDefName in settings.disallowedMemes)
                        {
                            MemeDef memeDef = DefDatabase<MemeDef>.GetNamed(memeDefName, false);
                            if (memeDef != null)
                            {
                                disallowedMemeDefs.Add(memeDef);
                            }
                        }
                    }

                    List<MemeDef> forcedMemeDefs = null;
                    if (settings.forcedMemes?.Count > 0)
                    {
                        forcedMemeDefs = new List<MemeDef>();
                        foreach (string memeDefName in settings.forcedMemes)
                        {
                            MemeDef memeDef = DefDatabase<MemeDef>.GetNamed(memeDefName, false);
                            if (memeDef != null)
                            {
                                forcedMemeDefs.Add(memeDef);
                            }
                        }
                    }

                    IdeoGenerationParms genParms = new IdeoGenerationParms(
                        Find.FactionManager.OfPlayer.def,
                        forceNoExpansionIdeo: false,
                        disallowedPreceptDefs,
                        disallowedMemeDefs,
                        forcedMemeDefs, // this overrides memes, rather than adding to them
                        classicExtra: false,
                        forceNoWeaponPreference: false,
                        settings.fluidIdeo
                    );
                    newIdeo = IdeoGenerator.GenerateIdeo(genParms);
                    newIdeo.memes.Clear();
                    int memeCount = settings.randomMemeRange.RandomInRange;
                    newIdeo.memes.AddRange(GenerateRandomMemes(memeCount, genParms));
                    newIdeo.SortMemesInDisplayOrder();
                    newIdeo.foundation.RandomizePrecepts(true, genParms);
                }

                Find.FactionManager.OfPlayer.ideos.RemoveAll();
                Find.FactionManager.OfPlayer.ideos.SetPrimary(newIdeo);
                Find.IdeoManager.RemoveUnusedStartingIdeos();
                newIdeo.initialPlayerIdeo = true;
                Find.IdeoManager.Add(newIdeo);
            }
        }

        private static List<MemeDef> GenerateRandomMemes(int count, IdeoGenerationParms parms)
        {
            FactionDef forFaction = parms.forFaction;
            bool forPlayerFaction = forFaction != null && forFaction.isPlayer;
            List<MemeDef> memes = new List<MemeDef>();
            bool flag = false;
            if (forFaction != null && forFaction.requiredMemes != null)
            {
                for (int i = 0; i < forFaction.requiredMemes.Count; i++)
                {
                    memes.Add(forFaction.requiredMemes[i]);
                    if (forFaction.requiredMemes[i].category == MemeCategory.Normal)
                    {
                        count--;
                    }
                    else if (forFaction.requiredMemes[i].category == MemeCategory.Structure)
                    {
                        flag = true;
                    }
                }
            }

            if (parms.forcedMemes != null)
            {
                foreach (MemeDef forcedMeme in parms.forcedMemes)
                {
                    if (forcedMeme.category == MemeCategory.Structure)
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (forFaction != null && forFaction.structureMemeWeights != null && !flag)
            {
                MemeWeight result2;
                if (forFaction.structureMemeWeights.Where((MemeWeight x) => IdeoUtility.CanAdd(x.meme, memes, forFaction, parms.forNewFluidIdeo) && (forPlayerFaction || !IdeoUtility.AnyIdeoHas(x.meme))).TryRandomElementByWeight((MemeWeight x) => x.selectionWeight * x.meme.randomizationSelectionWeightFactor, out var result))
                {
                    memes.Add(result.meme);
                    flag = true;
                }
                else if (forFaction.structureMemeWeights.Where((MemeWeight x) => IdeoUtility.CanAdd(x.meme, memes, forFaction, parms.forNewFluidIdeo)).TryRandomElementByWeight((MemeWeight x) => x.selectionWeight * x.meme.randomizationSelectionWeightFactor, out result2))
                {
                    memes.Add(result2.meme);
                    flag = true;
                }
            }

            if (!flag)
            {
                MemeDef result4;
                if (DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Structure && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo) && (forPlayerFaction || !IdeoUtility.AnyIdeoHas(x))).TryRandomElement(out var result3))
                {
                    memes.Add(result3);
                }
                else if (DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Structure && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo)).TryRandomElementByWeight((MemeDef x) => x.randomizationSelectionWeightFactor, out result4))
                {
                    memes.Add(result4);
                }
            }

            if (parms.forcedMemes != null)
            {
                memes.AddRange(parms.forcedMemes);
            }

            for (int num = memes.Count; num <= count; num++)
            {
                MemeDef result6;
                if (DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Normal && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo) && (forPlayerFaction || !IdeoUtility.AnyIdeoHas(x)) && (parms.disallowedMemes == null || !parms.disallowedMemes.Contains(x))).TryRandomElementByWeight((MemeDef x) => x.randomizationSelectionWeightFactor, out var result5))
                {
                    memes.Add(result5);
                }
                else if (DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => x.category == MemeCategory.Normal && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo) && (parms.disallowedMemes == null || !parms.disallowedMemes.Contains(x))).TryRandomElementByWeight((MemeDef x) => x.randomizationSelectionWeightFactor, out result6))
                {
                    memes.Add(result6);
                }
            }
            return memes;
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
                                .AllDefsListForReading.Where((GeneDef g) => g.canGenerateInGeneSet)
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
                                if (
                                    (metabolismTotal + geneDef.biostatMet)
                                    >= settings.minimumMetabolicEfficiency
                                )
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
