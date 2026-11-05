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

        private void DoFactionSettingsTabContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 60f, inRect.width, factionListingHeight);
            factionListingHeight = 0;
            Widgets.BeginScrollView(inRect, ref factionScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Factions.RequiredTitle".Translate());
            listingStandard.GapLine();
            factionListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Factions.RequiredDescription".Translate());
            factionListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            List<FactionDef> factionsAlwaysAdd = new List<FactionDef>();
            foreach (string factionDefName in settings.factionsAlwaysAdd)
            {
                FactionDef faction = DefDatabase<FactionDef>.GetNamed(factionDefName, false);
                if (faction == null)
                    continue;
                factionsAlwaysAdd.Add(faction);
            }
            for (int i = 0; i < factionsAlwaysAdd.Count; i++)
            {
                listingStandard.Gap(4f);
                if (DoFactionRow(listingStandard.GetRect(24f), factionsAlwaysAdd[i], settings.factionsAlwaysAdd, i))
                {
                    factionsAlwaysAdd.RemoveAt(i);
                    i--;
                }
                listingStandard.Gap(4f);
                factionListingHeight += 32f;
            }

            List<FloatMenuOption> list = new List<FloatMenuOption>();
            IEnumerable<FactionDef> configurableFactions = FactionGenerator.ConfigurableFactions;

            foreach (FactionDef configurableFaction in configurableFactions)
            {
                FactionDef localDef = configurableFaction;
                string text = localDef.LabelCap;
                Action action = delegate
                {
                    settings.factionsAlwaysAdd.Add(localDef.defName);
                };
                AcceptanceReport acceptanceReport = CanAddAlwaysFaction(localDef);
                if (!acceptanceReport)
                {
                    action = null;
                    if (!acceptanceReport.Reason.NullOrEmpty())
                    {
                        text = text + " (" + acceptanceReport.Reason + ")";
                    }
                }
                else
                {
                    int num3 = factionsAlwaysAdd.Count((FactionDef x) => x == localDef);
                    if (num3 > 0)
                    {
                        text = text + " (" + num3 + ")";
                    }
                }
                FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, localDef.FactionIcon, localDef.DefaultColor, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.Description;
                list.Add(floatMenuOption);
            }
            if (list.Count > 0)
            {
                Rect rect5 = listingStandard.GetRect(28f);
                if (Widgets.ButtonText(rect5, "Add".Translate().CapitalizeFirst() + "...") && TutorSystem.AllowAction("ConfiguringWorldFactions"))
                {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                factionListingHeight += 28f;
            }

            factionListingHeight += 28f;

            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Factions.RandomTitle".Translate());
            listingStandard.GapLine();
            factionListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Factions.RandomDescription".Translate());
            factionListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            listingStandard.Gap();
            listingStandard.Indent(8);
            listingStandard.Label("RandomStartMod.Factions.CountLabel".Translate());
            listingStandard.Outdent(8);
            factionListingHeight += Text.LineHeight + 12f;
            Widgets.IntRange(listingStandard.GetRect(32f), 1823998654, ref settings.randomFactionRange, 0, 100);
            factionListingHeight += 32f;
            listingStandard.Gap();
            factionListingHeight += 12f;

            List<FactionDef> factionsRandomlyAdd = new List<FactionDef>();
            foreach (string factionDefName in settings.factionsRandomlyAdd)
            {
                FactionDef faction = DefDatabase<FactionDef>.GetNamed(factionDefName, false);
                if (faction == null)
                    continue;
                factionsRandomlyAdd.Add(faction);
            }
            for (int i = 0; i < factionsRandomlyAdd.Count; i++)
            {
                listingStandard.Gap(4f);
                if (DoFactionRow(listingStandard.GetRect(24f), factionsRandomlyAdd[i], settings.factionsRandomlyAdd, i))
                {
                    factionsRandomlyAdd.RemoveAt(i);
                    i--;
                }
                listingStandard.Gap(4f);
                factionListingHeight += 32f;
            }

            AcceptanceReport CanAddAlwaysFaction(FactionDef f)
            {
                if (factionsAlwaysAdd.Count((FactionDef x) => x == f) >= Math.Abs(f.maxConfigurableAtWorldCreation))
                {
                    return "MaxFactionsForType".Translate(Math.Abs(f.maxConfigurableAtWorldCreation)).ToString().UncapitalizeFirst();
                }
                return true;
            }
            List<FloatMenuOption> list2 = new List<FloatMenuOption>();
            foreach (FactionDef configurableFaction in configurableFactions)
            {
                FactionDef localDef = configurableFaction;
                string text = localDef.LabelCap;
                Action action = delegate
                {
                    settings.factionsRandomlyAdd.Add(localDef.defName);
                };
                AcceptanceReport acceptanceReport = CanAddRandomFaction(localDef);
                if (!acceptanceReport)
                {
                    action = null;
                    if (!acceptanceReport.Reason.NullOrEmpty())
                    {
                        text = text + " (" + acceptanceReport.Reason + ")";
                    }
                }
                else
                {
                    int num3 = factionsRandomlyAdd.Count((FactionDef x) => x == localDef);
                    if (num3 > 0)
                    {
                        text = text + " (" + num3 + ")";
                    }
                }
                FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, localDef.FactionIcon, localDef.DefaultColor, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.Description;
                list2.Add(floatMenuOption);
            }

            if (list2.Count > 0)
            {
                Rect rect6 = listingStandard.GetRect(28f);
                if (Widgets.ButtonText(rect6, "Add".Translate().CapitalizeFirst() + "...") && TutorSystem.AllowAction("ConfiguringWorldFactions"))
                {
                    Find.WindowStack.Add(new FloatMenu(list2));
                }
                factionListingHeight += 28f;
            }

            factionListingHeight += 28f;


            StringBuilder stringBuilder = new StringBuilder();
            if (ModsConfig.RoyaltyActive && !factionsAlwaysAdd.Contains(FactionDefOf.Empire))
            {
                stringBuilder.AppendLine("Warning".Translate() + ": " + "FactionDisabledContentWarning".Translate(FactionDefOf.Empire.label));
            }
            if (!factionsAlwaysAdd.Contains(FactionDefOf.Mechanoid))
            {
                stringBuilder.AppendLine("Warning".Translate() + ": " + "MechanoidsDisabledContentWarning".Translate(FactionDefOf.Mechanoid.label));
            }
            if (!factionsAlwaysAdd.Contains(FactionDefOf.Insect))
            {
                stringBuilder.AppendLine("Warning".Translate() + ": " + "InsectsDisabledContentWarning".Translate(FactionDefOf.Insect.label));
            }
            if (!factionsAlwaysAdd.Contains(FactionDefOf.Ancients))
            {
                stringBuilder.AppendLine("Warning".Translate() + ": " + "FactionDisabledContentWarning".Translate(FactionDefOf.Ancients.label));
            }
            if (!factionsAlwaysAdd.Contains(FactionDefOf.AncientsHostile))
            {
                stringBuilder.AppendLine("Warning".Translate() + ": " + "InsectsDisabledContentWarning".Translate(FactionDefOf.AncientsHostile.label));
            }
            if (ModsConfig.AnomalyActive && !factionsAlwaysAdd.Contains(FactionDefOf.HoraxCult))
            {
                stringBuilder.AppendLine("Warning".Translate() + ": " + "FactionDisabledContentWarning".Translate(FactionDefOf.HoraxCult.label));
            }
            if (ModsConfig.AnomalyActive && !factionsAlwaysAdd.Contains(FactionDefOf.Entities))
            {
                stringBuilder.AppendLine("Warning".Translate() + ": " + "FactionDisabledContentWarning".Translate(FactionDefOf.Entities.label));
            }
            if (stringBuilder.Length > 0)
            {
                bool wordWrap = Text.WordWrap;
                string text2 = stringBuilder.ToString().TrimEndNewlines();
                GUI.color = Color.yellow;
                Text.Font = GameFont.Tiny;
                Text.WordWrap = true;
                Rect labelRect = listingStandard.Label(text2);
                factionListingHeight += labelRect.height;
                Text.WordWrap = wordWrap;
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
            }

            listingStandard.Gap();
            factionListingHeight += 12f;
            DoSettingToggle(listingStandard.GetRect(24f), "Unique".Translate().CapitalizeFirst(), "RandomStartMod.TooltipTitles.UniqueFactions".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Factions.UniqueFactionsTooltip".Translate(), ref settings.uniqueFactions);
            factionListingHeight += 24f;
            DoSettingToggle(listingStandard.GetRect(24f), "Randomize".Translate() + ": " + "Goodwill".Translate(), "RandomStartMod.TooltipTitles.RandomizeFactionGoodwill".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Factions.RandomizeFactionGoodwillTooltip".Translate(), ref settings.randomiseFactionGoodwill);
            factionListingHeight += 24f;

            // Reputation exclusion section
            if (settings.randomiseFactionGoodwill)
            {
                listingStandard.Gap();
                factionListingHeight += 12f;
                Text.Font = GameFont.Medium;
                Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Factions.ReputationExclusionsTitle".Translate());
                factionListingHeight += Text.LineHeight;
                Text.Font = GameFont.Small;
                Text.Font = GameFont.Tiny;
                Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Factions.ReputationExclusionsDescription".Translate());
                factionListingHeight += Text.LineHeight;
                Text.Font = GameFont.Small;

                // Show excluded factions as rows
                List<FactionDef> factionsExcludeFromReputation = new List<FactionDef>();
                if (settings.factionsExcludeFromReputationRandomization != null)
                {
                    foreach (string factionDefName in settings.factionsExcludeFromReputationRandomization)
                    {
                        FactionDef faction = DefDatabase<FactionDef>.GetNamed(factionDefName, false);
                        if (faction != null)
                            factionsExcludeFromReputation.Add(faction);
                    }
                }

                for (int i = 0; i < factionsExcludeFromReputation.Count; i++)
                {
                    listingStandard.Gap(4f);
                    if (settings.factionsExcludeFromReputationRandomization != null && DoFactionRow(listingStandard.GetRect(24f), factionsExcludeFromReputation[i], settings.factionsExcludeFromReputationRandomization, i))
                    {
                        break;
                    }
                    listingStandard.Gap(4f);
                    factionListingHeight += 32f;
                }

                // Add button for selecting factions to exclude
                if (listingStandard.ButtonText("Add".Translate().CapitalizeFirst() + "..."))
                {
                    // Initialize the list if it's null
                    if (settings.factionsExcludeFromReputationRandomization == null)
                    {
                        settings.factionsExcludeFromReputationRandomization = new List<string>();
                    }

                    List<FloatMenuOption> excludeList = new List<FloatMenuOption>();
                    foreach (FactionDef localDef in FactionGenerator.ConfigurableFactions.Where(def => !settings.factionsExcludeFromReputationRandomization.Contains(def.defName)))
                    {
                        FactionDef localDef2 = localDef;
                        string text = localDef.LabelCap.ToString();
                        FloatMenuOption floatMenuOption = new FloatMenuOption(text, delegate
                        {
                            settings.factionsExcludeFromReputationRandomization.Add(localDef.defName);
                        }, localDef.FactionIcon, localDef.DefaultColor, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                        floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.Description;
                        excludeList.Add(floatMenuOption);
                    }
                    if (excludeList.Count == 0)
                    {
                        excludeList.Add(new FloatMenuOption("No factions available to exclude", null));
                    }
                    Find.WindowStack.Add(new FloatMenu(excludeList));
                }
                factionListingHeight += 28f;
            }

            listingStandard.Gap();
            factionListingHeight += 12f;
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetFactions();
            }
            factionListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();

            AcceptanceReport CanAddRandomFaction(FactionDef f)
            {
                if (factionsAlwaysAdd.Count((FactionDef x) => x == f) >= Math.Abs(f.maxConfigurableAtWorldCreation))
                {
                    return "MaxFactionsForType".Translate(Math.Abs(f.maxConfigurableAtWorldCreation)).ToString().UncapitalizeFirst();
                }
                if (factionsRandomlyAdd.Count((FactionDef x) => x == f) > 0)
                    return "MaxFactionsForType".Translate(1).ToString().UncapitalizeFirst();
                return true;
            }
        }

        public bool DoFactionRow(Rect rect, FactionDef factionDef, List<string> factionDefNames, int index)
        {
            bool result = false;
            Rect rect2 = new Rect(rect.x, rect.y - 4f, rect.width, rect.height + 8f);
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect2);
            }
            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);
            GUI.color = factionDef.DefaultColor;
            widgetRow.Icon(factionDef.FactionIcon);
            GUI.color = Color.white;
            widgetRow.Gap(4f);
            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(factionDef.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                factionDefNames.Remove(factionDef.defName);
                result = true;
            }
            Widgets.EndGroup();
            ModContentPack mod = factionDef.modContentPack;
            if (Mouse.IsOver(rect2))
            {
                TooltipHandler.TipRegion(rect2, factionDef.LabelCap.AsTipTitle() + "\n" + factionDef.Description + "\n" + "\n" + GetSourceModName(factionDef).AsTipTitle());
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

    }
}
