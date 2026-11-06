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

        private void DoScenarioSettingsTabContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 60f, inRect.width, scenarioListingHeight);
            scenarioListingHeight = 0f;
            Widgets.BeginScrollView(inRect, ref scenariosScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            // Scenarios
            DoSettingToggle(listingStandard.GetRect(24f), "Randomize".Translate(), "RandomStartMod.TooltipTitles.RandomizeScenario".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Scenarios.RandomizeScenarioTooltip".Translate(), ref settings.createRandomScenario);
            scenarioListingHeight += 24f;
            if (!settings.createRandomScenario)
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("ScenPart_StartWithPawns_OutOf".Translate().CapitalizeFirst());
                listingStandard.GapLine();
                scenarioListingHeight += 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                Text.Font = GameFont.Tiny;
                Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Scenarios.Description".Translate());
                scenarioListingHeight += Text.LineHeight;
                Text.Font = GameFont.Small;

                List<ScenarioDef> enabledScenarioDefs = DefDatabase<ScenarioDef>.AllDefsListForReading.Where((ScenarioDef item) => !settings.disabledScenarios.Contains(item.scenario.name) && item.scenario.showInUI).ToList();
                List<Scenario> enabledCustomScenarios = ScenarioLister.ScenariosInCategory(ScenarioCategory.CustomLocal).Where((Scenario item) => !settings.disabledScenarios.Contains(item.name)).ToList();
                List<Scenario> enabledWorkshopScenarios = ScenarioLister.ScenariosInCategory(ScenarioCategory.SteamWorkshop).Where((Scenario item) => !settings.disabledScenarios.Contains(item.name)).ToList();

                int index = 0;
                int totalCount = enabledScenarioDefs.Count + enabledCustomScenarios.Count + enabledWorkshopScenarios.Count;

                var floatMenuOptions = new List<FloatMenuOption>();
                if (settings.disabledScenarios.NullOrEmpty())
                    settings.disabledScenarios = new List<string>();

                foreach (ScenarioDef scenarioDef in DefDatabase<ScenarioDef>.AllDefsListForReading)
                {
                    if (!scenarioDef.scenario.showInUI)
                        continue;

                    Texture2D tex = GetSourceIcon(scenarioDef);
                    string source = GetSourceModName(scenarioDef);
                    if (!settings.disabledScenarios.Contains(scenarioDef.scenario.name))
                    {
                        listingStandard.Gap(4f);
                        DoScenarioRow(listingStandard.GetRect(24f), scenarioDef.scenario, index, totalCount, tex, source);
                        listingStandard.Gap(4f);
                        scenarioListingHeight += 32f;
                        index++;
                    }
                    else
                    {
                        floatMenuOptions.Add(new FloatMenuOption(scenarioDef.LabelCap, () => settings.disabledScenarios.Remove(scenarioDef.scenario.name), tex, Color.white, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, scenarioDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true));
                    }
                }

                foreach (Scenario scenario in ScenarioLister.ScenariosInCategory(ScenarioCategory.CustomLocal))
                {
                    if (!scenario.showInUI)
                        continue;

                    Texture2D tex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Save");
                    string source = "Custom".Translate().CapitalizeFirst();
                    if (!settings.disabledScenarios.Contains(scenario.name))
                    {
                        listingStandard.Gap(4f);
                        DoScenarioRow(listingStandard.GetRect(24f), scenario, index, totalCount, tex, source);
                        listingStandard.Gap(4f);
                        scenarioListingHeight += 32f;
                        index++;
                    }
                    else
                    {
                        floatMenuOptions.Add(new FloatMenuOption(scenario.name, () => settings.disabledScenarios.Remove(scenario.name), tex, Color.white, MenuOptionPriority.Default, null, null, 24f, null, null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true));
                    }
                }

                foreach (Scenario scenario in ScenarioLister.ScenariosInCategory(ScenarioCategory.SteamWorkshop))
                {
                    if (!scenario.showInUI)
                        continue;

                    Texture2D tex = ContentFinder<Texture2D>.Get("UI/Icons/ContentSources/SteamWorkshop");
                    string source = "Workshop".Translate();
                    if (!settings.disabledScenarios.Contains(scenario.name))
                    {
                        listingStandard.Gap(4f);
                        DoScenarioRow(listingStandard.GetRect(24f), scenario, index, totalCount, tex, source);
                        listingStandard.Gap(4f);
                        scenarioListingHeight += 32f;
                        index++;
                    }
                    else
                    {
                        floatMenuOptions.Add(new FloatMenuOption(scenario.name, () => settings.disabledScenarios.Remove(scenario.name), tex, Color.white, MenuOptionPriority.Default, null, null, 24f, null, null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true));
                    }
                }

                if (floatMenuOptions.Count > 0)
                {
                    if (Widgets.ButtonText(listingStandard.GetRect(28f), "Add".Translate().CapitalizeFirst() + "..."))
                    {
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    }
                    scenarioListingHeight += 32f;
                }
            }

            listingStandard.Gap();
            scenarioListingHeight += 12f;
            Text.Font = GameFont.Medium;
            listingStandard.Label("Research".Translate());
            listingStandard.GapLine();
            scenarioListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Scenarios.ResearchDescription".Translate());
            scenarioListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            DoSettingToggle(listingStandard.GetRect(24f), $"{"Remove".Translate()}: {"MedGroupDefaults".Translate()}", "RandomStartMod.TooltipTitles.RemoveDefaultResearch".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Scenarios.RemoveDefaultResearchTooltip".Translate(), ref settings.removeStartingResearch);
            scenarioListingHeight += 24f;
            DoSettingToggle(listingStandard.GetRect(24f), "Randomize".Translate(), "RandomStartMod.TooltipTitles.RandomizeResearch".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Scenarios.RandomizeResearchTooltip".Translate(), ref settings.addRandomResearch);
            scenarioListingHeight += 24f;
            if (settings.addRandomResearch)
            {
                listingStandard.Gap();
                listingStandard.Indent(8);
                listingStandard.Label("PenFoodTab_Count".Translate() + ":");
                listingStandard.Outdent(8);
                scenarioListingHeight += Text.LineHeight + 12f;
                Widgets.IntRange(listingStandard.GetRect(32f), 1823998654, ref settings.randomResearchRange, 0, 20);
                scenarioListingHeight += 32f;

                listingStandard.Gap();
                listingStandard.Indent();
                listingStandard.Label("MaxTier".Translate() + ":");
                listingStandard.Outdent();
                scenarioListingHeight += Text.LineHeight + 12f;
                settings.randomResearchTechLevelLimit = Mathf.RoundToInt(Widgets.HorizontalSlider(listingStandard.GetRect(32f), settings.randomResearchTechLevelLimit, 2, 6, middleAlignment: true, $"TechLevel_{(TechLevel)settings.randomResearchTechLevelLimit}".Translate().CapitalizeFirst(), null, null, 1f));
                scenarioListingHeight += 32f;
                DoSettingToggle(listingStandard.GetRect(24f), $"{"ResearchUnlocks".Translate()}: {"Prerequisites".Translate()}", "RandomStartMod.TooltipTitles.UnlockPrerequisiteResearch".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Scenarios.UnlockPrerequisiteResearchTooltip".Translate(), ref settings.doRandomResearchPrerequisites);
                scenarioListingHeight += 24f;
            }


            listingStandard.Gap();
            scenarioListingHeight += 12f;
            Text.Font = GameFont.Medium;
            listingStandard.Label("ItemsTab".Translate());
            listingStandard.GapLine();
            scenarioListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Scenarios.ItemsDescription".Translate());
            scenarioListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            DoSettingToggle(listingStandard.GetRect(24f), $"{"Remove".Translate()}: {"MedGroupDefaults".Translate()}", "RandomStartMod.TooltipTitles.RemoveDefaultItems".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Scenarios.RemoveDefaultItemsTooltip".Translate(), ref settings.removeStartingItems);
            scenarioListingHeight += 24f;
            DoSettingToggle(listingStandard.GetRect(24f), "Randomize".Translate(), "RandomStartMod.TooltipTitles.RandomizeItems".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Scenarios.RandomizeItemsTooltip".Translate(), ref settings.addRandomItems);
            scenarioListingHeight += 24f;


            if (settings.addRandomItems)
            {
                listingStandard.Gap();
                listingStandard.Indent(8);
                listingStandard.Label("PenFoodTab_Count".Translate() + ":");
                listingStandard.Outdent(8);
                scenarioListingHeight += Text.LineHeight + 12f;
                Widgets.IntRange(listingStandard.GetRect(32f), 1823288654, ref settings.randomItemRange, 0, 20);
                scenarioListingHeight += 32f;
                listingStandard.Label("MaxTier".Translate() + ":");
                scenarioListingHeight += Text.LineHeight;
                settings.randomItemTechLevelLimit = Mathf.RoundToInt(Widgets.HorizontalSlider(listingStandard.GetRect(32f), settings.randomItemTechLevelLimit, 2, 6, middleAlignment: true, $"TechLevel_{(TechLevel)settings.randomItemTechLevelLimit}".Translate().CapitalizeFirst(), null, null, 1f));
                scenarioListingHeight += 32f;

                DoSettingToggle(listingStandard.GetRect(24f), $"{"StatsReport_MaxValue".Translate()} ({"Total".Translate().CapitalizeFirst()})", "RandomStartMod.TooltipTitles.MarketValueLimit".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Scenarios.MarketValueLimitTooltip".Translate(), ref settings.enableMarketValueLimit);
                scenarioListingHeight += 24f;
                if (settings.enableMarketValueLimit)
                {
                    listingStandard.Gap(2f);
                    Widgets.TextFieldNumericLabeled(listingStandard.GetRect(24f), $"{"Value".Translate()} ", ref settings.randomItemTotalMarketValueLimit, ref randomItemTotalMarketValueLimitTextBuffer);
                    scenarioListingHeight += 2f + 24f;
                }
            }

            listingStandard.Gap();
            scenarioListingHeight += 12f;
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetScenarios();
            }
            scenarioListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();
        }

        public bool DoScenarioRow(Rect rect, Scenario scenario, int index, int scenarioCount, Texture2D icon, string source)
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

            widgetRow.Icon(icon);

            GUI.color = Color.white;
            widgetRow.Gap(4f);
            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(scenario.name);
            Text.Anchor = TextAnchor.UpperLeft;
            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                if (scenarioCount > 1)
                {
                    settings.disabledScenarios.Add(scenario.name);
                    result = true;
                }
            }
            Widgets.EndGroup();
            if (Mouse.IsOver(rect2))
            {
                TooltipHandler.TipRegion(rect2, scenario.name.AsTipTitle() + "\n" + scenario.description + "\n" + "\n" + source.AsTipTitle());
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

    }
}
