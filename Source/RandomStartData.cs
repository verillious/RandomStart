using System.Collections.Generic;
using Verse;

namespace RandomStartMod
{
    internal sealed class RandomStartState
    {
        internal readonly RandomStartSettings Settings;
        internal bool ResearchApplied;
        internal bool ItemsPrepared;
        internal readonly HashSet<ResearchProjectDef> ProtectedIdeologyResearch = new HashSet<ResearchProjectDef>();
        internal readonly ScopedValue<bool> PreparingPawns = new ScopedValue<bool>();
        internal readonly ScopedValue<bool> GeneratingPawn = new ScopedValue<bool>();
        internal readonly ScopedValue<bool> GrantingResearch = new ScopedValue<bool>();
        internal readonly ScopedValue<HashSet<ResearchProjectDef>> BlockedPrerequisites = new ScopedValue<HashSet<ResearchProjectDef>>();
        internal RandomStartState(RandomStartSettings settings) { Settings = settings.SnapshotForStart(); }
    }

    internal static class RandomStartData
    {
        private static readonly StartSession<Game, RandomStartState> Sessions = new StartSession<Game, RandomStartState>();
        internal static RandomStartState State => Sessions.For(Current.Game);
        internal static void Begin(RandomStartSettings settings) => Sessions.Begin(Current.Game, new RandomStartState(settings));
        internal static void End(Game game) => Sessions.End(game);
    }
}
