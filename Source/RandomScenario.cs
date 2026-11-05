using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RandomStartMod.Compat;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RandomStartMod
{
    public static class RandomScenario
    {
        public static void SetupForRandomPlay()
        {
            try { SetupForRandomPlayInner(); }
            catch { RandomStartData.End(Current.Game); throw; }
        }

        private static void SetupForRandomPlayInner()
        {
            Util.LogMessage($"Randomising Scenario");
            RandomStartSettings settings = LoadedModManager
                .GetMod<RandomStartMod>()
                .GetSettings<RandomStartSettings>();

            Current.ProgramState = ProgramState.Entry;
            Current.Game = new Game { InitData = new GameInitData() };
            RandomStartData.Begin(settings);
            settings = RandomStartData.State.Settings;
            SetupRandomScenario(settings);
            StartingItems.Prepare(Find.Scenario, settings);
            Find.Scenario.PreConfigure();
            Page firstPage = Find.Scenario.GetFirstConfigPage();
            bool showPages = settings.useNormalSetupPages;
            var visited = new HashSet<Page>();
            for (Page page = firstPage; page != null; page = page.next)
            {
                if (!visited.Add(page)) throw new InvalidOperationException("Scenario configuration pages form a cycle.");
                if (page.GetType().Assembly != typeof(Page).Assembly) showPages = true;
            }
            if (showPages)
            {
                if (firstPage == null) throw new InvalidOperationException("This scenario has no configuration pages.");
                // Keep the copied scenario/item plan and use the real page chain.
                Find.WindowStack.Add(firstPage);
                if (!settings.useNormalSetupPages)
                    Find.WindowStack.Add(new Dialog_MessageBox("RandomStartMod.CustomSetupPages".Translate()));
                return;
            }
            SetupRandomDifficulty(settings);
            SetupRandomPlanet(settings);

            var biomes = settings.filterStartingBiome
                ? (settings.allowedBiomes ?? new List<string>()).Select(name => DefDatabase<BiomeDef>.GetNamedSilentFail(name))
                    .Where(biome => biome != null)
                : Enumerable.Empty<BiomeDef>();
            var hills = settings.filterStartingHilliness
                ? (settings.allowedHilliness ?? new List<int>()).Where(value => value >= 1 && value <= 4).Select(value => (Hilliness)value)
                : Enumerable.Empty<Hilliness>();
            var filters = new StartingTileFilters<BiomeDef, Hilliness>(biomes, hills,
                settings.limitStartingTileTemperature, settings.limitStartingTileTemperatureRange.min,
                settings.limitStartingTileTemperatureRange.max);
            var surface = Find.WorldGrid.Surface;
            int selected;
            if (filters.Active && RandomSelection.TryChooseIndex(surface.tiles.Count, index =>
            {
                Tile tile = surface.tiles[index];
                return tile.PrimaryBiome != null && tile.PrimaryBiome.canBuildBase
                    && tile.PrimaryBiome.implemented && tile.PrimaryBiome.canAutoChoose
                    && tile.hilliness != Hilliness.Impassable
                    && filters.Matches(tile.biome, tile.hilliness, tile.temperature);
            }, index => TileFinder.IsValidTileForNewSettlement(surface[index].tile),
                upper => Rand.Range(0, upper), out selected))
                Find.GameInitData.startingTile = surface[selected].tile;
            else
            {
                if (filters.Active) Util.LogMessage("No valid tile matched the starting filters; using the game's random starting tile.");
                Find.GameInitData.ChooseRandomStartingTile();
            }

            Util.LogMessage(
                $"Starting in {Find.WorldGrid[Find.GameInitData.startingTile].biome.label.CapitalizeFirst()}"
            );

            Season startingSeason = (Season)settings.startingSeason;
            if (settings.randomiseSeason)
                startingSeason = new[] { Season.Spring, Season.Summer, Season.Fall, Season.Winter }.RandomElement();

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

            Find.Scenario.PostIdeoChosen();

            Find.GameInitData.startedFromEntry = true;

            if (settings.randomiseFactionGoodwill)
            {
                var excluded = new HashSet<string>(settings.factionsExcludeFromReputationRandomization ?? new List<string>());
                var factions = Find.FactionManager.AllFactionsListForReading.Distinct().ToList();
                Pairwise.ForEach(factions, faction => excluded.Contains(faction.def.defName), (a, b) =>
                {
                    // Leave fixed/non-diplomatic relationships under the game's rules.
                    if ((!a.IsPlayer && !a.HasGoodwill) || (!b.IsPlayer && !b.HasGoodwill)
                        || a.def.permanentEnemy || b.def.permanentEnemy
                        || (a.def.permanentEnemyToEveryoneExceptPlayer && !b.IsPlayer)
                        || (b.def.permanentEnemyToEveryoneExceptPlayer && !a.IsPlayer)
                        || (a.def.permanentEnemyToEveryoneExcept != null && !a.def.permanentEnemyToEveryoneExcept.Contains(b.def))
                        || (b.def.permanentEnemyToEveryoneExcept != null && !b.def.permanentEnemyToEveryoneExcept.Contains(a.def))) return;
                    if (a.RelationWith(b, true) == null || b.RelationWith(a, true) == null)
                        a.TryMakeInitialRelationsWith(b);
                    if (a.RelationWith(b, true) == null || b.RelationWith(a, true) == null) return;
                    int desired = Rand.RangeInclusive(-10, 10) * 10;
                    // This API maintains the bilateral relation and applies modded
                    // goodwill rules. Do not delete either faction's relation list.
                    a.TryAffectGoodwillWith(b, desired - a.BaseGoodwillWith(b), false, false);
                });
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

        internal static void FinishStartingPawnSetup(RandomStartSettings settings)
        {
            bool usePrepareModerately = settings.enablePrepareModeratelyIntegration && PrepareModeratelyCompat.IsAvailable;
            if (usePrepareModerately)
            {
                if (!string.IsNullOrEmpty(settings.selectedPrepareModeratelyFilter))
                    PrepareModeratelyCompat.SetCurrentFilter(settings.selectedPrepareModeratelyFilter);
                PrepareModeratelyCompat.RandomiseStartingPawns();
            }
            else if (!settings.randomisePawnName && settings.pawnNames != null)
            {
                var pawns = Find.GameInitData.startingAndOptionalPawns;
                for (int i = 0; i < pawns.Count && i < settings.pawnNames.Count; i++)
                {
                    PawnNameData name = settings.pawnNames[i];
                    if (name == null || name.IsEmpty() || !pawns[i].RaceProps.Humanlike) continue;
                    NameTriple previous = pawns[i].Name as NameTriple ?? new NameTriple("", "", "");
                    pawns[i].Name = new NameTriple(
                        string.IsNullOrWhiteSpace(name.firstName) ? previous.First : name.firstName,
                        string.IsNullOrWhiteSpace(name.nickName) ? previous.Nick : name.nickName,
                        string.IsNullOrWhiteSpace(name.lastName) ? previous.Last : name.lastName);
                }
            }
            if (ModsConfig.BiotechActive) SetupRandomGenes(settings);
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
                        .ScenariosInCategory(ScenarioCategory.FromDef)
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

                Scenario randomScenario = scenarios.RandomElementWithFallback();
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

                Current.Game.Scenario = randomScenario.CopyForEditing();
            }
            Util.LogMessage($"Starting {Current.Game.Scenario}");
        }

        private static void SetupRandomDifficulty(RandomStartSettings settings)
        {
            DifficultyDef chosenDifficultyDef = DefDatabase<DifficultyDef>.GetNamedSilentFail(settings.difficulty)
                ?? DefDatabase<DifficultyDef>.GetNamedSilentFail("Rough")
                ?? DefDatabase<DifficultyDef>.AllDefs.First();
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
                difficulty.threatScale = settings.threatScale;
                difficulty.allowBigThreats = settings.allowBigThreats;
                difficulty.allowIntroThreats = settings.allowIntroThreats;
                difficulty.allowCaveHives = settings.allowCaveHives;
                difficulty.peacefulTemples = settings.peacefulTemples;
                difficulty.allowViolentQuests = settings.allowViolentQuests;
                difficulty.predatorsHuntHumanlikes = settings.predatorsHuntHumanlikes;
                difficulty.scariaRotChance = settings.scariaRotChance;
                difficulty.colonistMoodOffset = settings.colonistMoodOffset;
                difficulty.tradePriceFactorLoss = settings.tradePriceFactorLoss;
                difficulty.cropYieldFactor = settings.cropYieldFactor;
                difficulty.mineYieldFactor = settings.mineYieldFactor;
                difficulty.butcherYieldFactor = settings.butcherYieldFactor;
                difficulty.researchSpeedFactor = settings.researchSpeedFactor;
                difficulty.diseaseIntervalFactor = settings.diseaseIntervalFactor;
                difficulty.enemyReproductionRateFactor =
                    settings.enemyReproductionRateFactor;
                difficulty.playerPawnInfectionChanceFactor =
                    settings.playerPawnInfectionChanceFactor;
                difficulty.manhunterChanceOnDamageFactor =
                    settings.manhunterChanceOnDamageFactor;
                difficulty.deepDrillInfestationChanceFactor =
                    settings.deepDrillInfestationChanceFactor;
                difficulty.wastepackInfestationChanceFactor =
                    settings.wastepackInfestationChanceFactor;
                difficulty.foodPoisonChanceFactor = settings.foodPoisonChanceFactor;
                difficulty.maintenanceCostFactor = settings.maintenanceCostFactor;
                difficulty.enemyDeathOnDownedChanceFactor =
                    settings.enemyDeathOnDownedChanceFactor;
                difficulty.adaptationGrowthRateFactorOverZero =
                    settings.adaptationGrowthRateFactorOverZero;
                difficulty.adaptationEffectFactor = settings.adaptationEffectFactor;
                difficulty.questRewardValueFactor = settings.questRewardValueFactor;
                difficulty.raidLootPointsFactor = settings.raidLootPointsFactor;
                difficulty.allowTraps = settings.allowTraps;
                difficulty.allowTurrets = settings.allowTurrets;
                difficulty.allowMortars = settings.allowMortars;
                difficulty.classicMortars = settings.classicMortars;
                difficulty.allowExtremeWeatherIncidents =
                    settings.allowExtremeWeatherIncidents;
                difficulty.fixedWealthMode = settings.fixedWealthMode;
                difficulty.lowPopConversionBoost = settings.lowPopConversionBoost;
                difficulty.minThreatPointsRangeCeiling =
                    settings.minThreatPointsRangeCeiling;
                difficulty.babiesAreHealthy = settings.babiesAreHealthy;
                difficulty.noBabiesOrChildren = settings.noBabiesOrChildren;
                difficulty.childAgingRate = settings.childAgingRate;
                difficulty.adultAgingRate = settings.adultAgingRate;
                difficulty.unwaveringPrisoners = settings.unwaveringPrisoners;
                difficulty.childRaidersAllowed = settings.childRaidersAllowed;
                difficulty.anomalyThreatsInactiveFraction =
                    settings.anomalyThreatsInactiveFraction;
                difficulty.anomalyThreatsActiveFraction =
                    settings.anomalyThreatsActiveFraction;
                difficulty.studyEfficiencyFactor = settings.studyEfficiencyFactor;
            }



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
                landmarkDensity = (LandmarkDensity)
                    settings.randomiseLandmarkDensityRange.RandomInRange;

            float pollution = settings.pollution;
            if (settings.randomisePollution)
                pollution = settings.randomisePollutionRange.RandomInRange;

            List<FactionDef> worldFactions = new List<FactionDef>();

            foreach (string name in settings.factionsAlwaysAdd ?? new List<string>())
            {
                FactionDef faction = DefDatabase<FactionDef>.GetNamedSilentFail(name);
                if (faction != null) worldFactions.Add(faction);
            }
            var randomFactions = (settings.factionsRandomlyAdd ?? new List<string>())
                .Select(name => DefDatabase<FactionDef>.GetNamedSilentFail(name))
                .Where(faction => faction != null).Distinct().ToList();
            int factionCount = settings.randomFactionRange.RandomInRange;
            for (int i = 0; i < factionCount && randomFactions.Count > 0; i++)
            {
                FactionDef faction = randomFactions.RandomElement();
                worldFactions.Add(faction);
                if (settings.uniqueFactions) randomFactions.Remove(faction);
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

            foreach (ScenPart part in Find.Scenario.AllParts)
            {
                FactionDef required = part.def?.preventRemovalOfFaction;
                if (required != null && !worldFactions.Contains(required)) worldFactions.Add(required);
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
                RealisticPlanetsCompat.GenerateRealisticPlanetWorld(
                    settings.planetCoverage,
                    worldSeed,
                    rainfall,
                    temperature,
                    population,
                    landmarkDensity,
                    worldFactions,
                    pollution,
                    oceanType,
                    axialTilt,
                    settings.realisticPlanetsWorldType,
                    settings.randomiseRealisticPlanets
                );
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
                            PreceptDef preceptDef = DefDatabase<PreceptDef>.GetNamed(
                                preceptDefName,
                                false
                            );
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
                if (
                    forFaction
                        .structureMemeWeights.Where(
                            (MemeWeight x) =>
                                IdeoUtility.CanAdd(x.meme, memes, forFaction, parms.forNewFluidIdeo)
                                && (forPlayerFaction || !IdeoUtility.AnyIdeoHas(x.meme))
                        )
                        .TryRandomElementByWeight(
                            (MemeWeight x) =>
                                x.selectionWeight * x.meme.randomizationSelectionWeightFactor,
                            out var result
                        )
                )
                {
                    memes.Add(result.meme);
                    flag = true;
                }
                else if (
                    forFaction
                        .structureMemeWeights.Where(
                            (MemeWeight x) =>
                                IdeoUtility.CanAdd(x.meme, memes, forFaction, parms.forNewFluidIdeo)
                        )
                        .TryRandomElementByWeight(
                            (MemeWeight x) =>
                                x.selectionWeight * x.meme.randomizationSelectionWeightFactor,
                            out result2
                        )
                )
                {
                    memes.Add(result2.meme);
                    flag = true;
                }
            }

            if (!flag)
            {
                MemeDef result4;
                if (
                    DefDatabase<MemeDef>
                        .AllDefs.Where(
                            (MemeDef x) =>
                                x.category == MemeCategory.Structure
                                && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo)
                                && (forPlayerFaction || !IdeoUtility.AnyIdeoHas(x))
                        )
                        .TryRandomElement(out var result3)
                )
                {
                    memes.Add(result3);
                }
                else if (
                    DefDatabase<MemeDef>
                        .AllDefs.Where(
                            (MemeDef x) =>
                                x.category == MemeCategory.Structure
                                && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo)
                        )
                        .TryRandomElementByWeight(
                            (MemeDef x) => x.randomizationSelectionWeightFactor,
                            out result4
                        )
                )
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
                if (
                    DefDatabase<MemeDef>
                        .AllDefs.Where(
                            (MemeDef x) =>
                                x.category == MemeCategory.Normal
                                && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo)
                                && (forPlayerFaction || !IdeoUtility.AnyIdeoHas(x))
                                && (
                                    parms.disallowedMemes == null
                                    || !parms.disallowedMemes.Contains(x)
                                )
                        )
                        .TryRandomElementByWeight(
                            (MemeDef x) => x.randomizationSelectionWeightFactor,
                            out var result5
                        )
                )
                {
                    memes.Add(result5);
                }
                else if (
                    DefDatabase<MemeDef>
                        .AllDefs.Where(
                            (MemeDef x) =>
                                x.category == MemeCategory.Normal
                                && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo)
                                && (
                                    parms.disallowedMemes == null
                                    || !parms.disallowedMemes.Contains(x)
                                )
                        )
                        .TryRandomElementByWeight(
                            (MemeDef x) => x.randomizationSelectionWeightFactor,
                            out result6
                        )
                )
                {
                    memes.Add(result6);
                }
            }
            return memes;
        }

        private static void SetupRandomGenes(RandomStartSettings settings)
        {
            var pawns = Find.GameInitData.startingAndOptionalPawns.Where(pawn => pawn.genes != null).ToList();
            if (settings.enableRandomXenotypes)
            {
                var xenotypes = DefDatabase<XenotypeDef>.AllDefsListForReading.AsEnumerable();
                var factionSet = Faction.OfPlayer.def.basicMemberKind?.xenotypeSet;
                if (settings.respectFactionXenotypes && factionSet != null)
                    xenotypes = xenotypes.Where(xenotype => factionSet.Contains(xenotype));
                var pool = xenotypes.ToList();
                if (pool.Count == 0)
                    Log.Warning("[Random Start] No eligible xenotypes; keeping scenario-generated xenotypes.");
                else
                    foreach (Pawn pawn in pawns) pawn.genes.SetXenotype(pool.RandomElement());
            }
            if (!settings.enableRandomCustomXenotypes) return;
            var eligible = DefDatabase<GeneDef>.AllDefsListForReading.Where(gene => gene.canGenerateInGeneSet).ToList();
            var invalid = new HashSet<GeneDef>();
            foreach (Pawn pawn in pawns)
            {
                var pool = new List<GeneDef>(eligible);
                int count = Math.Min(Math.Max(0, settings.randomGeneRange.RandomInRange), pool.Count);
                bool changed = false;
                for (int i = 0; i < count; i++)
                {
                    GeneDef root = pool.RandomElement();
                    pool.Remove(root);
                    List<GeneDef> ordered;
                    if (!DependencyGraph.TryOrder(root,
                        gene => gene.prerequisite == null ? null : new[] { gene.prerequisite }, out ordered))
                    {
                        if (invalid.Add(root)) Log.Warning($"[Random Start] Skipping cyclic gene prerequisites for {root.defName}.");
                        continue;
                    }
                    bool xenogene = Rand.Bool;
                    changed |= GeneBatch.TryApply(ordered,
                        gene => pawn.genes.GenesListForReading.Any(existing => existing.def == gene),
                        gene => pawn.genes.AddGene(gene, xenogene), gene => pawn.genes.RemoveGene(gene),
                        () => !settings.enableMetabolicEfficiencyMinimum
                            || pawn.genes.GenesListForReading.Where(gene => gene.Active).Sum(gene => gene.def.biostatMet)
                                >= settings.minimumMetabolicEfficiency);
                }
                if (!changed) continue;
                var active = pawn.genes.GenesListForReading.Where(gene => gene.Active).Select(gene => gene.def).Distinct().ToList();
                if (active.Count == 0) continue;
                pawn.genes.xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(active);
                if (DefDatabase<XenotypeIconDef>.AllDefsListForReading.TryRandomElement(out var icon))
                    pawn.genes.iconDef = icon;
            }
        }
    }
}
