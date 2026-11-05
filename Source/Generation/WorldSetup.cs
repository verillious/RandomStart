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

        private static void SetupRandomPlanet(RandomStartSettings settings)
        {
            WorldSetupPlan plan = ChooseWorldParameters(settings, true);
            if (ModsConfig.IsActive("Oblitus.MyLittlePlanet") || ModsConfig.IsActive("Oblitus.MyLittlePlanet_Steam"))
                MLPCompat.ApplyWorldSize(settings.myLittlePlanetSubcount);

            if (ModsConfig.IsActive("koth.RealisticPlanets1.6") || ModsConfig.IsActive("koth.RealisticPlanets1.6_Steam"))
            {
                int oceanType = settings.randomiseOceanType ? settings.randomiseOceanTypeRange.RandomInRange : settings.realisticPlanetsOceanType;
                int axialTilt = settings.randomiseAxialTilt ? settings.randomiseAxialTiltRange.RandomInRange : settings.realisticPlanetsAxialTilt;
                RealisticPlanetsCompat.GenerateRealisticPlanetWorld(plan.Coverage, plan.Seed,
                    plan.Rainfall, plan.Temperature, plan.Population, plan.LandmarkDensity,
                    plan.Factions, plan.Pollution, oceanType, axialTilt,
                    settings.realisticPlanetsWorldType, settings.randomiseRealisticPlanets);
            }
            else
                Current.Game.World = WorldGenerator.GenerateWorld(plan.Coverage, plan.Seed,
                    plan.Rainfall, plan.Temperature, plan.Population, plan.LandmarkDensity,
                    plan.Factions, plan.Pollution);
        }

        internal static WorldSetupPlan ChooseWorldParameters(RandomStartSettings settings, bool initializeOptionalMods)
        {
            OverallRainfall rainfall = (OverallRainfall)settings.rainfall;
            if (settings.randomiseRainfall)
                rainfall = (OverallRainfall)settings.randomiseRainfallRange.RandomInRange;

            OverallTemperature temperature = (OverallTemperature)settings.temperature;
            if (settings.randomiseTemperature)
                temperature = (OverallTemperature)settings.randomiseTemperatureRange.RandomInRange;

            OverallPopulation population = (OverallPopulation)settings.population;
            if (settings.randomisePopulation)
                population = (OverallPopulation)settings.randomisePopulationRange.RandomInRange;

            LandmarkDensity landmarkDensity = (LandmarkDensity)settings.landmarkDensity;
            if (settings.randomiseLandmarkDensity)
                landmarkDensity = (LandmarkDensity)
                    settings.randomiseLandmarkDensityRange.RandomInRange;

            float pollution = settings.pollution;
            if (settings.randomisePollution)
                pollution = settings.randomisePollutionRange.RandomInRange;

            List<FactionDef> worldFactions = new List<FactionDef>();

            foreach (string name in settings.factionsAlwaysAdd ?? new List<string>())
            {
                FactionDef faction = DefDatabase<FactionDef>.GetNamedSilentFail(name);
                if (faction != null) worldFactions.Add(faction);
            }
            var randomFactions = (settings.factionsRandomlyAdd ?? new List<string>())
                .Select(name => DefDatabase<FactionDef>.GetNamedSilentFail(name))
                .Where(faction => faction != null).Distinct().ToList();
            int factionCount = settings.randomFactionRange.RandomInRange;
            for (int i = 0; i < factionCount && randomFactions.Count > 0; i++)
            {
                FactionDef faction = randomFactions.RandomElement();
                worldFactions.Add(faction);
                if (settings.uniqueFactions) randomFactions.Remove(faction);
            }

            if (initializeOptionalMods && (
                ModsConfig.IsActive("OskarPotocki.VFE.Empire")
                || ModsConfig.IsActive("OskarPotocki.VFE.Empire_Steam")
            ))
            {
                Compat.VFEECompat.EnsureScenarioFactions(worldFactions);
            }

            if (initializeOptionalMods && (
                ModsConfig.IsActive("OskarPotocki.VFE.Deserters")
                || ModsConfig.IsActive("OskarPotocki.VFE.Deserters_Steam")
            ))
            {
                Compat.VFEDCompat.EnsureScenarioFactions(worldFactions);
            }

            if (initializeOptionalMods && (
                ModsConfig.IsActive("kentington.saveourship2")
                || ModsConfig.IsActive("kentington.saveourship2_steam")
            ))
            {
                Compat.SOS2Compat.SetupForStartInSpace();
            }

            foreach (ScenPart part in Find.Scenario.AllParts)
            {
                FactionDef required = part.def?.preventRemovalOfFaction;
                if (required != null && !worldFactions.Contains(required)) worldFactions.Add(required);
            }
            Util.LogMessage($"Created {worldFactions.Count} factions");

            string worldSeed;
            if (settings.randomiseWorldSeed)
            {
                Util.LogMessage("Randomising world seed");
                worldSeed = GenText.RandomSeedString();
            }
            else
            {
                if (string.IsNullOrEmpty(settings.worldSeed))
                {
                    Util.LogMessage($"World Seed: '{settings.worldSeed}' is invalid");
                    worldSeed = GenText.RandomSeedString();
                    settings.worldSeed = worldSeed;
                }
                else
                {
                    Util.LogMessage("Using custom world seed");
                    worldSeed = settings.worldSeed;
                }
            }

            Util.LogMessage($"Using world seed: '{worldSeed}'");

            return new WorldSetupPlan(settings.planetCoverage, worldSeed, rainfall, temperature,
                population, landmarkDensity, worldFactions, pollution);
        }
    }

    internal sealed class WorldSetupPlan
    {
        internal readonly float Coverage, Pollution;
        internal readonly string Seed;
        internal readonly OverallRainfall Rainfall;
        internal readonly OverallTemperature Temperature;
        internal readonly OverallPopulation Population;
        internal readonly LandmarkDensity LandmarkDensity;
        internal readonly List<FactionDef> Factions;

        internal WorldSetupPlan(float coverage, string seed, OverallRainfall rainfall,
            OverallTemperature temperature, OverallPopulation population, LandmarkDensity landmarkDensity,
            List<FactionDef> factions, float pollution)
        {
            Coverage = coverage; Seed = seed; Rainfall = rainfall; Temperature = temperature;
            Population = population; LandmarkDensity = landmarkDensity; Factions = factions; Pollution = pollution;
        }
    }
}
