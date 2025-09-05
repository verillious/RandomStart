using Mono.Cecil;
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
    public class RandomStartMod : Mod
    {
        RandomStartSettings settings;

        private static readonly int[] MapSizes = new int[6] { 200, 225, 250, 275, 300, 325 };
        private static readonly int[] Seasons = new int[6] { 1, 2, 3, 4, 5, 6 };
        private static readonly float[] PlanetCoverages = new float[3] { 0.3f, 0.5f, 1f };

        private static Vector2 mainScrollPosition;
        private static Vector2 factionScrollPosition;
        private static Vector2 planetScrollPosition;
        private static Vector2 scenariosScrollPosition;
        private static Vector2 startingTileScrollPosition;

        private static float mainListingHeight;
        private static float factionListingHeight;
        private static float planetListingHeight;
        private static float storytellerListingHeight;
        private static float scenarioListingHeight;
        private static float optionalFeaturesListingHeight;
        private static float startingTileListingHeight;
        private static float sectionHeightThreats = 0f;
        private static float sectionHeightGeneral = 0f;
        private static float sectionHeightPlayerTools = 0f;
        private static float sectionHeightEconomy = 0f;
        private static float sectionHeightAdaptation = 0f;
        private static float sectionHeightIdeology = 0f;
        private static float sectionHeightChildren = 0f;

        public int currentTab = 0;
        public string randomItemTotalMarketValueLimitTextBuffer;
        public RandomStartMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RandomStartSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.openedSettings = true;

            Rect tabRect = new Rect(inRect)
            {
                y = inRect.y + 40f
            };
            Rect mainRect = new Rect(inRect)
            {
                height = inRect.height - 40f,
                y = inRect.y + 40f
            };

            Widgets.DrawMenuSection(mainRect);
            List<TabRecord> tabs = new List<TabRecord>
            {
                new TabRecord("DifficultyGeneralSection".Translate(), () =>
                {
                    currentTab = 0;
                    WriteSettings();
                }, currentTab == 0),
                new TabRecord("Storyteller".Translate(), () =>
                {
                    currentTab = 3;
                    WriteSettings();
                }, currentTab == 3),
                new TabRecord("StatsReport_ScenarioFactor".Translate(), () =>
                {
                    currentTab = 4;
                    WriteSettings();
                }, currentTab == 4),
                new TabRecord("TabPlanet".Translate(), () =>
                {
                    currentTab = 2;
                    WriteSettings();
                }, currentTab == 2),
                new TabRecord("RandomStartMod.Tabs.StartingTile".Translate(), () =>
                {
                    currentTab = 6;
                    WriteSettings();
                }, currentTab == 6),
                new TabRecord("Factions".Translate(), () =>
                {
                    currentTab = 1;
                    WriteSettings();
                }, currentTab == 1),
                new TabRecord("RandomStartMod.Characters".Translate(),  () =>
                {
                    currentTab = 7;
                    WriteSettings();
                }, currentTab == 7)
        };
            if (ModsConfig.BiotechActive || ModsConfig.IdeologyActive)
            {
                tabs.Add(
                    new TabRecord("MiscRecordsCategory".Translate(), () =>
                    {
                        currentTab = 5;
                        WriteSettings();
                    }, currentTab == 5)
                );
            }
            TabDrawer.DrawTabs(tabRect, tabs);

            if (currentTab == 0)
            {
                DoDifficultySettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 1)
            {
                DoFactionSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 2)
            {
                DoPlanetSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 3)
            {
                DoStorytellerSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 4)
            {
                DoScenarioSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 5)
            {
                DoOptionalFeaturesTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 6)
            {
                DoStartingTileSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 7)
            {
                DoCharactersSettingsTabContents(mainRect.ContractedBy(15f));
            }
        }

        public void DoDifficultySettingsTabContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 60f, inRect.width, mainListingHeight);
            mainListingHeight = 0f;
            Widgets.BeginScrollView(inRect, ref mainScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);


            // Difficulty
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("Difficulty".Translate());
            listingStandard.GapLine();
            mainListingHeight += Text.LineHeight + 12;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Difficulty.Description".Translate());
            mainListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            foreach (DifficultyDef allDef in DefDatabase<DifficultyDef>.AllDefs)
            {
                TaggedString labelCap = allDef.LabelCap;
                if (allDef.isCustom)
                {
                    labelCap += "...";
                }
                if (listingStandard.RadioButton(labelCap, settings.difficulty == allDef.defName, 0f, allDef.description.ResolveTags(), 0f))
                {
                    settings.difficulty = allDef.defName;
                }
                listingStandard.Gap(3f);
                mainListingHeight += 32f;
            }

            listingStandard.Gap(15f);
            if (listingStandard.RadioButton("ReloadAnytimeMode".Translate(), !settings.permadeath, 0f, "ReloadAnytimeModeInfo".Translate()))
            {
                settings.permadeath = false;
            }
            listingStandard.Gap(3f);
            if (listingStandard.RadioButton("CommitmentMode".TranslateWithBackup("PermadeathMode"), settings.permadeath, 0f, "PermadeathModeInfo".Translate()))
            {
                settings.permadeath = true;
            }
            if (ModsConfig.IsActive("brrainz.nopausechallenge") || ModsConfig.IsActive("brrainz.nopausechallenge_steam"))
            {
                listingStandard.Gap(3f);
                DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.Difficulty.NoPauseChallenge".Translate(), "RandomStartMod.TooltipTitles.NoPauseChallenge".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Difficulty.NoPauseChallengeTooltip".Translate(), ref settings.noPauseEnabled);
                DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.Difficulty.HalfSpeedEnabled".Translate(), "RandomStartMod.TooltipTitles.HalfSpeedEnabled".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Difficulty.HalfSpeedEnabledTooltip".Translate(), ref settings.noPauseHalfSpeedEnabled);
                mainListingHeight += 3f + 32f + 32f;
            }

            mainListingHeight += 15f + 32f + 3f + 32f;

            if (ModsConfig.AnomalyActive)
            {
                listingStandard.Gap();
                // Difficulty
                listingStandard.Gap();
                mainListingHeight += 24;
                Text.Font = GameFont.Medium;
                listingStandard.Label("DifficultyAnomalySection".Translate());
                listingStandard.GapLine();
                mainListingHeight += 12f + Text.LineHeight;
                Text.Font = GameFont.Small;
                Text.Font = GameFont.Tiny;
                Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Difficulty.AnomalyDescription".Translate());
                mainListingHeight += Text.LineHeight;
                Text.Font = GameFont.Small;
                AnomalyPlaystyleDef selectedPlaystyle = DefDatabase<AnomalyPlaystyleDef>.GetNamed(settings.anomalyPlaystyle);
                listingStandard.Label("ChooseAnomalyPlaystyle".Translate());
                listingStandard.Gap();
                mainListingHeight += 12f + Text.LineHeight;
                foreach (AnomalyPlaystyleDef allDef in DefDatabase<AnomalyPlaystyleDef>.AllDefs)
                {
                    string text = allDef.LabelCap.AsTipTitle() + "\n" + allDef.description;
                    if (listingStandard.RadioButton(allDef.LabelCap, selectedPlaystyle == allDef, 30f, text, 0f))
                    {
                        settings.anomalyPlaystyle = allDef.defName;
                    }
                    listingStandard.Gap(3f);
                    mainListingHeight += 3f + 32f;
                }

                listingStandard.Gap();
                mainListingHeight += 12f;

                if (selectedPlaystyle.displayThreatFractionSliders || selectedPlaystyle.overrideThreatFraction)
                {
                    listingStandard.Label("CanBeEditedInStorytellerSettings".Translate() + ":");
                    mainListingHeight += Text.LineHeight;
                }
                if (selectedPlaystyle.displayThreatFractionSliders)
                {
                    TaggedString taggedString = "Difficulty_AnomalyThreatsInactive_Info".Translate();
                    listingStandard.Label("Difficulty_AnomalyThreatsInactive_Label".Translate() + ": " + settings.anomalyThreatsInactiveFraction.ToStringPercent() + " - " + Dialog_AnomalySettings.GetFrequencyLabel(settings.anomalyThreatsInactiveFraction), -1f, taggedString);
                    settings.anomalyThreatsInactiveFraction = listingStandard.Slider(settings.anomalyThreatsInactiveFraction, 0f, 1f);
                    float anomalyThreatsNum = settings.anomalyThreatsActiveFraction;
                    TaggedString taggedString2 = "Difficulty_AnomalyThreatsActive_Info".Translate(Mathf.Clamp01(anomalyThreatsNum).ToStringPercent(), Mathf.Clamp01(anomalyThreatsNum * 1.5f).ToStringPercent());
                    listingStandard.Label("Difficulty_AnomalyThreatsActive_Label".Translate() + ": " + settings.anomalyThreatsActiveFraction.ToStringPercent() + " - " + Dialog_AnomalySettings.GetFrequencyLabel(settings.anomalyThreatsActiveFraction), -1f, taggedString2);
                    settings.anomalyThreatsActiveFraction = listingStandard.Slider(settings.anomalyThreatsActiveFraction, 0f, 1f);
                    mainListingHeight += Text.LineHeight + Text.LineHeight + 32f + 32f;
                }
                else if (selectedPlaystyle.overrideThreatFraction)
                {
                    TaggedString taggedString3 = "Difficulty_AnomalyThreats_Info".Translate();
                    listingStandard.Label("Difficulty_AnomalyThreats_Label".Translate() + ": " + settings.overrideAnomalyThreatsFraction.ToStringPercent() + " - " + Dialog_AnomalySettings.GetFrequencyLabel(settings.overrideAnomalyThreatsFraction), -1f, taggedString3);
                    settings.overrideAnomalyThreatsFraction = listingStandard.Slider(settings.overrideAnomalyThreatsFraction, 0f, 1f);
                    mainListingHeight += Text.LineHeight + 32f;
                }
                if (selectedPlaystyle.displayStudyFactorSlider)
                {
                    listingStandard.Label("Difficulty_StudyEfficiency_Label".Translate() + ": " + settings.studyEfficiencyFactor.ToStringPercent(), -1f, "Difficulty_StudyEfficiency_Info".Translate());
                    settings.studyEfficiencyFactor = listingStandard.Slider(settings.studyEfficiencyFactor, 0f, 5f);
                    mainListingHeight += Text.LineHeight + 32f;
                }
            }

            if (settings.difficulty == "Custom")
            {
                listingStandard.Gap();
                mainListingHeight += 24;
                Text.Font = GameFont.Medium;
                listingStandard.Label("DifficultyCustomSectionLabel".Translate());
                listingStandard.GapLine();
                mainListingHeight += 12f + Text.LineHeight;
                Text.Font = GameFont.Small;
                Text.Font = GameFont.Tiny;
                Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Difficulty.CustomDescription".Translate());
                mainListingHeight += Text.LineHeight;
                Text.Font = GameFont.Small;
                if (listingStandard.ButtonText("DifficultyReset".Translate()))
                {
                    MakeResetDifficultyFloatMenu();
                }

                Listing_Standard listing_Standard = new Listing_Standard();
                float num3 = listingStandard.ColumnWidth;
                listing_Standard.ColumnWidth = num3 / 2f - 17f;
                Rect rect4 = listingStandard.GetRect(1200f);
                listing_Standard.Begin(rect4);
                listing_Standard.Gap();
                float curHeight = listing_Standard.CurHeight;
                float gapHeight = 5f;
                DrawCustomLeft(listing_Standard);
                listing_Standard.Gap(gapHeight);
                listing_Standard.NewColumn();
                listing_Standard.Gap(curHeight);
                DrawCustomRight(listing_Standard);
                listing_Standard.Gap(gapHeight);
                listing_Standard.End();
                mainListingHeight += 1200f;
                mainListingHeight += 1200f;
            }

            listingStandard.Gap();
            mainListingHeight += 12f;
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetDifficulty();
            }
            mainListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();
        }

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
                    string source = GetSourceModMetaData(scenarioDef).Name;
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

        private void DoPlanetSettingsTabContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 60f, inRect.width, planetListingHeight);
            planetListingHeight = 0;
            Widgets.BeginScrollView(inRect, ref planetScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            //Planet Coverage
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Planet.CoverageTitle".Translate());
            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Planet.CoverageDescription".Translate());
            planetListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            if (listingStandard.ButtonText(settings.planetCoverage.ToStringPercent()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (float coverage in PlanetCoverages)
                {
                    string text = coverage.ToStringPercent();
                    if (coverage <= 0.1f)
                    {
                        text += " (dev)";
                    }
                    FloatMenuOption item = new FloatMenuOption(text, delegate
                    {
                        if (settings.planetCoverage != coverage)
                        {
                            settings.planetCoverage = coverage;
                            if (settings.planetCoverage == 1f)
                            {
                                Messages.Message("MessageMaxPlanetCoveragePerformanceWarning".Translate(), MessageTypeDefOf.CautionInput, historical: false);
                            }
                        }
                    });
                    list.Add(item);
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            planetListingHeight += 32f;

            //Map Size
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Planet.MapSizeTitle".Translate());
            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Planet.MapSizeDescription".Translate());
            planetListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            IEnumerable<int> enumerable = MapSizes.AsEnumerable();
            if (listingStandard.ButtonText("MapSizeDesc".Translate(settings.mapSize, settings.mapSize * settings.mapSize)))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (int size in enumerable)
                {
                    string text = "MapSizeDesc".Translate(size, size * size);
                    FloatMenuOption item = new FloatMenuOption(text, delegate
                    {
                        if (settings.mapSize != size)
                        {
                            settings.mapSize = size;
                        }
                    });
                    list.Add(item);
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            planetListingHeight += 32f;

            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Planet.GenerationTitle".Translate());
            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Planet.GenerationDescription".Translate());
            planetListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;
            listingStandard.Gap();
            planetListingHeight += 12f;
            listingStandard.Label("RandomStartMod.Planet.RandomizeLabel".Translate());
            listingStandard.Gap();
            planetListingHeight += 12f;

            DoSettingToggle(listingStandard.GetRect(24f), "WorldSeed".Translate(), "RandomStartMod.TooltipTitles.RandomiseWorldSeed".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomiseWorldSeedTooltip".Translate(), ref settings.randomiseWorldSeed);
            planetListingHeight += 24f + Text.LineHeight;

            if (!settings.randomiseWorldSeed)
            {
                listingStandard.Gap(5f);
                Rect worldSeedRect = listingStandard.GetRect(30f);
                settings.worldSeed = Widgets.TextField(worldSeedRect, settings.worldSeed);
                planetListingHeight += 35f;
                listingStandard.Gap(2f);
                if (listingStandard.ButtonText("RandomizeSeed".Translate()))
                {
                    settings.worldSeed = GenText.RandomSeedString();
                }
                planetListingHeight += 34f;
            }

            listingStandard.Gap();

            DoSettingToggle(listingStandard.GetRect(24f), "PlanetRainfall".Translate(), "RandomStartMod.TooltipTitles.RandomizeRainfall".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizeRainfallTooltip".Translate(), ref settings.randomiseRainfall);
            planetListingHeight += 24f + Text.LineHeight;


            if (!settings.randomiseRainfall)
            {
                listingStandard.Gap(2f);
                Rect rainfallRect = listingStandard.GetRect(30f);
                if (ModsConfig.IsActive("koth.RealisticPlanets1.6") || ModsConfig.IsActive("koth.RealisticPlanets1.6_Steam"))
                {
                    settings.rainfall = Mathf.RoundToInt(Widgets.HorizontalSlider(rainfallRect, settings.rainfall, 0, 4, middleAlignment: true, Util.GetIntLabelShort(settings.rainfall), null, null, 1f));
                }
                else
                {
                    settings.rainfall = Mathf.RoundToInt(Widgets.HorizontalSlider(rainfallRect, settings.rainfall, 0, OverallRainfallUtility.EnumValuesCount - 1, middleAlignment: true, Util.GetIntLabel(settings.rainfall), null, null, 1f));
                }
                planetListingHeight += 32f;
            }
            else
            {
                Rect rainfallRect = listingStandard.GetRect(32f);
                if (ModsConfig.IsActive("koth.RealisticPlanets1.6") || ModsConfig.IsActive("koth.RealisticPlanets1.6_Steam"))
                {
                    Widgets.IntRange(rainfallRect, 1623498654, ref settings.randomiseRainfallRange, 0, 4, Util.GetIntRangeLabelShort(settings.randomiseRainfallRange));
                }
                else
                {
                    Widgets.IntRange(rainfallRect, 1623498654, ref settings.randomiseRainfallRange, 0, OverallRainfallUtility.EnumValuesCount - 1, Util.GetIntRangeLabel(settings.randomiseRainfallRange));
                }
                planetListingHeight += 32f;
            }

            listingStandard.Gap();

            DoSettingToggle(listingStandard.GetRect(24f), "PlanetTemperature".Translate(), "RandomStartMod.TooltipTitles.RandomizeTemperature".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizeTemperatureTooltip".Translate(), ref settings.randomiseTemperature);
            planetListingHeight += 12f + 24;

            if (!settings.randomiseTemperature)
            {
                listingStandard.Gap(2f);
                Rect temperatureRect = listingStandard.GetRect(30f);
                settings.temperature = Mathf.RoundToInt(Widgets.HorizontalSlider(temperatureRect, (float)settings.temperature, 0f, OverallTemperatureUtility.EnumValuesCount - 1, middleAlignment: true, Util.GetIntLabel(settings.temperature), null, null, 1f));
                planetListingHeight += 32f;
            }
            else
            {
                Rect temperatureRect = listingStandard.GetRect(32f);
                Widgets.IntRange(temperatureRect, 1623498655, ref settings.randomiseTemperatureRange, 0, 6, Util.GetIntRangeLabel(settings.randomiseTemperatureRange));
                planetListingHeight += 32f;
            }

            listingStandard.Gap();
            DoSettingToggle(listingStandard.GetRect(24f), "PlanetPopulation".Translate(), "RandomStartMod.TooltipTitles.RandomizePopulation".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizePopulationTooltip".Translate(), ref settings.randomisePopulation);
            planetListingHeight += 12f + 24f;

            if (!settings.randomisePopulation)
            {
                listingStandard.Gap(2f);
                Rect populationRect = listingStandard.GetRect(30f);
                settings.population = Mathf.RoundToInt(Widgets.HorizontalSlider(populationRect, (float)settings.population, 0f, OverallPopulationUtility.EnumValuesCount - 1, middleAlignment: true, Util.GetIntLabel(settings.population), null, null, 1f));
                planetListingHeight += 32f;
            }
            else
            {
                Rect populationRect = listingStandard.GetRect(32f);
                Widgets.IntRange(populationRect, 1623498656, ref settings.randomisePopulationRange, 0, 6, Util.GetIntRangeLabel(settings.randomisePopulationRange));
                planetListingHeight += 32f;
            }

            listingStandard.Gap();
            DoSettingToggle(listingStandard.GetRect(24f), "PlanetLandmarkDensity".Translate(), null, ref settings.randomiseLandmarkDensity);
            planetListingHeight += 12f + 24f;

            if (!settings.randomiseLandmarkDensity)
            {
                listingStandard.Gap(2f);
                Rect landmarkRect = listingStandard.GetRect(30f);
                settings.landmarkDensity = Mathf.RoundToInt(Widgets.HorizontalSlider(landmarkRect, (float)settings.landmarkDensity, 0f, LandmarkDensityUtility.EnumValuesCount - 1, middleAlignment: true, Util.GetIntLabel(settings.landmarkDensity), null, null, 1f));
                planetListingHeight += 32f;
            }
            else
            {
                Rect landmarkRect = listingStandard.GetRect(32f);
                Widgets.IntRange(landmarkRect, 1623498668, ref settings.randomiseLandmarkDensityRange, 0, LandmarkDensityUtility.EnumValuesCount - 1, Util.GetIntRangeLabel(settings.randomiseLandmarkDensityRange));
                planetListingHeight += 32f;
            }

            listingStandard.Gap();
            DoSettingToggle(listingStandard.GetRect(24f), "Pollution_Label".Translate(), "RandomStartMod.TooltipTitles.RandomizePollution".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizePollutionTooltip".Translate(), ref settings.randomisePollution);
            planetListingHeight += 12f + 24f;

            if (!settings.randomisePollution)
            {
                listingStandard.Gap(2f);
                Rect pollutionRect = listingStandard.GetRect(30f);
                settings.pollution = Widgets.HorizontalSlider(pollutionRect, settings.pollution, 0f, 1f, middleAlignment: true, settings.pollution.ToStringPercent(), null, null, 0.05f);
                planetListingHeight += 32f;
            }
            else
            {
                Rect pollutionRect = listingStandard.GetRect(32f);
                Widgets.FloatRange(pollutionRect, 1623498651, ref settings.randomisePollutionRange, 0.0f, 1.0f, Util.GetFloatRangeLabelPercent(settings.randomisePollutionRange));
                planetListingHeight += 32f;
            }

            listingStandard.Gap();

            DoSettingToggle(listingStandard.GetRect(24f), "MapStartSeason".Translate(), "RandomStartMod.TooltipTitles.RandomizeStartingSeason".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizeStartingSeasonTooltip".Translate(), ref settings.randomiseSeason);
            planetListingHeight += 12f + 32f;

            if (!settings.randomiseSeason)
            {
                listingStandard.Gap();
                IEnumerable<int> seasons = Seasons.AsEnumerable();
                Season settingSeason = (Season)settings.startingSeason;
                if (listingStandard.ButtonText(settingSeason.LabelCap()))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (int season in seasons)
                    {
                        Season labelSeason = (Season)season;
                        string text = labelSeason.LabelCap();
                        FloatMenuOption item = new FloatMenuOption(text, delegate
                        {
                            if (settings.startingSeason != season)
                            {
                                settings.startingSeason = season;
                            }
                        });
                        list.Add(item);
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                planetListingHeight += 32f;
            }

            if (ModsConfig.IsActive("Oblitus.MyLittlePlanet") || ModsConfig.IsActive("Oblitus.MyLittlePlanet_Steam"))
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("My Little Planet");
                listingStandard.GapLine();
                planetListingHeight += 12f + 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                listingStandard.Label("MLPWorldPlanetSize".Translate());
                planetListingHeight += Text.LineHeight;
                Rect mlpRect = listingStandard.GetRect(30f);
                Compat.MLPCompat.DrawMLPSlider(mlpRect);
                planetListingHeight += 30f;
            }

            if (ModsConfig.IsActive("koth.RealisticPlanets1.6") || ModsConfig.IsActive("koth.RealisticPlanets1.6_Steam"))
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("Realistic Planets");
                listingStandard.GapLine();
                planetListingHeight += 36f + Text.LineHeight;
                Text.Font = GameFont.Small;
                DoSettingToggle(listingStandard.GetRect(24f), "Planets.WorldPresets".Translate(), null, ref settings.randomiseRealisticPlanets);
                TooltipHandler.TipRegion(new Rect(0f, planetListingHeight - 18, inRect.width, 30), "RandomStartMod.RandomiseWorldPresetsTip".Translate());
                planetListingHeight += 44f;
                if (!settings.randomiseRealisticPlanets)
                {
                    RealisticPlanetsCompat.DoWorldTypeSelectionButton(listingStandard);
                    planetListingHeight += 32f;
                }
                listingStandard.Gap();
                DoSettingToggle(listingStandard.GetRect(24f), "Planets.OceanType".Translate(), null, ref settings.randomiseOceanType);
                planetListingHeight += 36f;
                if (!settings.randomiseOceanType)
                {
                    listingStandard.Gap(2f);
                    Rect rect4 = listingStandard.GetRect(30f);
                    settings.realisticPlanetsOceanType = Mathf.RoundToInt(Widgets.HorizontalSlider(rect4, settings.realisticPlanetsOceanType, 0, Planets_Code.WorldGen.WorldTypeUtility.EnumValuesCount - 1, middleAlignment: true, Util.GetIntLabel(settings.realisticPlanetsOceanType), null, null, 1f));
                    planetListingHeight += 32f;
                }
                else
                {
                    Rect rect5 = listingStandard.GetRect(32f);
                    Widgets.IntRange(rect5, 1623498657, ref settings.randomiseOceanTypeRange, 0, Planets_Code.WorldGen.WorldTypeUtility.EnumValuesCount - 1, Util.GetIntRangeLabel(settings.randomiseOceanTypeRange));
                    planetListingHeight += 32f;
                }
                DoSettingToggle(listingStandard.GetRect(24f), "Planets.AxialTilt".Translate(), null, ref settings.randomiseAxialTilt);
                planetListingHeight += 36f;
                if (!settings.randomiseAxialTilt)
                {
                    listingStandard.Gap(2f);
                    Rect rect4 = listingStandard.GetRect(30f);
                    settings.realisticPlanetsAxialTilt = Mathf.RoundToInt(Widgets.HorizontalSlider(rect4, settings.realisticPlanetsAxialTilt, 0, Planets_Code.WorldGen.AxialTiltUtility.EnumValuesCount - 1, middleAlignment: true, Util.GetIntLabelShort(settings.realisticPlanetsAxialTilt), null, null, 1f));
                    planetListingHeight += 32f;
                }
                else
                {
                    Rect rect5 = listingStandard.GetRect(32f);
                    Widgets.IntRange(rect5, 1623498659, ref settings.randomiseAxialTiltRange, 0, Planets_Code.WorldGen.AxialTiltUtility.EnumValuesCount - 1, Util.GetIntRangeLabelShort(settings.randomiseAxialTiltRange));
                    planetListingHeight += 32f;
                }
                listingStandard.Gap();
                DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.UseRPWordType".Translate(), null, ref settings.realisticPlanetsUseWordType);
                planetListingHeight += 36f;
                if (settings.realisticPlanetsUseWordType)
                {
                    listingStandard.Gap(2f);
                    Rect rect8 = listingStandard.GetRect(30f);
                    settings.realisticPlanetsUseWordTypeChance = Widgets.HorizontalSlider(rect8, settings.realisticPlanetsUseWordTypeChance, 0f, 1f, middleAlignment: true, settings.realisticPlanetsUseWordTypeChance.ToStringPercent(), null, null, 0.05f);
                    planetListingHeight += 32f;
                }
                planetListingHeight += 32f;
                listingStandard.Gap();
            }

            if (ModsConfig.IsActive("Woolstrand.RealRuins") || ModsConfig.IsActive("Woolstrand.RealRuins_Steam"))
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("Real Ruins");
                listingStandard.GapLine();
                planetListingHeight += 12f + 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                DoSettingToggle(listingStandard.GetRect(24f), "RunInBackground".Translate(), "Auto Real Ruins".AsTipTitle() + "\n\n" + "Automatically enable Real Ruins generation to populate the world with ruins from other players' colonies, adding variety and exploration opportunities.", ref settings.enableAutoRealRuins);
                planetListingHeight += 24f;
                DoSettingToggle(listingStandard.GetRect(24f), "RealRuins.BiomeFiltering".Translate(), "Real Ruins Biome Filtering".AsTipTitle() + "\n\n" + "Filter Real Ruins based on biome compatibility, ensuring ruins appear in appropriate environments that match their original settings.", ref settings.realRuinsBiomeFilter);
                planetListingHeight += 24f;
            }

            listingStandard.Gap();
            planetListingHeight += 12f;
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetPlanet();
            }
            planetListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();
        }

        private void DoOptionalFeaturesTabContents(Rect inRect)
        {
            if (settings == null)
            {
                Util.LogMessage("Settings is null in DoOptionalFeaturesTabContents");
                return;
            }

            Rect rect = new Rect(0f, 60f, inRect.width, optionalFeaturesListingHeight);
            optionalFeaturesListingHeight = 0f;
            Widgets.BeginScrollView(inRect, ref mainScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            if (ModsConfig.BiotechActive)
            {
                Text.Font = GameFont.Medium;
                listingStandard.Label("RandomStartMod.Misc.GenesTitle".Translate());
                listingStandard.GapLine();
                optionalFeaturesListingHeight += 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                Text.Font = GameFont.Tiny;
                Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.GenesDescription".Translate());
                optionalFeaturesListingHeight += Text.LineHeight;
                Text.Font = GameFont.Small;
                DoSettingToggle(listingStandard.GetRect(24f), $"{"Randomize".Translate()}: {"Xenotype".Translate()}", "RandomStartMod.TooltipTitles.RandomizeXenotype".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.RandomizeXenotypeTooltip".Translate(), ref settings.enableRandomXenotypes);
                optionalFeaturesListingHeight += 24f;
                DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.RespectMemberXenotypes".Translate(), "RandomStartMod.TooltipTitles.RespectFactionXenotypes".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.RespectFactionXenotypesTooltip".Translate(), ref settings.respectFactionXenotypes);
                optionalFeaturesListingHeight += 24f;
                DoSettingToggle(listingStandard.GetRect(24f), $"{"Randomize".Translate()}: {"Genes".Translate().CapitalizeFirst()}", "RandomStartMod.TooltipTitles.RandomizeCustomGenes".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.RandomizeCustomGenesTooltip".Translate(), ref settings.enableRandomCustomXenotypes);
                optionalFeaturesListingHeight += 24f;
                if (settings.enableRandomCustomXenotypes)
                {
                    listingStandard.Gap();
                    listingStandard.Indent(8);
                    listingStandard.Label("RandomStartMod.Misc.CountLabel".Translate());
                    listingStandard.Outdent(8);
                    optionalFeaturesListingHeight += Text.LineHeight + 12f;
                    Widgets.IntRange(listingStandard.GetRect(32f), 382399865, ref settings.randomGeneRange, 0, 20);
                    optionalFeaturesListingHeight += 32f;
                    listingStandard.Gap(2f);
                    DoSettingToggle(listingStandard.GetRect(24f), $"{"minimum".Translate().CapitalizeFirst()}: {"MetabolismTotal".Translate()}", "RandomStartMod.TooltipTitles.MinimumMetabolicEfficiency".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.MinimumMetabolicEfficiencyTooltip".Translate(), ref settings.enableMetabolicEfficiencyMinimum);
                    optionalFeaturesListingHeight += 2f + 24f;
                    if (settings.enableMetabolicEfficiencyMinimum)
                    {
                        listingStandard.Gap(2f);
                        settings.minimumMetabolicEfficiency = Mathf.RoundToInt(Widgets.HorizontalSlider(listingStandard.GetRect(24f), settings.minimumMetabolicEfficiency, -5, 5, true, $"{settings.minimumMetabolicEfficiency.ToStringWithSign()} ({"HungerRate".Translate()} x{GeneTuning.MetabolismToFoodConsumptionFactorCurve.Evaluate(settings.minimumMetabolicEfficiency).ToStringPercent()})", null, null, 1f));
                        optionalFeaturesListingHeight += 2f + 24f;
                    }
                }
            }
            if (ModsConfig.IdeologyActive)
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("RandomStartMod.Misc.IdeologyTitle".Translate());
                listingStandard.GapLine();
                optionalFeaturesListingHeight += 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                Text.Font = GameFont.Tiny;
                Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.IdeologyDescription".Translate());
                optionalFeaturesListingHeight += Text.LineHeight;
                Text.Font = GameFont.Small;
                DoSettingToggle(listingStandard.GetRect(24f), "PlayClassic".Translate(), "RandomStartMod.TooltipTitles.PlayClassicMode".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.PlayClassicModeTooltip".Translate(), ref settings.disableIdeo);
                optionalFeaturesListingHeight += 24f;
                if (!settings.disableIdeo)
                {
                    DoSettingToggle(listingStandard.GetRect(24f), "CreateFluid".Translate(), "FluidIdeoTip".Translate(), ref settings.fluidIdeo);
                    optionalFeaturesListingHeight += 24f;

                    // Random Meme Range
                    listingStandard.Indent(8);
                    listingStandard.Label("RandomStartMod.Misc.RandomMemeCountLabel".Translate());
                    listingStandard.Outdent(8);
                    optionalFeaturesListingHeight += Text.LineHeight + 12f;
                    Widgets.IntRange(listingStandard.GetRect(32f), 382399866, ref settings.randomMemeRange, 1, 10);
                    optionalFeaturesListingHeight += 32f;
                    listingStandard.Gap(2f);
                    optionalFeaturesListingHeight += 2f;

                    // Ensure ideology lists are initialized
                    if (settings.forcedMemes == null) settings.forcedMemes = new List<string>();
                    if (settings.disallowedMemes == null) settings.disallowedMemes = new List<string>();
                    if (settings.disallowedPrecepts == null) settings.disallowedPrecepts = new List<string>();

                    // Forced Memes Section
                    listingStandard.Gap();
                    Text.Font = GameFont.Medium;
                    listingStandard.Label("RandomStartMod.Misc.ForcedMemesTitle".Translate());
                    listingStandard.GapLine();
                    optionalFeaturesListingHeight += 24f + Text.LineHeight;
                    Text.Font = GameFont.Small;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.ForcedMemesDescription".Translate());
                    optionalFeaturesListingHeight += Text.LineHeight;
                    Text.Font = GameFont.Small;

                    List<MemeDef> forcedMemes = new List<MemeDef>();
                    foreach (string memeDefName in settings.forcedMemes)
                    {
                        MemeDef meme = DefDatabase<MemeDef>.GetNamed(memeDefName, false);
                        if (meme != null)
                            forcedMemes.Add(meme);
                    }
                    for (int i = 0; i < forcedMemes.Count; i++)
                    {
                        listingStandard.Gap(4f);
                        if (DoMemeRow(listingStandard.GetRect(24f), forcedMemes[i], settings.forcedMemes, i))
                        {
                            i--;
                        }
                        listingStandard.Gap(4f);
                        optionalFeaturesListingHeight += 32f;
                    }

                    List<FloatMenuOption> forcedMemeOptions = new List<FloatMenuOption>();
                    foreach (MemeDef memeDef in DefDatabase<MemeDef>.AllDefs.OrderBy(x => x.LabelCap.ToString()))
                    {
                        MemeDef localDef = memeDef;
                        string text = localDef.LabelCap;
                        Action action = () => settings.forcedMemes.Add(localDef.defName);

                        AcceptanceReport acceptanceReport = CanAddForcedMeme(localDef);
                        if (!acceptanceReport)
                        {
                            action = null;
                            if (!acceptanceReport.Reason.NullOrEmpty())
                            {
                                text = text + " (" + acceptanceReport.Reason + ")";
                            }
                        }

                        Texture2D icon = localDef.Icon ?? GetSourceIcon(localDef);
                        FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, icon, Color.white, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                        floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.description;
                        forcedMemeOptions.Add(floatMenuOption);
                    }
                    if (forcedMemeOptions.Count > 0)
                    {
                        if (listingStandard.ButtonText("Add".Translate().CapitalizeFirst() + "..."))
                        {
                            Find.WindowStack.Add(new FloatMenu(forcedMemeOptions));
                        }
                        optionalFeaturesListingHeight += 28f;
                    }

                    // Disallowed Memes Section
                    listingStandard.Gap();
                    Text.Font = GameFont.Medium;
                    listingStandard.Label("RandomStartMod.Misc.DisallowedMemesTitle".Translate());
                    listingStandard.GapLine();
                    optionalFeaturesListingHeight += 24f + Text.LineHeight;
                    Text.Font = GameFont.Small;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.DisallowedMemesDescription".Translate());
                    optionalFeaturesListingHeight += Text.LineHeight;
                    Text.Font = GameFont.Small;

                    List<MemeDef> disallowedMemes = new List<MemeDef>();
                    foreach (string memeDefName in settings.disallowedMemes)
                    {
                        MemeDef meme = DefDatabase<MemeDef>.GetNamed(memeDefName, false);
                        if (meme != null)
                            disallowedMemes.Add(meme);
                    }
                    for (int i = 0; i < disallowedMemes.Count; i++)
                    {
                        listingStandard.Gap(4f);
                        if (DoMemeRow(listingStandard.GetRect(24f), disallowedMemes[i], settings.disallowedMemes, i))
                        {
                            i--;
                        }
                        listingStandard.Gap(4f);
                        optionalFeaturesListingHeight += 32f;
                    }

                    List<FloatMenuOption> disallowedMemeOptions = new List<FloatMenuOption>();
                    foreach (MemeDef memeDef in DefDatabase<MemeDef>.AllDefs.OrderBy(x => x.LabelCap.ToString()))
                    {
                        if (!settings.disallowedMemes.Contains(memeDef.defName))
                        {
                            MemeDef localDef = memeDef;
                            string text = localDef.LabelCap;
                            Action action = () => settings.disallowedMemes.Add(localDef.defName);
                            Texture2D icon = localDef.Icon ?? GetSourceIcon(localDef);
                            FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, icon, Color.white, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                            floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.description;
                            disallowedMemeOptions.Add(floatMenuOption);
                        }
                    }
                    if (disallowedMemeOptions.Count > 0)
                    {
                        if (listingStandard.ButtonText("Add".Translate().CapitalizeFirst() + "..."))
                        {
                            Find.WindowStack.Add(new FloatMenu(disallowedMemeOptions));
                        }
                        optionalFeaturesListingHeight += 28f;
                    }

                    // Disallowed Precepts Section
                    listingStandard.Gap();
                    Text.Font = GameFont.Medium;
                    listingStandard.Label("RandomStartMod.Misc.DisallowedPrecepts".Translate());
                    listingStandard.GapLine();
                    optionalFeaturesListingHeight += 24f + Text.LineHeight;
                    Text.Font = GameFont.Small;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.DisallowedPreceptsDescription".Translate());
                    optionalFeaturesListingHeight += Text.LineHeight;
                    Text.Font = GameFont.Small;
                    Text.Font = GameFont.Small;

                    List<PreceptDef> disallowedPrecepts = new List<PreceptDef>();
                    foreach (string preceptDefName in settings.disallowedPrecepts)
                    {
                        PreceptDef precept = DefDatabase<PreceptDef>.GetNamed(preceptDefName, false);
                        if (precept != null)
                            disallowedPrecepts.Add(precept);
                    }
                    for (int i = 0; i < disallowedPrecepts.Count; i++)
                    {
                        listingStandard.Gap(4f);
                        if (DoPreceptRow(listingStandard.GetRect(24f), disallowedPrecepts[i], settings.disallowedPrecepts, i))
                        {
                            i--;
                        }
                        listingStandard.Gap(4f);
                        optionalFeaturesListingHeight += 32f;
                    }

                    List<FloatMenuOption> disallowedPreceptOptions = new List<FloatMenuOption>();
                    foreach (PreceptDef preceptDef in DefDatabase<PreceptDef>.AllDefs.OrderBy(x => x.issue?.LabelCap.ToString() ?? x.LabelCap.ToString()))
                    {
                        if (!settings.disallowedPrecepts.Contains(preceptDef.defName) && preceptDef.visibleOnAddFloatMenu)
                        {
                            PreceptDef localDef = preceptDef;
                            string text = IdeoUIUtility.GetPreceptLabel(localDef, localDef.ritualPatternBase);
                            bool flag = IdeoUIUtility.IsBasicPrecept(localDef);
                            if (flag)
                            {
                                text = localDef.issue.LabelCap + ": " + text;
                                if (localDef.issue == null || text == "")
                                {
                                    continue; // Skip if something is empty
                                }
                            }
                            Action action = () => settings.disallowedPrecepts.Add(localDef.defName);
                            Texture2D icon = localDef.Icon ?? GetSourceIcon(localDef);
                            FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, icon, Color.white, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                            floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.description;
                            disallowedPreceptOptions.Add(floatMenuOption);
                        }
                    }
                    if (disallowedPreceptOptions.Count > 0)
                    {
                        if (listingStandard.ButtonText("Add".Translate().CapitalizeFirst() + "..."))
                        {
                            Find.WindowStack.Add(new FloatMenu(disallowedPreceptOptions));
                        }
                        optionalFeaturesListingHeight += 28f;
                    }

                    if (GenFilePaths.AllCustomIdeoFiles.Any())
                    {
                        DoSettingToggle(listingStandard.GetRect(24f), "LoadExistingIdeoligion".Translate(), "RandomStartMod.TooltipTitles.LoadExistingIdeology".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.LoadExistingIdeologyTooltip".Translate(), ref settings.overrideIdeo);
                        optionalFeaturesListingHeight += 24f;
                        if (settings.overrideIdeo)
                        {

                            string text = "ModClickToSelect".Translate();
                            if (File.Exists(settings.customIdeoOverrideFile))
                            {
                                FileInfo currentFile = new FileInfo(settings.customIdeoOverrideFile);
                                text = currentFile.Name.Replace(".rid", "");
                            }
                            if (listingStandard.ButtonText(text))
                            {

                                List<FloatMenuOption> list = new List<FloatMenuOption>();
                                foreach (System.IO.FileInfo fileInfo in GenFilePaths.AllCustomIdeoFiles)
                                {
                                    text = fileInfo.Name.Replace(".rid", "");
                                    FloatMenuOption item = new FloatMenuOption(text, delegate
                                    {
                                        if (settings.customIdeoOverrideFile != fileInfo.FullPath)
                                        {
                                            settings.customIdeoOverrideFile = fileInfo.FullPath;
                                        }
                                    });
                                    list.Add(item);
                                }
                                Find.WindowStack.Add(new FloatMenu(list));
                            }
                            optionalFeaturesListingHeight += 32f;
                        }
                    }
                }

            }

            listingStandard.Gap();
            optionalFeaturesListingHeight += 12f;
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetOptionalFeatures();
            }
            optionalFeaturesListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();
        }

        private void DoStartingTileSettingsTabContents(Rect inRect)
        {
            if (settings == null)
            {
                Util.LogMessage("Settings is null in DoStartingTileSettingsTabContents");
                return;
            }

            // Clean up any excluded biomes from the settings
            CleanUpExcludedBiomes();
            CleanUpInvalidHilliness();

            Rect rect = new Rect(0f, 60f, inRect.width, startingTileListingHeight);
            startingTileListingHeight = 0f;
            Widgets.BeginScrollView(inRect, ref startingTileScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.StartingTile.Title".Translate());
            listingStandard.GapLine();
            startingTileListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            listingStandard.Gap();
            startingTileListingHeight += 12f;

            // Filter Starting Biome toggle
            DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.StartingTile.FilterStartingBiome".Translate(), "RandomStartMod.TooltipTitles.FilterStartingBiome".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.StartingTile.FilterStartingBiomeTooltip".Translate(), ref settings.filterStartingBiome);
            startingTileListingHeight += 24f;

            if (settings.filterStartingBiome)
            {
                listingStandard.Gap();
                listingStandard.Label("RandomStartMod.StartingTile.AllowedBiomes".Translate());
                startingTileListingHeight += 12f + Text.LineHeight;

                // Ensure allowedBiomes list is not null
                if (settings.allowedBiomes == null)
                {
                    settings.allowedBiomes = new List<string>();
                }

                // Display current allowed biomes (only those with canBuildBase=true)
                List<BiomeDef> biomesAllowed = new List<BiomeDef>();
                foreach (string biomeDefName in settings.allowedBiomes)
                {
                    BiomeDef biome = DefDatabase<BiomeDef>.GetNamed(biomeDefName, false);
                    if (biome == null || !biome.canBuildBase || !biome.canAutoChoose)
                        continue;
                    biomesAllowed.Add(biome);
                }
                for (int i = 0; i < biomesAllowed.Count; i++)
                {
                    listingStandard.Gap(4f);
                    if (DoBiomeRow(listingStandard.GetRect(24f), biomesAllowed[i], settings.allowedBiomes, i))
                    {
                        i--;
                    }
                    listingStandard.Gap(4f);
                    startingTileListingHeight += 32f;
                }

                if (biomesAllowed.Count == 0)
                {
                    listingStandard.Label("RandomStartMod.StartingTile.NoBiomesSelected".Translate());
                    startingTileListingHeight += Text.LineHeight;
                }

                listingStandard.Gap();
                startingTileListingHeight += 12f;

                // Add biome button
                if (listingStandard.ButtonText("RandomStartMod.StartingTile.AddBiome".Translate()))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    // Manually filter out underground biomes that don't appear on planet surface
                    string[] excludedBiomes = { "Underground", "Labyrinth", "MetalHell", "Undercave" };
                    IEnumerable<BiomeDef> availableBiomes = DefDatabase<BiomeDef>.AllDefs.Where(b => b != null && b.canBuildBase && b.implemented && b.canAutoChoose && !excludedBiomes.Contains(b.defName));

                    foreach (BiomeDef biomeDef in availableBiomes)
                    {
                        BiomeDef localDef = biomeDef;
                        string text = localDef.LabelCap;
                        Action action = delegate
                        {
                            settings.allowedBiomes.Add(localDef.defName);
                        };
                        AcceptanceReport acceptanceReport = CanAddBiome(localDef);
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
                            int num = biomesAllowed.Count((BiomeDef x) => x == localDef);
                            if (num > 0)
                            {
                                text = text + " (" + num + ")";
                            }
                        }
                        Texture2D biomeIcon = BaseContent.BadTex; // Simple fallback for now
                        if (!biomeDef.texture.NullOrEmpty())
                        {
                            biomeIcon = ContentFinder<Texture2D>.Get(biomeDef.texture, false) ?? BaseContent.BadTex;
                        }
                        FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, biomeIcon, Color.white, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                        floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.description;
                        list.Add(floatMenuOption);
                    }

                    if (list.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }
                startingTileListingHeight += 32f + 12f;
            }

            listingStandard.Gap();
            startingTileListingHeight += 12f;

            // Filter Starting Hilliness toggle
            DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.StartingTile.FilterStartingHilliness".Translate(), "RandomStartMod.TooltipTitles.FilterStartingHilliness".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.StartingTile.FilterStartingHillinessTooltip".Translate(), ref settings.filterStartingHilliness);
            startingTileListingHeight += 24f;

            if (settings.filterStartingHilliness)
            {
                listingStandard.Gap();
                listingStandard.Label("RandomStartMod.StartingTile.AllowedHilliness".Translate());
                startingTileListingHeight += 12f + Text.LineHeight;

                // Ensure allowedHilliness list is not null
                if (settings.allowedHilliness == null)
                {
                    settings.allowedHilliness = new List<int>();
                }

                // Display current allowed hilliness types
                List<int> hillinessTypes = new List<int> { 1, 2, 3, 4 }; // Flat, SmallHills, LargeHills, Mountainous
                List<int> hillinessAllowed = new List<int>();
                foreach (int hillinessValue in settings.allowedHilliness)
                {
                    if (hillinessTypes.Contains(hillinessValue))
                    {
                        hillinessAllowed.Add(hillinessValue);
                    }
                }

                for (int i = 0; i < hillinessAllowed.Count; i++)
                {
                    listingStandard.Gap(4f);
                    if (DoHillinessRow(listingStandard.GetRect(24f), hillinessAllowed[i], settings.allowedHilliness, i))
                    {
                        i--;
                    }
                    listingStandard.Gap(4f);
                    startingTileListingHeight += 32f;
                }

                if (hillinessAllowed.Count == 0)
                {
                    listingStandard.Label("RandomStartMod.StartingTile.NoHillinessSelected".Translate());
                    startingTileListingHeight += Text.LineHeight;
                }

                listingStandard.Gap();
                startingTileListingHeight += 12f;

                // Add hilliness button
                if (listingStandard.ButtonText("RandomStartMod.StartingTile.AddHilliness".Translate()))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    List<int> availableHilliness = new List<int> { 1, 2, 3, 4 }; // Flat, SmallHills, LargeHills, Mountainous

                    foreach (int hillinessValue in availableHilliness)
                    {
                        int localValue = hillinessValue;
                        string hillinessKey = "";
                        switch (hillinessValue)
                        {
                            case 1: hillinessKey = "Hilliness_Flat"; break;
                            case 2: hillinessKey = "Hilliness_SmallHills"; break;
                            case 3: hillinessKey = "Hilliness_LargeHills"; break;
                            case 4: hillinessKey = "Hilliness_Mountainous"; break;
                        }

                        string text = hillinessKey.Translate();
                        Action action = delegate
                        {
                            settings.allowedHilliness.Add(localValue);
                        };

                        AcceptanceReport acceptanceReport = CanAddHilliness(localValue);
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
                            int num = hillinessAllowed.Count((int x) => x == localValue);
                            if (num > 0)
                            {
                                text = text + " (" + num + ")";
                            }
                        }

                        FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, BaseContent.BadTex, Color.white);
                        list.Add(floatMenuOption);
                    }

                    if (list.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }
                startingTileListingHeight += 32f + 12f;
            }

            listingStandard.Gap();
            startingTileListingHeight += 36f + Text.LineHeight;
            Text.Font = GameFont.Small;
            DoSettingToggle(listingStandard.GetRect(24f), "Temperature".Translate(), null, ref settings.limitStartingTileTemperature);
            startingTileListingHeight += 36f + Text.LineHeight;
            if (settings.limitStartingTileTemperature)
            {
                Widgets.FloatRange(listingStandard.GetRect(32f), 1623498652, ref settings.limitStartingTileTemperatureRange, -50f, 50f);
                startingTileListingHeight += 32f + 12f;
            }

            listingStandard.Gap();
            startingTileListingHeight += 12f;

            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetStartingTile();
            }
            startingTileListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();
        }

        private void DoCharactersSettingsTabContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 60f, inRect.width, planetListingHeight);
            planetListingHeight = 0f;
            Widgets.BeginScrollView(inRect, ref planetScrollPosition, rect, showScrollbars: false);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Characters".Translate());

            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
            optionalFeaturesListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.StartingColonistsDescription".Translate());
            optionalFeaturesListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;
            DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.TooltipTitles.StartingPawnsMustBeCapableOfViolence".Translate(), "RandomStartMod.TooltipTitles.StartingPawnsMustBeCapableOfViolence".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.StartingPawnsMustBeCapableOfViolenceTooltip".Translate(), ref settings.startingPawnForceViolence);
            optionalFeaturesListingHeight += 24f;

            listingStandard.Gap();
            Text.Font = GameFont.Small;
            planetListingHeight += 12f;
            listingStandard.Label("RandomStartMod.Characters".Translate() + " 1:");

            listingStandard.Gap();
            planetListingHeight += 12f;
            DoSettingToggle(listingStandard.GetRect(24f), "Name".Translate(), null, ref settings.randomisePawnName);
            planetListingHeight += 36f;
            if (!settings.randomisePawnName)
            {
                Rect originalRect = listingStandard.GetRect(32f);
                float segmentWidtht = originalRect.width / 3;
                Rect fRect = new Rect(originalRect.x, originalRect.y, segmentWidtht, originalRect.height);
                Rect nRect = new Rect(originalRect.x + segmentWidtht, originalRect.y, segmentWidtht, originalRect.height);
                Rect lRect = new Rect(originalRect.x + segmentWidtht * 2, originalRect.y, segmentWidtht, originalRect.height);
                settings.PawnFirstName = Widgets.TextField(fRect, settings.PawnFirstName);
                settings.PawnNickName = Widgets.TextField(nRect, settings.PawnNickName);
                settings.PawnLastName = Widgets.TextField(lRect, settings.PawnLastName);
                planetListingHeight += 32f;
            }

            listingStandard.Gap();
            planetListingHeight += 36f;
            DoSettingToggle(listingStandard.GetRect(24f), "Stat_Age_Label".Translate(), null, ref settings.randomisePawnAge);
            planetListingHeight += 36f + Text.LineHeight;
            if (!settings.randomisePawnAge)
            {
                Rect rect9 = listingStandard.GetRect(32f);
                Widgets.IntRange(rect9, 1623498651, ref settings.randomisePawnAgeRange, 0, 100, Util.GetAgeRangeLabelPercent(settings.randomisePawnAgeRange));
                planetListingHeight += 32f;
            }

            listingStandard.Gap();
            planetListingHeight += 36f + Text.LineHeight;
            DoSettingToggle(listingStandard.GetRect(24f), "Sex".Translate(), null, ref settings.randomisePawnSex);
            planetListingHeight += 44f;
            if (!settings.randomisePawnSex)
            {
                List<string> list = Enum.GetNames(typeof(Gender)).ToList();
                int num = settings.PawnSex;
                if (num >= list.Count)
                {
                    num = 0;
                    settings.PawnSex = 0;
                }
                if (listingStandard.ButtonText(list[num]))
                {
                    List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        int sex = i;
                        FloatMenuOption item = new FloatMenuOption(list[i], delegate
                        {
                            settings.PawnSex = sex;
                        });
                        list2.Add(item);
                    }
                    Find.WindowStack.Add(new FloatMenu(list2));
                }
                planetListingHeight += 32f;
            }

            planetListingHeight += 36f + Text.LineHeight;
            DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.PawnNotDisabledWorkTags".Translate(), null, ref settings.PawnNotDisabledWorkTags);
            planetListingHeight += 32f;

            listingStandard.Gap();
            planetListingHeight += 12f;
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetStartingColonists();
            }
            planetListingHeight += 32f;
            listingStandard.End();
            Widgets.EndScrollView();
        }

        private void DrawCustomLeft(Listing_Standard listing)
        {
            Listing_Standard listing_Standard = DrawCustomSectionStart(listing, sectionHeightThreats, "DifficultyThreatSection".Translate());
            DrawCustomDifficultySlider(listing_Standard, "threatScale", ref settings.threatScale, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySetting(listing_Standard, "allowBigThreats", ref settings.allowBigThreats);
            DrawCustomDifficultySetting(listing_Standard, "allowViolentQuests", ref settings.allowViolentQuests);
            DrawCustomDifficultySetting(listing_Standard, "allowIntroThreats", ref settings.allowIntroThreats);
            DrawCustomDifficultySetting(listing_Standard, "predatorsHuntHumanlikes", ref settings.predatorsHuntHumanlikes);
            DrawCustomDifficultySetting(listing_Standard, "allowExtremeWeatherIncidents", ref settings.allowExtremeWeatherIncidents);
            if (ModsConfig.BiotechActive)
            {
                DrawCustomDifficultySlider(listing_Standard, "wastepackInfestationChanceFactor", ref settings.wastepackInfestationChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            }
            DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightThreats);
            listing_Standard = DrawCustomSectionStart(listing, sectionHeightEconomy, "DifficultyEconomySection".Translate());
            DrawCustomDifficultySlider(listing_Standard, "cropYieldFactor", ref settings.cropYieldFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "mineYieldFactor", ref settings.mineYieldFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "butcherYieldFactor", ref settings.butcherYieldFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "researchSpeedFactor", ref settings.researchSpeedFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "questRewardValueFactor", ref settings.questRewardValueFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "raidLootPointsFactor", ref settings.raidLootPointsFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "tradePriceFactorLoss", ref settings.tradePriceFactorLoss, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 0.5f);
            DrawCustomDifficultySlider(listing_Standard, "maintenanceCostFactor", ref settings.maintenanceCostFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0.01f, 1f);
            DrawCustomDifficultySlider(listing_Standard, "scariaRotChance", ref settings.scariaRotChance, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
            DrawCustomDifficultySlider(listing_Standard, "enemyDeathOnDownedChanceFactor", ref settings.enemyDeathOnDownedChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
            DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightEconomy);
            if (ModsConfig.IdeologyActive)
            {
                listing_Standard = DrawCustomSectionStart(listing, sectionHeightIdeology, "DifficultyIdeologySection".Translate());
                DrawCustomDifficultySlider(listing_Standard, "lowPopConversionBoost", ref settings.lowPopConversionBoost, ToStringStyle.Integer, ToStringNumberSense.Factor, 1f, 5f, 1f);
                DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightIdeology);
            }
            if (!ModsConfig.BiotechActive)
            {
                return;
            }
            listing_Standard = DrawCustomSectionStart(listing, sectionHeightChildren, "DifficultyChildrenSection".Translate());
            DrawCustomDifficultySetting(listing_Standard, "noBabiesOrChildren", ref settings.noBabiesOrChildren);

            DrawCustomDifficultySetting(listing_Standard, "babiesAreHealthy", ref settings.babiesAreHealthy);
            if (!settings.noBabiesOrChildren)
            {
                DrawCustomDifficultySetting(listing_Standard, "childRaidersAllowed", ref settings.childRaidersAllowed);
                if (ModsConfig.AnomalyActive)
                {
                    DrawCustomDifficultySetting(listing_Standard, "childShamblersAllowed", ref settings.childShamblersAllowed);
                }
            }
            else
            {
                DrawDisabledCustomDifficultySetting(listing_Standard, "childRaidersAllowed", "BabiesAreHealthyDisableReason".Translate());
                if (ModsConfig.AnomalyActive)
                {
                    DrawDisabledCustomDifficultySetting(listing_Standard, "childShamblersAllowed", "BabiesAreHealthyDisableReason".Translate());
                }
            }
            DrawCustomDifficultySlider(listing_Standard, "childAgingRate", ref settings.childAgingRate, ToStringStyle.Integer, ToStringNumberSense.Factor, 1f, 6f, 1f);
            DrawCustomDifficultySlider(listing_Standard, "adultAgingRate", ref settings.adultAgingRate, ToStringStyle.Integer, ToStringNumberSense.Factor, 1f, 6f, 1f);
            DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightChildren);
        }

        private void DrawCustomRight(Listing_Standard listing)
        {
            Listing_Standard listing_Standard = DrawCustomSectionStart(listing, sectionHeightGeneral, "DifficultyGeneralSection".Translate());
            DrawCustomDifficultySlider(listing_Standard, "colonistMoodOffset", ref settings.colonistMoodOffset, ToStringStyle.Integer, ToStringNumberSense.Offset, -20f, 20f, 1f);
            DrawCustomDifficultySlider(listing_Standard, "foodPoisonChanceFactor", ref settings.foodPoisonChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "manhunterChanceOnDamageFactor", ref settings.manhunterChanceOnDamageFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "playerPawnInfectionChanceFactor", ref settings.playerPawnInfectionChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "diseaseIntervalFactor", ref settings.diseaseIntervalFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f, 0.01f, reciprocate: true, 100f);
            DrawCustomDifficultySlider(listing_Standard, "enemyReproductionRateFactor", ref settings.enemyReproductionRateFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "deepDrillInfestationChanceFactor", ref settings.deepDrillInfestationChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultySlider(listing_Standard, "friendlyFireChanceFactor", ref settings.friendlyFireChanceFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
            DrawCustomDifficultySlider(listing_Standard, "allowInstantKillChance", ref settings.allowInstantKillChance, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
            DrawCustomDifficultySetting(listing_Standard, "peacefulTemples", ref settings.peacefulTemples, invert: true);
            DrawCustomDifficultySetting(listing_Standard, "allowCaveHives", ref settings.allowCaveHives);
            DrawCustomDifficultySetting(listing_Standard, "unwaveringPrisoners", ref settings.unwaveringPrisoners);
            DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightGeneral);
            listing_Standard = DrawCustomSectionStart(listing, sectionHeightPlayerTools, "DifficultyPlayerToolsSection".Translate());
            DrawCustomDifficultySetting(listing_Standard, "allowTraps", ref settings.allowTraps);
            DrawCustomDifficultySetting(listing_Standard, "allowTurrets", ref settings.allowTurrets);
            DrawCustomDifficultySetting(listing_Standard, "allowMortars", ref settings.allowMortars);
            DrawCustomDifficultySetting(listing_Standard, "classicMortars", ref settings.classicMortars);
            DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightPlayerTools);
            listing_Standard = DrawCustomSectionStart(listing, sectionHeightAdaptation, "DifficultyAdaptationSection".Translate());
            DrawCustomDifficultySlider(listing_Standard, "adaptationGrowthRateFactorOverZero", ref settings.adaptationGrowthRateFactorOverZero, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
            DrawCustomDifficultySlider(listing_Standard, "adaptationEffectFactor", ref settings.adaptationEffectFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
            DrawCustomDifficultySetting(listing_Standard, "fixedWealthMode", ref settings.fixedWealthMode);
            GUI.enabled = settings.fixedWealthMode;
            float value = Mathf.Round(12f / settings.fixedWealthTimeFactor);
            DrawCustomDifficultySlider(listing_Standard, "fixedWealthTimeFactor", ref value, ToStringStyle.Integer, ToStringNumberSense.Absolute, 1f, 20f, 1f);
            settings.fixedWealthTimeFactor = 12f / value;
            GUI.enabled = true;
            DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightAdaptation);
        }

        private static Listing_Standard DrawCustomSectionStart(Listing_Standard listing, float height, string label, string tooltip = null)
        {
            listing.Gap();
            TaggedString transLabel = label.Translate();
            TaggedString transTooltip = tooltip.Translate();
            listing.Label(transLabel, -1f, transTooltip);
            Listing_Standard listing_Standard = listing.BeginSection(height, 8f, 6f);
            listing_Standard.maxOneColumn = true;
            return listing_Standard;
        }

        private static void DrawCustomSectionEnd(Listing_Standard listing, Listing_Standard section, out float height)
        {
            listing.EndSection(section);

            height = section.CurHeight;
        }

        private void MakeResetDifficultyFloatMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (DifficultyDef src in DefDatabase<DifficultyDef>.AllDefs)
            {
                if (!src.isCustom)
                {
                    list.Add(new FloatMenuOption(src.LabelCap, delegate
                    {
                        settings.threatScale = src.threatScale;
                        settings.allowBigThreats = src.allowBigThreats;
                        settings.allowIntroThreats = src.allowIntroThreats;
                        settings.allowCaveHives = src.allowCaveHives;
                        settings.peacefulTemples = src.peacefulTemples;
                        settings.allowViolentQuests = src.allowViolentQuests;
                        settings.predatorsHuntHumanlikes = src.predatorsHuntHumanlikes;
                        settings.scariaRotChance = src.scariaRotChance;
                        settings.colonistMoodOffset = src.colonistMoodOffset;
                        settings.tradePriceFactorLoss = src.tradePriceFactorLoss;
                        settings.cropYieldFactor = src.cropYieldFactor;
                        settings.mineYieldFactor = src.mineYieldFactor;
                        settings.butcherYieldFactor = src.butcherYieldFactor;
                        settings.researchSpeedFactor = src.researchSpeedFactor;
                        settings.diseaseIntervalFactor = src.diseaseIntervalFactor;
                        settings.enemyReproductionRateFactor = src.enemyReproductionRateFactor;
                        settings.playerPawnInfectionChanceFactor = src.playerPawnInfectionChanceFactor;
                        settings.manhunterChanceOnDamageFactor = src.manhunterChanceOnDamageFactor;
                        settings.deepDrillInfestationChanceFactor = src.deepDrillInfestationChanceFactor;
                        settings.wastepackInfestationChanceFactor = src.wastepackInfestationChanceFactor;
                        settings.foodPoisonChanceFactor = src.foodPoisonChanceFactor;
                        settings.maintenanceCostFactor = src.maintenanceCostFactor;
                        settings.enemyDeathOnDownedChanceFactor = src.enemyDeathOnDownedChanceFactor;
                        settings.adaptationGrowthRateFactorOverZero = src.adaptationGrowthRateFactorOverZero;
                        settings.adaptationEffectFactor = src.adaptationEffectFactor;
                        settings.questRewardValueFactor = src.questRewardValueFactor;
                        settings.raidLootPointsFactor = src.raidLootPointsFactor;
                        settings.allowTraps = src.allowTraps;
                        settings.allowTurrets = src.allowTurrets;
                        settings.allowMortars = src.allowMortars;
                        settings.classicMortars = src.classicMortars;
                        settings.allowExtremeWeatherIncidents = src.allowExtremeWeatherIncidents;
                        settings.fixedWealthMode = src.fixedWealthMode;
                        settings.fixedWealthTimeFactor = 1f;
                        settings.friendlyFireChanceFactor = 0.4f;
                        settings.allowInstantKillChance = 1f;
                        settings.lowPopConversionBoost = src.lowPopConversionBoost;
                        settings.minThreatPointsRangeCeiling = src.minThreatPointsRangeCeiling;
                        settings.babiesAreHealthy = src.babiesAreHealthy;
                        settings.noBabiesOrChildren = src.noBabiesOrChildren;
                        settings.childAgingRate = src.childAgingRate;
                        settings.adultAgingRate = src.adultAgingRate;
                        settings.unwaveringPrisoners = src.unwaveringPrisoners;
                        settings.childRaidersAllowed = src.childRaidersAllowed;
                        settings.anomalyThreatsInactiveFraction = src.anomalyThreatsInactiveFraction;
                        settings.anomalyThreatsActiveFraction = src.anomalyThreatsActiveFraction;
                        settings.studyEfficiencyFactor = src.studyEfficiencyFactor;
                    }));
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private static void DrawCustomDifficultySlider(Listing_Standard listing, string optionName, ref float value, ToStringStyle style, ToStringNumberSense numberSense, float min, float max, float precision = 0.01f, bool reciprocate = false, float reciprocalCutoff = 1000f)
        {
            string text = (reciprocate ? "_Inverted" : "");
            string text2 = optionName.CapitalizeFirst();
            string key = "Difficulty_" + text2 + text + "_Label";
            string key2 = "Difficulty_" + text2 + text + "_Info";
            float num = value;
            if (reciprocate)
            {
                num = Reciprocal(num, reciprocalCutoff);
            }
            TaggedString label = key.Translate() + ": " + num.ToStringByStyle(style, numberSense);
            listing.Label(label, -1f, key2.Translate());
            float num2 = listing.Slider(num, min, max);
            if (num2 != num)
            {
                num = GenMath.RoundTo(num2, precision);
            }
            if (reciprocate)
            {
                num = Reciprocal(num, reciprocalCutoff);
            }
            value = num;
        }

        private static void DrawCustomDifficultySetting(Listing_Standard listing, string optionName, ref bool value, bool invert = false, bool showTooltip = true)
        {
            string text = (invert ? "_Inverted" : "");
            string text2 = optionName.CapitalizeFirst();
            string key = "Difficulty_" + text2 + text + "_Label";
            string key2 = "Difficulty_" + text2 + text + "_Info";
            bool checkOn = (invert ? (!value) : value);
            listing.CheckboxLabeled(key.Translate(), ref checkOn, showTooltip ? key2.Translate() : ((TaggedString)null));
            value = (invert ? (!checkOn) : checkOn);
        }

        private static void DrawDisabledCustomDifficultySetting(Listing_Standard listing, string optionName, TaggedString disableReason)
        {
            string text = optionName.CapitalizeFirst();
            string key = "Difficulty_" + text + "_Label";
            string key2 = "Difficulty_" + text + "_Info";
            Color color = GUI.color;
            GUI.color = ColoredText.SubtleGrayColor;
            listing.Label(key.Translate(), -1f, (key2.Translate() + "\n\n" + disableReason.Colorize(ColoredText.WarningColor)).ToString());
            GUI.color = color;
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
                factionDefNames.RemoveAt(index);
                result = true;
            }
            Widgets.EndGroup();
            ModContentPack mod = factionDef.modContentPack;
            if (Mouse.IsOver(rect2))
            {
                TooltipHandler.TipRegion(rect2, factionDef.LabelCap.AsTipTitle() + "\n" + factionDef.Description + "\n" + "\n" + GetSourceModMetaData(factionDef).Name.AsTipTitle());
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

        public bool DoBiomeRow(Rect rect, BiomeDef biomeDef, List<string> biomeDefNames, int index)
        {
            bool result = false;
            Rect rect2 = new Rect(rect.x, rect.y - 4f, rect.width, rect.height + 8f);
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect2);
            }
            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);

            // Draw biome icon if available, otherwise use a default texture
            Texture2D biomeIcon = BaseContent.BadTex; // Simple fallback for now
            if (!biomeDef.texture.NullOrEmpty())
            {
                biomeIcon = ContentFinder<Texture2D>.Get(biomeDef.texture, false) ?? BaseContent.BadTex;
            }
            GUI.color = Color.white;
            widgetRow.Icon(biomeIcon);
            widgetRow.Gap(4f);

            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(biomeDef.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                biomeDefNames.RemoveAt(index);
                result = true;
            }
            Widgets.EndGroup();

            if (Mouse.IsOver(rect2))
            {
                string tooltip = biomeDef.LabelCap.AsTipTitle() + "\n" + biomeDef.description;
                if (biomeDef.modContentPack != null)
                {
                    tooltip += "\n\n" + GetSourceModMetaData(biomeDef).Name.AsTipTitle();
                }
                TooltipHandler.TipRegion(rect2, tooltip);
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

        public bool DoHillinessRow(Rect rect, int hillinessValue, List<int> hillinessValues, int index)
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
            widgetRow.Icon(BaseContent.BadTex); // Simple icon for hilliness
            widgetRow.Gap(4f);

            // Get the translated hilliness name
            string hillinessName = "";
            switch (hillinessValue)
            {
                case 1: hillinessName = "Hilliness_Flat".Translate(); break;
                case 2: hillinessName = "Hilliness_SmallHills".Translate(); break;
                case 3: hillinessName = "Hilliness_LargeHills".Translate(); break;
                case 4: hillinessName = "Hilliness_Mountainous".Translate(); break;
                default: hillinessName = "Unknown"; break;
            }

            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(hillinessName);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                hillinessValues.RemoveAt(index);
                result = true;
            }
            Widgets.EndGroup();

            if (Mouse.IsOver(rect2))
            {
                string tooltip = hillinessName.AsTipTitle();
                TooltipHandler.TipRegion(rect2, tooltip);
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

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
            return def.modContentPack.ModMetaData;
        }

        public Texture2D GetSourceIcon(Def def)
        {
            ModContentPack modContentPack = def.modContentPack;
            if (modContentPack == null)
            {
                return ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Save");
            }
            ModMetaData modMetaData = modContentPack.ModMetaData;
            if (modContentPack.IsOfficialMod)
            {
                return modMetaData.Expansion.Icon;
            }
            else
                return modMetaData.Icon;
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

        public override string SettingsCategory()
        {
            return "RandomStartMod.Title".Translate();
        }

        public bool CheckFirstTimeDialog()
        {
            if (!settings.openedSettings)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("RandomStartMod.LaunchingWithoutConfiguring".Translate("<b>" + "RandomStartMod.Title".Translate() + "</b>"));
                sb.AppendInNewLine(" ");
                sb.AppendInNewLine("AreYouSure".Translate());
                sb.AppendInNewLine(" ");
                sb.AppendInNewLine(" ");
                sb.AppendInNewLine("RandomStartMod.SettingsAccess".Translate());
                Find.WindowStack.Add(
                    new Dialog_MessageBox(
                        sb.ToString(),
                        "ModSettings".Translate(),
                        () => Find.WindowStack.Add(new Dialog_ModSettings(this)),
                        "Start".Translate(),
                        () => LongEventHandler.QueueLongEvent(delegate
                            {
                                RandomScenario.SetupForRandomPlay();
                            }, "GeneratingMap", doAsynchronously: false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap),
                        "RandomStartMod.Title".Translate()
                        )
                    );
                settings.openedSettings = true;
                WriteSettings();
                return false;
            }
            return true;
        }

        private void CleanUpExcludedBiomes()
        {
            if (settings.allowedBiomes == null)
                return;

            // Remove any excluded biomes from the settings
            string[] excludedBiomes = { "Underground", "Labyrinth", "MetalHell", "Undercave" };

            List<string> biomesToRemove = new List<string>();
            foreach (string biomeName in settings.allowedBiomes)
            {
                if (excludedBiomes.Contains(biomeName))
                {
                    biomesToRemove.Add(biomeName);
                }
            }

            foreach (string biomeToRemove in biomesToRemove)
            {
                settings.allowedBiomes.Remove(biomeToRemove);
            }
        }

        private void CleanUpInvalidHilliness()
        {
            if (settings.allowedHilliness == null)
                return;

            // Remove any invalid hilliness values (only allow 1-4: Flat, SmallHills, LargeHills, Mountainous)
            List<int> validHilliness = new List<int> { 1, 2, 3, 4 };

            List<int> hillinessToRemove = new List<int>();
            foreach (int hillinessValue in settings.allowedHilliness)
            {
                if (!validHilliness.Contains(hillinessValue))
                {
                    hillinessToRemove.Add(hillinessValue);
                }
            }

            foreach (int hillinessToRemove_item in hillinessToRemove)
            {
                settings.allowedHilliness.Remove(hillinessToRemove_item);
            }
        }

        AcceptanceReport CanAddBiome(BiomeDef b)
        {
            if (settings.allowedBiomes.Contains(b.defName))
            {
                return "Already added";
            }

            // Manually filter out underground biomes that don't appear on planet surface
            string[] excludedBiomes = { "Underground", "Labyrinth", "MetalHell", "Undercave" };
            if (excludedBiomes.Contains(b.defName))
            {
                return "Not available on planet surface";
            }

            return AcceptanceReport.WasAccepted;
        }

        AcceptanceReport CanAddHilliness(int hillinessValue)
        {
            if (settings.allowedHilliness.Contains(hillinessValue))
            {
                return "Already added";
            }

            return AcceptanceReport.WasAccepted;
        }

        AcceptanceReport CanAddForcedMeme(MemeDef memeDef)
        {
            if (settings.forcedMemes.Contains(memeDef.defName))
            {
                return "Already added";
            }

            // Check if this is a Structure meme and if there's already one in the list
            if (memeDef.category == MemeCategory.Structure)
            {
                foreach (string existingMemeDefName in settings.forcedMemes)
                {
                    MemeDef existingMeme = DefDatabase<MemeDef>.GetNamed(existingMemeDefName, false);
                    if (existingMeme != null && existingMeme.category == MemeCategory.Structure)
                    {
                        return "Only one structure meme allowed";
                    }
                }
            }

            return AcceptanceReport.WasAccepted;
        }



        public bool DoMemeRow(Rect rect, MemeDef memeDef, List<string> memeDefNames, int index)
        {
            bool result = false;
            Rect rect2 = new Rect(rect.x, rect.y - 4f, rect.width, rect.height + 8f);
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect2);
            }
            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);

            // Get icon from meme itself or source mod
            Texture2D icon = memeDef.Icon ?? GetSourceIcon(memeDef);
            GUI.color = Color.white;
            widgetRow.Icon(icon);
            GUI.color = Color.white;
            widgetRow.Gap(4f);
            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(memeDef.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                memeDefNames.RemoveAt(index);
                result = true;
            }
            Widgets.EndGroup();

            if (Mouse.IsOver(rect2))
            {
                string tooltip = memeDef.LabelCap.AsTipTitle() + "\n" + memeDef.description;
                if (memeDef.modContentPack != null)
                {
                    tooltip += "\n\n" + GetSourceModMetaData(memeDef).Name.AsTipTitle();
                }
                TooltipHandler.TipRegion(rect2, tooltip);
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

        public bool DoPreceptRow(Rect rect, PreceptDef preceptDef, List<string> preceptDefNames, int index)
        {
            bool result = false;
            string text = IdeoUIUtility.GetPreceptLabel(preceptDef, preceptDef.ritualPatternBase);
            bool flag = IdeoUIUtility.IsBasicPrecept(preceptDef);
            if (flag)
            {
                text = preceptDef.issue.LabelCap + ": " + text;
                if (preceptDef.issue == null || text == "")
                {
                    return false;
                }
            }
            Rect rect2 = new Rect(rect.x, rect.y - 4f, rect.width, rect.height + 8f);
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect2);
            }
            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);

            // Get icon from precept itself or source mod
            Texture2D icon = preceptDef.Icon ?? GetSourceIcon(preceptDef);
            GUI.color = Color.white;
            widgetRow.Icon(icon);
            GUI.color = Color.white;
            widgetRow.Gap(4f);

            Text.Anchor = TextAnchor.MiddleCenter;

            widgetRow.Label(text);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                preceptDefNames.RemoveAt(index);
                result = true;
            }
            Widgets.EndGroup();

            if (Mouse.IsOver(rect2))
            {
                string tooltip = text.AsTipTitle() + "\n" + preceptDef.description;
                if (preceptDef.modContentPack != null)
                {
                    tooltip += "\n\n" + GetSourceModMetaData(preceptDef).Name.AsTipTitle();
                }
                TooltipHandler.TipRegion(rect2, tooltip);
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }
    }
}