using RimWorld;
using System.Linq;
using Verse;

namespace RandomStartMod
{
    public static class Util
    {
        public static void LogMessage(string message)
        {
            Log.Message($"[{"RandomStartMod.Title".Translate()}] {message}");
        }

        public static bool IsScenarioFromMod(string modName)
        {
            ScenarioDef scenarioDef = DefDatabase<ScenarioDef>.AllDefsListForReading.First((ScenarioDef s) => s.scenario == Find.Scenario);
            return scenarioDef.modContentPack.Name == modName;
        }
    }
}
