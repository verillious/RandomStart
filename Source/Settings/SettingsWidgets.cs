using RandomStartMod.Compat;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RandomStartMod
{
    public partial class RandomStartMod
    {

        public void DoSettingToggle(Rect rect, string label, string description, ref bool checkOn)
        {

            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);
            widgetRow.Gap(4f);
            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(label);
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.Checkbox(new Vector2(rect.width - 24f - 6f, 0f), ref checkOn);
            Widgets.EndGroup();
            if (Widgets.ButtonInvisible(new Rect(rect.x, rect.y, rect.width - 24f - 6f, rect.height)))
            {
                checkOn = !checkOn;
                if (checkOn)
                {
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                }
                else
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                }
            }
            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, description);
                Widgets.DrawHighlight(rect);
            }

        }

        public ModMetaData GetSourceModMetaData(Def def)
        {
            return def?.modContentPack?.ModMetaData;
        }

        private string GetSourceModName(Def def) => GetSourceModMetaData(def)?.Name ?? "Unknown".Translate().ToString();

        public Texture2D GetSourceIcon(Def def)
        {
            var content = def?.modContentPack;
            var metadata = content?.ModMetaData;
            return (content?.IsOfficialMod == true ? metadata?.Expansion?.Icon : metadata?.Icon)
                ?? ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Save");
        }

        private static float Reciprocal(float f, float cutOff)
        {
            cutOff *= 10f;
            if (Mathf.Abs(f) < 0.01f)
            {
                return cutOff;
            }
            if (f >= 0.99f * cutOff)
            {
                return 0f;
            }
            return 1f / f;
        }

    }
}
