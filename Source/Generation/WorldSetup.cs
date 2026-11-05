using System.Collections.Generic;
using System.Linq;
using RandomStartMod.Compat;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RandomStartMod
{
    public static partial class RandomScenario
    {

        private static void SetupRandomPlanet(RandomStartSettings settings)
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

            ScenarioFactionRequirements.Apply(worldFactions, Find.Scenario.AllParts);
            if (ModsConfig.IsActive("kentington.saveourship2") || ModsConfig.IsActive("kentington.saveourship2_steam"))
                SOS2Compat.SetupForStartInSpace();
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

            if (ModsConfig.IsActive("Oblitus.MyLittlePlanet") || ModsConfig.IsActive("Oblitus.MyLittlePlanet_Steam"))
                MLPCompat.ApplyWorldSize(settings.myLittlePlanetSubcount);

            if (
                ModsConfig.IsActive("koth.RealisticPlanets1.6")
                || ModsConfig.IsActive("koth.RealisticPlanets1.6_Steam")
            )
            {
                int oceanType = settings.realisticPlanetsOceanType;
                if (settings.randomiseOceanType)
                {
                    oceanType = settings.randomiseOceanTypeRange.RandomInRange;
                }
                int axialTilt = settings.realisticPlanetsAxialTilt;
                if (settings.randomiseAxialTilt)
                {
                    axialTilt = settings.randomiseAxialTiltRange.RandomInRange;
                }
                bool smallPlanet = ModsConfig.IsActive("Oblitus.MyLittlePlanet") || ModsConfig.IsActive("Oblitus.MyLittlePlanet_Steam");
                RealisticPlanetsCompat.ApplyWorldSettings(smallPlanet ? settings.myLittlePlanetSubcount : 10,
                    oceanType, axialTilt, settings.realisticPlanetsWorldType);
            }

            // All world-generation mods participate in this one live call. A
            // failed generator must propagate its error, never be run twice.
            Current.Game.World = WorldGenerator.GenerateWorld(
                settings.planetCoverage,
                worldSeed,
                rainfall,
                temperature,
                population,
                landmarkDensity,
                worldFactions,
                pollution
            );
        }

    }
}
