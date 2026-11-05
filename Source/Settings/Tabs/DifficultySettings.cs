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

    }
}
