using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace RandomStartMod
{
    public class RandomStartMod : Mod
    {
        RandomStartSettings settings;

        private static readonly int[] MapSizes = new int[6] { 200, 225, 250, 275, 300, 325 };
        private static readonly float[] PlanetCoverages = new float[3] { 0.3f, 0.5f, 1f };

        private static Vector2 mainScrollPosition;
        private static Vector2 factionScrollPosition;
        private static Vector2 planetScrollPosition;

        private static float mainListingHeight;
        private static float factionListingHeight;
        private static float planetListingHeight;

        public int currentTab = 0;
        public RandomStartMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<RandomStartSettings>();
        }


        public override void DoSettingsWindowContents(Rect inRect)
        {
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
                new TabRecord("Factions".Translate(), () =>
                {
                    currentTab = 1;
                    WriteSettings();
                }, currentTab == 1),
                new TabRecord("TabPlanet".Translate(), () =>
                {
                    currentTab = 2;
                    WriteSettings();
                }, currentTab == 2),
            };
            TabDrawer.DrawTabs(tabRect, tabs);

            if (currentTab == 0)
            {
                DoMainSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 1)
            {
                DoFactionSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 2)
            {
                DoPlanetSettingsTabContents(mainRect.ContractedBy(15f));
            }

        }

        public void DoMainSettingsTabContents(Rect inRect)
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
                    continue;
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

            // Storytellers
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("Storyteller".Translate());
            listingStandard.GapLine();
            mainListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;

            Rect leftStorytellerRect = listingStandard.GetRect(30f);
            float width = leftStorytellerRect.width;
            leftStorytellerRect.width /= 2f;
            if (Widgets.ButtonText(leftStorytellerRect, "ClickToAdd".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (settings.disabledStorytellers.NullOrEmpty())
                    settings.disabledStorytellers = new List<string>();

                foreach (StorytellerDef item in DefDatabase<StorytellerDef>.AllDefs.OrderBy((StorytellerDef tel) => tel.listOrder))
                {
                    if (item.listVisible && settings.disabledStorytellers.Contains(item.defName))
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item.LabelCap, () => settings.disabledStorytellers.Remove(item.defName)));
                    }
                }

                if (floatMenuOptions.Count > 0)
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            mainListingHeight += 32f;
            Rect rightStorytellerRect = listingStandard.GetRect(30f);
            rightStorytellerRect.width = leftStorytellerRect.width;
            rightStorytellerRect.x += width - rightStorytellerRect.width;
            rightStorytellerRect.y -= 30f;
            if (Widgets.ButtonText(rightStorytellerRect, "ClickToRemove".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (settings.disabledStorytellers.NullOrEmpty())
                    settings.disabledStorytellers = new List<string>();

                listingStandard.Indent();
                foreach (StorytellerDef item in DefDatabase<StorytellerDef>.AllDefs.OrderBy((StorytellerDef tel) => tel.listOrder))
                {
                    if (item.listVisible && !settings.disabledStorytellers.Contains(item.defName))
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item.LabelCap, () => settings.disabledStorytellers.Add(item.defName)));
                    }
                }
                listingStandard.Outdent();

                if (floatMenuOptions.Count > 0 && floatMenuOptions.Count != 1)
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            mainListingHeight += 32f;
            listingStandard.Label("Enabled".Translate() + ":");
            mainListingHeight += Text.LineHeight;

            listingStandard.Indent();
            Text.Font = GameFont.Tiny;
            foreach (StorytellerDef item in DefDatabase<StorytellerDef>.AllDefs.OrderBy((StorytellerDef tel) => tel.listOrder))
            {
                if (item.listVisible && !settings.disabledStorytellers.Contains(item.defName))
                {
                    listingStandard.Label(item.LabelCap, -1, item.description.ResolveTags());
                    mainListingHeight += Text.LineHeight;
                }
            }
            listingStandard.Outdent();
            Text.Font = GameFont.Small;



            // Scenarios
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("StatsReport_ScenarioFactor".Translate());
            listingStandard.GapLine();
            mainListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;

            Rect leftScenarioRect = listingStandard.GetRect(30f);
            leftScenarioRect.width /= 2f;
            if (Widgets.ButtonText(leftScenarioRect, "ClickToAdd".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (settings.disabledScenarios.NullOrEmpty())
                    settings.disabledScenarios = new List<string>();

                foreach (Scenario item in ScenarioLister.ScenariosInCategory(ScenarioCategory.FromDef))
                {
                    if (settings.disabledScenarios.Contains(item.name) && item.showInUI)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item.name, () => settings.disabledScenarios.Remove(item.name)));
                    }
                }
                if (settings.enableCustomScenarios)
                    foreach (Scenario item in ScenarioLister.ScenariosInCategory(ScenarioCategory.CustomLocal))
                    {
                        if (settings.disabledScenarios.Contains(item.name) && item.showInUI)
                        {
                            floatMenuOptions.Add(new FloatMenuOption(item.name, () => settings.disabledScenarios.Remove(item.name)));
                        }
                    }
                if (settings.enableSteamWorkshopScenarios)
                    foreach (Scenario item in ScenarioLister.ScenariosInCategory(ScenarioCategory.SteamWorkshop))
                    {
                        if (settings.disabledScenarios.Contains(item.name) && item.showInUI)
                        {
                            floatMenuOptions.Add(new FloatMenuOption(item.name, () => settings.disabledScenarios.Remove(item.name)));
                        }
                    }

                if (floatMenuOptions.Count > 0)
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
            Rect rightScenarioRect = listingStandard.GetRect(30f);
            rightScenarioRect.width = leftStorytellerRect.width;
            rightScenarioRect.x += width - rightScenarioRect.width;
            rightScenarioRect.y -= 30f;



            if (Widgets.ButtonText(rightScenarioRect, "ClickToRemove".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (settings.disabledScenarios.NullOrEmpty())
                    settings.disabledScenarios = new List<string>();

                listingStandard.Indent();

                foreach (Scenario item in ScenarioLister.ScenariosInCategory(ScenarioCategory.FromDef))
                {
                    if (!settings.disabledScenarios.Contains(item.name) && item.showInUI)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item.name, () => settings.disabledScenarios.Add(item.name)));
                    }
                }
                if (settings.enableCustomScenarios)
                    foreach (Scenario item in ScenarioLister.ScenariosInCategory(ScenarioCategory.CustomLocal))
                    {
                        if (!settings.disabledScenarios.Contains(item.name) && item.showInUI)
                        {
                            floatMenuOptions.Add(new FloatMenuOption(item.name, () => settings.disabledScenarios.Add(item.name)));
                        }
                    }
                if (settings.enableSteamWorkshopScenarios)
                    foreach (Scenario item in ScenarioLister.ScenariosInCategory(ScenarioCategory.SteamWorkshop))
                    {
                        if (!settings.disabledScenarios.Contains(item.name) && item.showInUI)
                        {
                            floatMenuOptions.Add(new FloatMenuOption(item.name, () => settings.disabledScenarios.Add(item.name)));
                        }
                    }

                listingStandard.Outdent();


                if (floatMenuOptions.Count > 0 && floatMenuOptions.Count != 1)
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }

            mainListingHeight += 32f;
            listingStandard.CheckboxLabeled("ScenariosCustom".Translate(), ref settings.enableCustomScenarios, 0f);
            listingStandard.CheckboxLabeled("ScenariosSteamWorkshop".Translate(), ref settings.enableSteamWorkshopScenarios, 0f);

            listingStandard.Label("Enabled".Translate() + ":");
            mainListingHeight += 64f + Text.LineHeight;
            listingStandard.Indent();
            Text.Font = GameFont.Tiny;
            foreach (Scenario item in ScenarioLister.ScenariosInCategory(ScenarioCategory.FromDef))
            {
                if (!settings.disabledScenarios.Contains(item.name) && item.showInUI)
                {
                    listingStandard.Label(item.name, -1, item.description.ResolveTags());
                    mainListingHeight += Text.LineHeight;
                }
            }
            if (settings.enableCustomScenarios)
                foreach (Scenario item in ScenarioLister.ScenariosInCategory(ScenarioCategory.CustomLocal))
                {
                    if (!settings.disabledScenarios.Contains(item.name) && item.showInUI)
                    {
                        listingStandard.Label(item.name, -1, item.description.ResolveTags());
                        mainListingHeight += Text.LineHeight;
                    }
                }
            if (settings.enableSteamWorkshopScenarios)
                foreach (Scenario item in ScenarioLister.ScenariosInCategory(ScenarioCategory.SteamWorkshop))
                {
                    if (!settings.disabledScenarios.Contains(item.name) && item.showInUI)
                    {
                        listingStandard.Label(item.name, -1, item.description.ResolveTags());
                        mainListingHeight += Text.LineHeight;
                    }
                }

            listingStandard.Outdent();
            Text.Font = GameFont.Small;


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

            listingStandard.CheckboxLabeled("Randomize".Translate(), ref settings.randomiseFactions, 0f);
            factionListingHeight += 32f;

            if(!settings.randomiseFactions)
            {
                listingStandard.End();
                Widgets.EndScrollView();
                return;
            }
 
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.FactionsAlwaysAdd".Translate());
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

            Rect rect5 = listingStandard.GetRect(28f);
            if (Widgets.ButtonText(rect5, "Add".Translate().CapitalizeFirst() + "...") && TutorSystem.AllowAction("ConfiguringWorldFactions"))
            {
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
                Find.WindowStack.Add(new FloatMenu(list));
            }
            factionListingHeight += 28f;
            factionListingHeight += 28f;

            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.FactionsRandomlyAdd".Translate());
            listingStandard.GapLine();
            factionListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;

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

            Rect rect6 = listingStandard.GetRect(28f);
            if (Widgets.ButtonText(rect6, "Add".Translate().CapitalizeFirst() + "...") && TutorSystem.AllowAction("ConfiguringWorldFactions"))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
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
                    list.Add(floatMenuOption);
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            factionListingHeight += 28f;
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
                stringBuilder.AppendLine("Warning".Translate() + ": " + "AncientsDisabledContentWarning".Translate(FactionDefOf.Ancients.label));
            }
            if (!factionsAlwaysAdd.Contains(FactionDefOf.AncientsHostile))
            {
                stringBuilder.AppendLine("Warning".Translate() + ": " + "AncientsHostileDisabledContentWarning".Translate(FactionDefOf.AncientsHostile.label));
            }
            if (!factionsAlwaysAdd.Contains(FactionDefOf.HoraxCult))
            {
                stringBuilder.AppendLine("Warning".Translate() + ": " + "HoraxCultHostileDisabledContentWarning".Translate(FactionDefOf.HoraxCult.label));
            }
            if (!factionsAlwaysAdd.Contains(FactionDefOf.Entities))
            {
                stringBuilder.AppendLine("Warning".Translate() + ": " + "EntitiesHostileDisabledContentWarning".Translate(FactionDefOf.Entities.label));
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
            foreach (int item in enumerable)
            {
                string label = "MapSizeDesc".Translate(item, item * item);
                if (listingStandard.RadioButton(label, settings.mapSize == item))
                {
                    settings.mapSize = item;
                }
                listingStandard.Gap(3f);
                planetListingHeight += 3f + 32f;
            }

            //Generation Settings
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("TabPlanet".Translate());
            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            listingStandard.Label("Randomize".Translate() + ":");
            listingStandard.CheckboxLabeled("PlanetRainfall".Translate(), ref settings.randomiseRainfall, 0f);
            planetListingHeight += 32f + Text.LineHeight;


            if (!settings.randomiseRainfall)
            {
                listingStandard.Gap();
                Rect rainfallRect = listingStandard.GetRect(30f);
                settings.rainfall = Mathf.RoundToInt(Widgets.HorizontalSlider(rainfallRect, (float)settings.rainfall, 0f, OverallRainfallUtility.EnumValuesCount - 1, middleAlignment: true, "PlanetRainfall_Normal".Translate(), "PlanetRainfall_Low".Translate(), "PlanetRainfall_High".Translate(), 1f));
                planetListingHeight += 12f + 32f;

            }
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("PlanetTemperature".Translate(), ref settings.randomiseTemperature, 0f);
            planetListingHeight += 12f + 32f;

            if (!settings.randomiseTemperature)
            {
                listingStandard.Gap();
                Rect temperatureRect = listingStandard.GetRect(30f);
                settings.temperature = Mathf.RoundToInt(Widgets.HorizontalSlider(temperatureRect, (float)settings.temperature, 0f, OverallTemperatureUtility.EnumValuesCount - 1, middleAlignment: true, "PlanetTemperature_Normal".Translate(), "PlanetTemperature_Low".Translate(), "PlanetTemperature_High".Translate(), 1f));
                planetListingHeight += 12f + 32f;

            }
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("PlanetPopulation".Translate(), ref settings.randomisePopulation, 0f);
            planetListingHeight += 12f + 32f;
            if (!settings.randomisePopulation)
            {
                listingStandard.Gap();
                Rect populationRect = listingStandard.GetRect(30f);
                settings.population = Mathf.RoundToInt(Widgets.HorizontalSlider(populationRect, (float)settings.population, 0f, OverallPopulationUtility.EnumValuesCount - 1, middleAlignment: true, "PlanetPopulation_Normal".Translate(), "PlanetPopulation_Low".Translate(), "PlanetPopulation_High".Translate(), 1f));
                planetListingHeight += 12f + 32f;
            }
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("PlanetPollution".Translate(), ref settings.randomisePollution, 0f);
            planetListingHeight += 12f + 32f;
            if (!settings.randomisePollution)
            {
                listingStandard.Gap();
                Rect pollutionRect = listingStandard.GetRect(30f);
                settings.pollution = Widgets.HorizontalSlider(pollutionRect, settings.pollution, 0f, 1f, middleAlignment: true, settings.pollution.ToStringPercent(), null, null, 0.05f);

                planetListingHeight += 12f + 32f;
            }
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("MapStartSeason".Translate(), ref settings.randomiseSeason, 0f);
            planetListingHeight += 12f + 32f;
            if (!settings.randomiseSeason)
            {
                listingStandard.Gap();
                planetListingHeight += 12f;
                if (listingStandard.RadioButton(Season.Spring.LabelCap(), (Season)settings.startingSeason == Season.Spring))
                {
                    settings.startingSeason = (int)Season.Spring;
                }
                if (listingStandard.RadioButton(Season.Summer.LabelCap(), (Season)settings.startingSeason == Season.Summer))
                {
                    settings.startingSeason = (int)Season.Summer;
                }
                if (listingStandard.RadioButton(Season.Fall.LabelCap(), (Season)settings.startingSeason == Season.Fall))
                {
                    settings.startingSeason = (int)Season.Fall;
                }
                if (listingStandard.RadioButton(Season.Winter.LabelCap(), (Season)settings.startingSeason == Season.Winter))
                {
                    settings.startingSeason = (int)Season.Winter;
                }
                if (listingStandard.RadioButton(Season.PermanentSummer.LabelCap(), (Season)settings.startingSeason == Season.PermanentSummer))
                {
                    settings.startingSeason = (int)Season.PermanentSummer;
                }
                if (listingStandard.RadioButton(Season.PermanentWinter.LabelCap(), (Season)settings.startingSeason == Season.PermanentWinter))
                {
                    settings.startingSeason = (int)Season.PermanentWinter;
                }
                mainListingHeight += 32f * 6;

            }
            listingStandard.End();
            Widgets.EndScrollView();
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
            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete) && TutorSystem.AllowAction("ConfiguringWorldFactions"))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                factionDefNames.RemoveAt(index);
                result = true;
            }
            Widgets.EndGroup();
            if (Mouse.IsOver(rect2))
            {
                TooltipHandler.TipRegion(rect2, factionDef.LabelCap.AsTipTitle() + "\n" + factionDef.Description);
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

        public override string SettingsCategory()
        {
            return "RandomStartMod.Title".Translate();
        }
    }
}