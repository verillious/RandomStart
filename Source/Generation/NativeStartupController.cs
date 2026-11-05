using System;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace RandomStartMod
{
    // Development entry only until real-game lifecycle traces validate this path.
    // The normal Random button continues to use the existing startup path.
    internal sealed class NativeStartupController
    {
        private sealed class PendingStart
        {
            internal readonly Game PreviousGame = Current.Game;
            internal readonly RandomStartSettings Settings;
            internal bool Attached;
            internal PendingStart(RandomStartSettings settings) { Settings = settings; }
        }

        private static readonly ScopedValue<PendingStart> Pending = new ScopedValue<PendingStart>();
        private readonly RandomStartSettings settings;
        private readonly StartupSequence<Page> sequence = new StartupSequence<Page>();
        private Page currentPage;
        private int openedFrame;
        private bool goodwillApplied;

        private NativeStartupController(RandomStartSettings settings) { this.settings = settings; }

        [DebugAction("Random Start", "Prototype native startup", allowedGameStates = AllowedGameStates.Entry)]
        private static void StartFromDebugMenu() => LongEventHandler.QueueLongEvent(Start,
            "GeneratingMap", false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);

        private static void Start()
        {
            var settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>().SnapshotForStart();
            var request = new PendingStart(settings);
            var scenario = RandomScenario.ChooseRandomScenario(settings);
            Log.Message("[Random Start native prototype] Using native setup. Ideology and unfamiliar pages require manual completion; "
                + "mod-specific Random Start integrations are not applied. The normal Random button remains available.");
            using (Pending.Enter(request))
            {
                try
                {
                    // Call the live method, including other mods' Harmony patches.
                    Page_SelectScenario.BeginScenarioConfiguration(scenario, null);
                    if (!request.Attached)
                        throw new InvalidOperationException("Native setup did not initialize a new game's scenario through PreConfigure.");
                }
                catch
                {
                    if (request.Attached) RandomStartData.End(Current.Game);
                    throw;
                }
            }
        }

        internal static bool Attach(Scenario scenario)
        {
            PendingStart request = Pending.Value;
            if (request == null || request.Attached || Current.Game == null
                || ReferenceEquals(Current.Game, request.PreviousGame) || !ReferenceEquals(scenario, Find.Scenario)) return false;
            RandomStartData.Begin(request.Settings);
            request.Attached = true;
            var state = RandomStartData.State;
            state.NativeStartup = new NativeStartupController(state.Settings);
            StartingItems.Prepare(scenario, state.Settings);
            return true;
        }

        internal void Opened(Page page)
        {
            if (page == null || !page.IsOpen) return;
            currentPage = page;
            openedFrame = Time.frameCount;
            Log.Message("[Random Start native prototype] Opened " + page.GetType().FullName);
            sequence.ConfigureOnce(page, () =>
            {
                // Exact supported types only. A subclass can implement a different workflow.
                Type type = page.GetType();
                if (type == typeof(Page_SelectStoryteller))
                {
                    RandomScenario.SetupRandomDifficulty(settings, false);
                    var storytellerPage = (Page_SelectStoryteller)page;
                    storytellerPage.storyteller = Current.Game.storyteller.def;
                    storytellerPage.difficulty = Current.Game.storyteller.difficultyDef;
                    storytellerPage.difficultyValues = Current.Game.storyteller.difficulty;
                    Find.GameInitData.permadeathChosen = true;
                }
                else if (type == typeof(Page_CreateWorldParams))
                {
                    WorldSetupPlan plan = RandomScenario.ChooseWorldParameters(settings, false);
                    var worldPage = (Page_CreateWorldParams)page;
                    worldPage.planetCoverage = plan.Coverage;
                    worldPage.seedString = plan.Seed;
                    worldPage.rainfall = plan.Rainfall;
                    worldPage.temperature = plan.Temperature;
                    worldPage.population = plan.Population;
                    worldPage.landmarkDensity = plan.LandmarkDensity;
                    worldPage.pollution = plan.Pollution;
                    worldPage.factions = plan.Factions;
                    // The native page, including its validation, owns world generation.
                }
                else if (type == typeof(Page_SelectStartingSite))
                {
                    RandomScenario.SetupRandomStartingSite(settings);
                    Find.WorldSelector.SelectedTile = Find.GameInitData.startingTile;
                }
                else if (type == typeof(Page_ConfigureStartingPawns))
                {
                    if (!goodwillApplied)
                    {
                        goodwillApplied = true;
                        RandomScenario.SetupRandomGoodwill(settings);
                    }
                }
                else
                {
                    bool alreadyStopped = sequence.Stopped;
                    sequence.Stop();
                    Log.Message("[Random Start native prototype] Automatic advancement stopped at " + type.FullName
                        + ". Complete the remaining pages normally. Ideology settings are not automated by this prototype.");
                    if (!alreadyStopped)
                        Find.WindowStack.Add(new Dialog_MessageBox("RandomStartMod.NativePrototypeManual".Translate()));
                }
            });
        }

        internal void AfterWindowsDrawn()
        {
            if (sequence.Stopped || Current.ProgramState != ProgramState.Entry || currentPage == null || !currentPage.IsOpen
                || Time.frameCount <= openedFrame || Event.current?.type != EventType.Repaint) return;
            bool blocked = LongEventHandler.AnyEventNowOrWaiting
                || !ReferenceEquals(Find.WindowStack.Windows.LastOrDefault(), currentPage);
            Page page = currentPage;
            sequence.TryAdvance(page, blocked, () =>
            {
                Log.Message("[Random Start native prototype] Requesting native acceptance of " + page.GetType().FullName);
                // Use the real page's acceptance handler, not a copied DoNext body,
                // a reverse patch, a fabricated click, or a direct InitGameStart call.
                page.OnAcceptKeyPressed();
            });
        }

        internal static void Fail(Exception error)
        {
            RandomStartData.End(Current.Game);
            Log.Error("[Random Start native prototype] Stopped; cancel this setup and return to the menu. " + error);
            Find.WindowStack.Add(new Dialog_MessageBox("RandomStartMod.NativePrototypeFailed".Translate()));
        }
    }
}
