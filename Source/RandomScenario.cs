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
    public static partial class RandomScenario
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
            Current.Game.Scenario = ChooseRandomScenario(settings);
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

            SetupRandomStartingSite(settings);

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

            SetupRandomGoodwill(settings);
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

        internal static void SetupRandomStartingSite(RandomStartSettings settings)
        {
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

        }

        internal static void SetupRandomGoodwill(RandomStartSettings settings)
        {
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
        }

        internal static void FinishStartingPawnSetup(RandomStartSettings settings)
        {
            bool usePrepareModerately = RandomStartData.State?.NativeStartup == null
                && settings.enablePrepareModeratelyIntegration && PrepareModeratelyCompat.IsAvailable;
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

        internal static Scenario ChooseRandomScenario(RandomStartSettings settings)
        {
            Scenario chosen;

            if (settings.createRandomScenario)
            {
                chosen = ScenarioMaker.GenerateNewRandomScenario(
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

                chosen = randomScenario.CopyForEditing();
            }
            Util.LogMessage($"Starting {chosen}");
            return chosen;
        }
    }
}
