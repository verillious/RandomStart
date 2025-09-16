using Verse;

namespace RandomStartMod
{
    [StaticConstructorOnStartup]
    public static class RandomStartData
    {
        // Toggles randomisation of
        // - Starting items (in ScenPart_PlayerPawnsArriveMethod_Patch)
        // - Starting research (in ResearchUtility_Patch)
        public static bool startedFromRandom = false;
    }
}
