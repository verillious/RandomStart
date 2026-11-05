using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RandomStartMod.Compat;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RandomStartMod
{
    public static partial class RandomScenario
    {

        private static void SetupRandomDifficulty(RandomStartSettings settings)
        {
            DifficultyDef chosenDifficultyDef = DefDatabase<DifficultyDef>.GetNamedSilentFail(settings.difficulty)
                ?? DefDatabase<DifficultyDef>.GetNamedSilentFail("Rough")
                ?? DefDatabase<DifficultyDef>.AllDefs.First();
            StorytellerDef chosenStoryteller = StorytellerDefOf.Cassandra;
            List<StorytellerDef> possibleStorytellers = new List<StorytellerDef>();

            foreach (StorytellerDef item in DefDatabase<StorytellerDef>.AllDefs)
            {
                if (!settings.disabledStorytellers.Contains(item.defName) && item.listVisible)
                {
                    possibleStorytellers.Add(item);
                }
            }

            if (possibleStorytellers.Count > 0)
            {
                int storytellerIndex = Rand.Range(0, possibleStorytellers.Count);
                chosenStoryteller = possibleStorytellers[storytellerIndex];
            }
            else
            {
                settings.disabledStorytellers.Remove(chosenStoryteller.defName);
            }


            if (
                ModsConfig.IsActive("brrainz.nopausechallenge")
                || ModsConfig.IsActive("brrainz.nopausechallenge_steam")
            )
            {
                Compat.NoPauseCompat.SetupForNoPause();
            }

            Current.Game.storyteller = new Storyteller(chosenStoryteller, chosenDifficultyDef);

            if (chosenDifficultyDef.isCustom)
            {
                Difficulty difficulty = Current.Game.storyteller.difficulty;
                difficulty.threatScale = settings.threatScale;
                difficulty.allowBigThreats = settings.allowBigThreats;
                difficulty.allowIntroThreats = settings.allowIntroThreats;
                difficulty.allowCaveHives = settings.allowCaveHives;
                difficulty.peacefulTemples = settings.peacefulTemples;
                difficulty.allowViolentQuests = settings.allowViolentQuests;
                difficulty.predatorsHuntHumanlikes = settings.predatorsHuntHumanlikes;
                difficulty.scariaRotChance = settings.scariaRotChance;
                difficulty.colonistMoodOffset = settings.colonistMoodOffset;
                difficulty.tradePriceFactorLoss = settings.tradePriceFactorLoss;
                difficulty.cropYieldFactor = settings.cropYieldFactor;
                difficulty.mineYieldFactor = settings.mineYieldFactor;
                difficulty.butcherYieldFactor = settings.butcherYieldFactor;
                difficulty.researchSpeedFactor = settings.researchSpeedFactor;
                difficulty.diseaseIntervalFactor = settings.diseaseIntervalFactor;
                difficulty.enemyReproductionRateFactor =
                    settings.enemyReproductionRateFactor;
                difficulty.playerPawnInfectionChanceFactor =
                    settings.playerPawnInfectionChanceFactor;
                difficulty.manhunterChanceOnDamageFactor =
                    settings.manhunterChanceOnDamageFactor;
                difficulty.deepDrillInfestationChanceFactor =
                    settings.deepDrillInfestationChanceFactor;
                difficulty.wastepackInfestationChanceFactor =
                    settings.wastepackInfestationChanceFactor;
                difficulty.foodPoisonChanceFactor = settings.foodPoisonChanceFactor;
                difficulty.maintenanceCostFactor = settings.maintenanceCostFactor;
                difficulty.enemyDeathOnDownedChanceFactor =
                    settings.enemyDeathOnDownedChanceFactor;
                difficulty.adaptationGrowthRateFactorOverZero =
                    settings.adaptationGrowthRateFactorOverZero;
                difficulty.adaptationEffectFactor = settings.adaptationEffectFactor;
                difficulty.questRewardValueFactor = settings.questRewardValueFactor;
                difficulty.raidLootPointsFactor = settings.raidLootPointsFactor;
                difficulty.allowTraps = settings.allowTraps;
                difficulty.allowTurrets = settings.allowTurrets;
                difficulty.allowMortars = settings.allowMortars;
                difficulty.classicMortars = settings.classicMortars;
                difficulty.allowExtremeWeatherIncidents =
                    settings.allowExtremeWeatherIncidents;
                difficulty.fixedWealthMode = settings.fixedWealthMode;
                difficulty.lowPopConversionBoost = settings.lowPopConversionBoost;
                difficulty.minThreatPointsRangeCeiling =
                    settings.minThreatPointsRangeCeiling;
                difficulty.babiesAreHealthy = settings.babiesAreHealthy;
                difficulty.noBabiesOrChildren = settings.noBabiesOrChildren;
                difficulty.childAgingRate = settings.childAgingRate;
                difficulty.adultAgingRate = settings.adultAgingRate;
                difficulty.unwaveringPrisoners = settings.unwaveringPrisoners;
                difficulty.childRaidersAllowed = settings.childRaidersAllowed;
                difficulty.anomalyThreatsInactiveFraction =
                    settings.anomalyThreatsInactiveFraction;
                difficulty.anomalyThreatsActiveFraction =
                    settings.anomalyThreatsActiveFraction;
                difficulty.studyEfficiencyFactor = settings.studyEfficiencyFactor;
            }



            if (chosenDifficultyDef.isCustom)
            {
                Difficulty difficulty = Current.Game.storyteller.difficulty;
                difficulty.fixedWealthTimeFactor = settings.fixedWealthTimeFactor;
                difficulty.friendlyFireChanceFactor = settings.friendlyFireChanceFactor;
                difficulty.allowInstantKillChance = settings.allowInstantKillChance;
            }

            if (ModsConfig.AnomalyActive)
            {
                Difficulty difficulty = Current.Game.storyteller.difficulty;
                AnomalyPlaystyleDef chosenAnomalyPlaystyleDef =
                    DefDatabase<AnomalyPlaystyleDef>.GetNamed(settings.anomalyPlaystyle);
                if (chosenAnomalyPlaystyleDef.overrideThreatFraction)
                {
                    difficulty.overrideAnomalyThreatsFraction =
                        settings.overrideAnomalyThreatsFraction;
                }
                else
                {
                    difficulty.overrideAnomalyThreatsFraction = null;
                }
                difficulty.anomalyThreatsInactiveFraction = settings.anomalyThreatsInactiveFraction;
                difficulty.anomalyThreatsActiveFraction = settings.anomalyThreatsActiveFraction;
                difficulty.studyEfficiencyFactor = settings.studyEfficiencyFactor;
                difficulty.AnomalyPlaystyleDef = chosenAnomalyPlaystyleDef;
            }
            Find.GameInitData.permadeath = settings.permadeath;
            Util.LogMessage($"Using {chosenStoryteller.LabelCap}, {chosenDifficultyDef.LabelCap}");
        }

    }
}
