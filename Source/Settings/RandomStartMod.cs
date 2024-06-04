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

        private static float mainListingHeight;
        private static float factionListingHeight;
        private static float planetListingHeight;
        private static float storytellerListingHeight;
        private static float scenarioListingHeight;
        private static float optionalFeaturesListingHeight;

        private static float sectionHeightThreats = 0f;
        private static float sectionHeightGeneral = 0f;
        private static float sectionHeightPlayerTools = 0f;
        private static float sectionHeightEconomy = 0f;
        private static float sectionHeightAdaptation = 0f;
        private static float sectionHeightIdeology = 0f;
        private static float sectionHeightChildren = 0f;


        public int currentTab = 0;
        public RandomStartMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<RandomStartSettings>();
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
                new TabRecord("Factions".Translate(), () =>
                {
                    currentTab = 1;
                    WriteSettings();
                }, currentTab == 1),
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
            if (ModsConfig.IsActive("brrainz.nopausechallenge"))
            {
                listingStandard.Gap(3f);
                listingStandard.CheckboxLabeled("No Pause Challenge", ref settings.noPauseEnabled);
                listingStandard.CheckboxLabeled("Half Speed enabled", ref settings.noPauseHalfSpeedEnabled);
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
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("ScenPart_StartWithPawns_OutOf".Translate().CapitalizeFirst());
            listingStandard.GapLine();
            scenarioListingHeight += 24f + Text.LineHeight;
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
            listingStandard.Label("ScenPart_StartWithPawns_OutOf".Translate().CapitalizeFirst());
            listingStandard.GapLine();
            storytellerListingHeight += 24f + Text.LineHeight;
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
            listingStandard.Label("Required".Translate());
            listingStandard.GapLine();
            factionListingHeight += 24f + Text.LineHeight;
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
            foreach (FactionDef configurableFaction in FactionGenerator.ConfigurableFactions)
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
            listingStandard.Label("Random".Translate());
            listingStandard.GapLine();
            factionListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;

            listingStandard.Label("PenFoodTab_Count".Translate() + ":");
            factionListingHeight += Text.LineHeight;
            Widgets.IntRange(listingStandard.GetRect(32f), 1823998654, ref settings.randomFactionRange, 0, 20);
            factionListingHeight += 32f;

            listingStandard.CheckboxLabeled("Unique".Translate().CapitalizeFirst(), ref settings.uniqueFactions);
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
                if (factionsAlwaysAdd.Count((FactionDef x) => x == f) >= f.maxConfigurableAtWorldCreation)
                {
                    return "MaxFactionsForType".Translate(f.maxConfigurableAtWorldCreation).ToString().UncapitalizeFirst();
                }
                return true;
            }
            List<FloatMenuOption> list2 = new List<FloatMenuOption>();
            foreach (FactionDef configurableFaction in FactionGenerator.ConfigurableFactions)
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
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetFactions();
            }
            factionListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();

            AcceptanceReport CanAddRandomFaction(FactionDef f)
            {
                if (factionsAlwaysAdd.Count((FactionDef x) => x == f) >= f.maxConfigurableAtWorldCreation)
                {
                    return "MaxFactionsForType".Translate(f.maxConfigurableAtWorldCreation).ToString().UncapitalizeFirst();
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
            listingStandard.Label("PlanetCoverage".Translate());
            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
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
            listingStandard.Label("MapSize".Translate());
            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
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
            listingStandard.Label("TabPlanet".Translate());
            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            listingStandard.Gap();
            planetListingHeight += 12f;
            listingStandard.Label("Randomize".Translate() + ":");
            listingStandard.Gap();
            planetListingHeight += 12f;
            listingStandard.CheckboxLabeled("PlanetRainfall".Translate(), ref settings.randomiseRainfall, 12f);
            planetListingHeight += 32f + Text.LineHeight;


            if (!settings.randomiseRainfall)
            {
                listingStandard.Indent();
                listingStandard.Gap();
                Rect rainfallRect = listingStandard.GetRect(30f);
                rainfallRect.width -= 12f;
                settings.rainfall = Mathf.RoundToInt(Widgets.HorizontalSlider(rainfallRect, (float)settings.rainfall, 0f, OverallRainfallUtility.EnumValuesCount - 1, middleAlignment: true, "PlanetRainfall_Normal".Translate(), "PlanetRainfall_Low".Translate(), "PlanetRainfall_High".Translate(), 1f));
                planetListingHeight += 12f + 32f;
                listingStandard.Outdent();

            }
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("PlanetTemperature".Translate(), ref settings.randomiseTemperature, 12f);
            planetListingHeight += 12f + 32f;

            if (!settings.randomiseTemperature)
            {
                listingStandard.Indent();
                listingStandard.Gap();
                Rect temperatureRect = listingStandard.GetRect(30f);
                temperatureRect.width -= 12f;
                settings.temperature = Mathf.RoundToInt(Widgets.HorizontalSlider(temperatureRect, (float)settings.temperature, 0f, OverallTemperatureUtility.EnumValuesCount - 1, middleAlignment: true, "PlanetTemperature_Normal".Translate(), "PlanetTemperature_Low".Translate(), "PlanetTemperature_High".Translate(), 1f));
                planetListingHeight += 12f + 32f;
                listingStandard.Outdent();

            }
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("PlanetPopulation".Translate(), ref settings.randomisePopulation, 12f);
            planetListingHeight += 12f + 32f;
            if (!settings.randomisePopulation)
            {
                listingStandard.Indent();
                listingStandard.Gap();
                Rect populationRect = listingStandard.GetRect(30f);
                populationRect.width -= 12f;
                settings.population = Mathf.RoundToInt(Widgets.HorizontalSlider(populationRect, (float)settings.population, 0f, OverallPopulationUtility.EnumValuesCount - 1, middleAlignment: true, "PlanetPopulation_Normal".Translate(), "PlanetPopulation_Low".Translate(), "PlanetPopulation_High".Translate(), 1f));
                planetListingHeight += 12f + 32f;
                listingStandard.Outdent();
            }

            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Pollution_Label".Translate(), ref settings.randomisePollution, 12f);
            planetListingHeight += 12f + 32f;
            if (!settings.randomisePollution)
            {
                listingStandard.Indent();
                listingStandard.Gap();
                Rect pollutionRect = listingStandard.GetRect(30f);
                pollutionRect.width -= 12f;
                settings.pollution = Widgets.HorizontalSlider(pollutionRect, settings.pollution, 0f, 1f, middleAlignment: true, settings.pollution.ToStringPercent(), null, null, 0.05f);
                planetListingHeight += 12f + 32f;
                listingStandard.Outdent();
            }
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("MapStartSeason".Translate(), ref settings.randomiseSeason, 12f);
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

            if (ModsConfig.IsActive("Oblitus.MyLittlePlanet"))
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

            if (ModsConfig.IsActive("zvq.RealisticPlanetsContinued"))
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("Realistic Planets Continued");
                listingStandard.GapLine();
                planetListingHeight += 12f + 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                listingStandard.CheckboxLabeled("Randomize".Translate(), ref settings.randomiseRealisticPlanets);
                planetListingHeight += 32f;
                if (!settings.randomiseRealisticPlanets)
                {
                    Compat.RealisticPlanetsCompat.DoWorldTypeSelectionButton(listingStandard);
                    planetListingHeight += 32f;
                }

            }

            if (ModsConfig.IsActive("Woolstrand.RealRuins"))
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("Real Ruins");
                listingStandard.GapLine();
                planetListingHeight += 12f + 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                listingStandard.CheckboxLabeled("RunInBackground".Translate(), ref settings.enableAutoRealRuins);
                planetListingHeight += 32f;
                listingStandard.CheckboxLabeled("RealRuins.BiomeFiltering".Translate(), ref settings.realRuinsBiomeFilter, "RealRuins.BiomeFilteringTT".Translate());
                planetListingHeight += 32f;
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
            Rect rect = new Rect(0f, 60f, inRect.width, optionalFeaturesListingHeight);
            optionalFeaturesListingHeight = 0f;
            Widgets.BeginScrollView(inRect, ref mainScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            if (ModsConfig.BiotechActive)
            {
                Text.Font = GameFont.Medium;
                listingStandard.Label("Genes".Translate().CapitalizeFirst());
                listingStandard.GapLine();
                optionalFeaturesListingHeight += 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                DoOptionalFeatureRow(listingStandard.GetRect(24f), $"{"Randomize".Translate()}: {"Xenotype".Translate()}", null, ref settings.enableRandomXenotypes);
                optionalFeaturesListingHeight += 24f;
                DoOptionalFeatureRow(listingStandard.GetRect(24f), "RandomStartMod.RespectMemberXenotypes".Translate(), null, ref settings.respectFactionXenotypes);
                optionalFeaturesListingHeight += 24f;
                DoOptionalFeatureRow(listingStandard.GetRect(24f), $"{"Randomize".Translate()}: {"Genes".Translate().CapitalizeFirst()}", null, ref settings.enableRandomCustomXenotypes);
                optionalFeaturesListingHeight += 24f;
                if (settings.enableRandomCustomXenotypes)
                {
                    listingStandard.Label("PenFoodTab_Count".Translate() + ":");
                    optionalFeaturesListingHeight += Text.LineHeight;
                    Widgets.IntRange(listingStandard.GetRect(32f), 382399865, ref settings.randomGeneRange, 0, 20);
                    optionalFeaturesListingHeight += 32f;
                }
            }
            if (ModsConfig.IdeologyActive)
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("DifficultyIdeologySection".Translate());
                listingStandard.GapLine();
                optionalFeaturesListingHeight += 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                DoOptionalFeatureRow(listingStandard.GetRect(24f), "PlayClassic".Translate(), null, ref settings.disableIdeo);
                optionalFeaturesListingHeight += 24f;
                if (!settings.disableIdeo)
                {
                    DoOptionalFeatureRow(listingStandard.GetRect(24f), "CreateFluid".Translate(), "FluidIdeoTip".Translate(), ref settings.fluidIdeo);
                    optionalFeaturesListingHeight += 24f;
                    if (GenFilePaths.AllCustomIdeoFiles.Any())
                    {
                        DoOptionalFeatureRow(listingStandard.GetRect(24f), "LoadExistingIdeoligion".Translate(), null, ref settings.overrideIdeo);
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

        private void DrawCustomLeft(Listing_Standard listing)
        {
            Listing_Standard listing_Standard = DrawCustomSectionStart(listing, sectionHeightThreats, "DifficultyThreatSection".Translate());
            DrawCustomDifficultySlider(listing_Standard, "threatScale", ref settings.threatScale, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 5f);
            DrawCustomDifficultyCheckbox(listing_Standard, "allowBigThreats", ref settings.allowBigThreats);
            DrawCustomDifficultyCheckbox(listing_Standard, "allowViolentQuests", ref settings.allowViolentQuests);
            DrawCustomDifficultyCheckbox(listing_Standard, "allowIntroThreats", ref settings.allowIntroThreats);
            DrawCustomDifficultyCheckbox(listing_Standard, "predatorsHuntHumanlikes", ref settings.predatorsHuntHumanlikes);
            DrawCustomDifficultyCheckbox(listing_Standard, "allowExtremeWeatherIncidents", ref settings.allowExtremeWeatherIncidents);
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
            DrawCustomDifficultyCheckbox(listing_Standard, "noBabiesOrChildren", ref settings.noBabiesOrChildren);
            DrawCustomDifficultyCheckbox(listing_Standard, "babiesAreHealthy", ref settings.babiesAreHealthy);
            if (!settings.noBabiesOrChildren)
            {
                DrawCustomDifficultyCheckbox(listing_Standard, "childRaidersAllowed", ref settings.childRaidersAllowed);
                if (ModsConfig.AnomalyActive)
                {
                    DrawCustomDifficultyCheckbox(listing_Standard, "childShamblersAllowed", ref settings.childShamblersAllowed);
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
            DrawCustomDifficultyCheckbox(listing_Standard, "peacefulTemples", ref settings.peacefulTemples, invert: true);
            DrawCustomDifficultyCheckbox(listing_Standard, "allowCaveHives", ref settings.allowCaveHives);
            DrawCustomDifficultyCheckbox(listing_Standard, "unwaveringPrisoners", ref settings.unwaveringPrisoners);
            DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightGeneral);
            listing_Standard = DrawCustomSectionStart(listing, sectionHeightPlayerTools, "DifficultyPlayerToolsSection".Translate());
            DrawCustomDifficultyCheckbox(listing_Standard, "allowTraps", ref settings.allowTraps);
            DrawCustomDifficultyCheckbox(listing_Standard, "allowTurrets", ref settings.allowTurrets);
            DrawCustomDifficultyCheckbox(listing_Standard, "allowMortars", ref settings.allowMortars);
            DrawCustomDifficultyCheckbox(listing_Standard, "classicMortars", ref settings.classicMortars);
            DrawCustomSectionEnd(listing, listing_Standard, out sectionHeightPlayerTools);
            listing_Standard = DrawCustomSectionStart(listing, sectionHeightAdaptation, "DifficultyAdaptationSection".Translate());
            DrawCustomDifficultySlider(listing_Standard, "adaptationGrowthRateFactorOverZero", ref settings.adaptationGrowthRateFactorOverZero, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
            DrawCustomDifficultySlider(listing_Standard, "adaptationEffectFactor", ref settings.adaptationEffectFactor, ToStringStyle.PercentZero, ToStringNumberSense.Absolute, 0f, 1f);
            DrawCustomDifficultyCheckbox(listing_Standard, "fixedWealthMode", ref settings.fixedWealthMode);
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
            listing.Label(label, -1f, tooltip);
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

        private static void DrawCustomDifficultyCheckbox(Listing_Standard listing, string optionName, ref bool value, bool invert = false, bool showTooltip = true)
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

        public void DoOptionalFeatureRow(Rect rect, string label, string description, ref bool checkOn)
        {

            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);
            widgetRow.Gap(4f);
            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(label);
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.Checkbox(new Vector2(rect.width - 24f - 6f, 0f), ref checkOn);
            Widgets.EndGroup();
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
    }
}