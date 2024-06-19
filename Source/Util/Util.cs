using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RandomStartMod
{
    public static class Util
    {
        public static void LogMessage(string message)
        {
            Log.Message($"[{"RandomStartMod.Title".Translate()}] {message}");
        }

        public static string GetIntRangeLabel(IntRange range)
        {
            List<string> intRangeLabels = new List<string>() { "RandomStartMod.VeryLow", "PlanetRainfall_Low", "RandomStartMod.ALittleLess", "PlanetRainfall_Normal", "RandomStartMod.ALittleMore", "PlanetRainfall_High", "RandomStartMod.VeryHigh" };
            string string1 = intRangeLabels[range.min];
            string string2 = intRangeLabels[range.max];
            return string1.Translate() + " - " + string2.Translate();
        }

        public static string GetFloatRangeLabelPercent(FloatRange range)
        {
            return $"{(int)(range.min * 100)}% - {(int)(range.max * 100)}%";
        }
    }
}
