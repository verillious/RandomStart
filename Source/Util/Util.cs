using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RandomStartMod
{
    public static class Util
    {
        public static void LogMessage(string message)
        {
            Log.Message($"<color=magenta>[{"RandomStartMod.Title".Translate()}]</color> {message}");
        }

        public static TaggedString GetIntRangeLabel(IntRange range)
        {
            List<string> intRangeLabels = new List<string>() { "RandomStartMod.VeryLow", "PlanetRainfall_Low", "RandomStartMod.ALittleLess", "PlanetRainfall_Normal", "RandomStartMod.ALittleMore", "PlanetRainfall_High", "RandomStartMod.VeryHigh" };
            string string1 = intRangeLabels[range.min];
            string string2 = intRangeLabels[range.max];
            return string1.Translate() + " - " + string2.Translate();
        }

        public static string GetIntLabel(int input)
        {
            List<string> intRangeLabels = new List<string>() { "RandomStartMod.VeryLow", "PlanetRainfall_Low", "RandomStartMod.ALittleLess", "PlanetRainfall_Normal", "RandomStartMod.ALittleMore", "PlanetRainfall_High", "RandomStartMod.VeryHigh" };
            string string1 = intRangeLabels[input];
            return string1.Translate();
        }

        public static string GetFloatRangeLabelPercent(FloatRange range)
        {
            return $"{(int)(range.min * 100)}% - {(int)(range.max * 100)}%";
        }

        public static void DrawCountAdjuster(ref int value, Rect inRect, ref string buffer, int min, int max, bool readOnly = false, int? setToMin = null, int? setToMax = null)
        {
            int val = value;
            Rect rect = inRect.ContractedBy(50f, 0f);
            Rect rect2 = rect.LeftPartPixels(30f);
            rect.xMin = rect.xMin + 30f;
            Rect rect3 = rect.LeftPartPixels(30f);
            rect.xMin = rect.xMin + 30f;
            Rect rect4 = rect.RightPartPixels(30f);
            rect.xMax = rect.xMax - 30f;
            Rect rect5 = rect.RightPartPixels(30f);
            rect.xMax = rect.xMax - 30f;
            int num = GenUI.CurrentAdjustmentMultiplier();
            if (!readOnly && (setToMin.HasValue ? (value > setToMin.Value) : (value != min)) && Widgets.ButtonText(rect2, "<<"))
            {
                value = setToMin ?? min;
            }
            if (!readOnly && value - num >= min && Widgets.ButtonText(rect3, "<"))
            {
                value -= num;
            }
            if (!readOnly && (setToMax.HasValue ? (value < setToMax.Value) : (value != max)) && Widgets.ButtonText(rect4, ">>"))
            {
                value = setToMax ?? max;
            }
            if (!readOnly && value + num <= max && Widgets.ButtonText(rect5, ">"))
            {
                value += num;
            }
            if (value < min)
            {
                value = min;
            }
            if (value > max)
            {
                value = max;
            }
            if (value != val || readOnly)
            {
                buffer = value.ToString();
            }
            Widgets.TextFieldNumeric(rect.ContractedBy(3f, 0f), ref val, ref buffer, min, max);
            if (!readOnly)
            {
                value = val;
            }
        }
    }
}
