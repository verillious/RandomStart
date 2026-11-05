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

        private void DoStorytellerSettingsTabContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 60f, inRect.width, storytellerListingHeight);
            storytellerListingHeight = 0f;
            Widgets.BeginScrollView(inRect, ref mainScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            // Storytellers
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Storyteller.Title".Translate());
            listingStandard.GapLine();
            storytellerListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Storyteller.Description".Translate());
            storytellerListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            List<StorytellerDef> enabledStorytellers = DefDatabase<StorytellerDef>.AllDefsListForReading.Where((StorytellerDef item) => item.listVisible && !settings.disabledStorytellers.Contains(item.defName)).ToList();

            for (int i = 0; i < enabledStorytellers.Count; i++)
            {
                listingStandard.Gap(4f);
                if (DoStorytellerRow(listingStandard.GetRect(24f), enabledStorytellers[i], i, enabledStorytellers.Count))
                {
                    i--;
                }
                listingStandard.Gap(4f);
                storytellerListingHeight += 32f;
            }
            var floatMenuOptions = new List<FloatMenuOption>();
            if (settings.disabledStorytellers.NullOrEmpty())
                settings.disabledStorytellers = new List<string>();

            foreach (StorytellerDef item in DefDatabase<StorytellerDef>.AllDefs.OrderBy((StorytellerDef tel) => tel.listOrder))
            {
                if (item.listVisible && settings.disabledStorytellers.Contains(item.defName))
                {
                    floatMenuOptions.Add(new FloatMenuOption(item.LabelCap, () => settings.disabledStorytellers.Remove(item.defName), GetSourceIcon(item), Color.white, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, item), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true));
                }
            }


            if (floatMenuOptions.Count > 0)
            {
                if (Widgets.ButtonText(listingStandard.GetRect(28f), "Add".Translate().CapitalizeFirst() + "..."))
                {

                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                }
                storytellerListingHeight += 32f;
            }

            listingStandard.Gap();
            storytellerListingHeight += 12f;
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetStorytellers();
            }
            storytellerListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();
            base.DoSettingsWindowContents(inRect);
        }

        public bool DoStorytellerRow(Rect rect, StorytellerDef storytellerDef, int index, int storyTellerCount)
        {
            bool result = false;
            Rect rect2 = new Rect(rect.x, rect.y - 4f, rect.width, rect.height + 8f);
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect2);
            }
            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);
            GUI.color = Color.white;
            ModContentPack mod = storytellerDef.modContentPack;
            widgetRow.Icon(GetSourceIcon(storytellerDef));
            GUI.color = Color.white;
            widgetRow.Gap(4f);
            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(storytellerDef.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                if (storyTellerCount > 1)
                {
                    settings.disabledStorytellers.Add(storytellerDef.defName);
                    result = true;
                }
            }
            Widgets.EndGroup();
            if (Mouse.IsOver(rect2))
            {
                TooltipHandler.TipRegion(rect2, storytellerDef.LabelCap.AsTipTitle() + "\n" + storytellerDef.description + "\n" + "\n" + GetSourceModMetaData(storytellerDef).Name.AsTipTitle());
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

    }
}
