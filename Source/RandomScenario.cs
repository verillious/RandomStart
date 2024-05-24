using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using Verse;

namespace RandomStartMod
{    public static class RandomScenario
    {
        public static void SetupForRandomPlay()
        {

            Util.LogMessage($"Randomising Scenario");
            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();

            Current.ProgramState = ProgramState.Entry;
            Current.Game = new Game();
            Current.Game.InitData = new GameInitData();

            ScenarioDef chosenScenario = ScenarioDefOf.Crashlanded;
            List<ScenarioDef> possibleScenarios = new List<ScenarioDef>();

            foreach (ScenarioDef item in DefDatabase<ScenarioDef>.AllDefs)
            {
                if (!settings.disabledScenarios.Contains(item.defName) && item.scenario.showInUI)
                {
                    possibleScenarios.Add(item);
                }
            }

            if (possibleScenarios.Count > 0)
            {
                int scenarioIndex = Rand.Range(0, possibleScenarios.Count);
                chosenScenario = possibleScenarios[scenarioIndex];
            }
            else
            {
                settings.disabledScenarios.Remove(chosenScenario.defName);
            }


            Current.Game.Scenario = chosenScenario.scenario;

            Util.LogMessage($"Starting {Current.Game.Scenario}");

            DifficultyDef chosenDifficulty = DifficultyDefOf.Rough;

            foreach (DifficultyDef item in DefDatabase<DifficultyDef>.AllDefs)
            {
                if (item.defName == settings.difficulty)
                {
                    chosenDifficulty = item;
                    break;
                }
            }
            
            StorytellerDef chosenStoryteller = StorytellerDefOf.Cassandra;
            List<StorytellerDef> possibleStorytellers = new List<StorytellerDef>();

            foreach (StorytellerDef item in DefDatabase<StorytellerDef>.AllDefs)
            {
                if (!settings.disabledStorytellers.Contains(item.defName))
                {
                    possibleStorytellers.Add(item);
                }
            }

            if(possibleStorytellers.Count > 0)
            {
                int storytellerIndex = Rand.Range(0, possibleStorytellers.Count);
                chosenStoryteller = possibleStorytellers[storytellerIndex];
            }
            else
            {
                settings.disabledStorytellers.Remove(chosenStoryteller.defName);
            }

            chosenStoryteller.tutorialMode = false;

            Current.Game.storyteller = new Storyteller(chosenStoryteller, chosenDifficulty);



            if (ModsConfig.AnomalyActive)
            {
                Difficulty difficulty = Current.Game.storyteller.difficulty;
                AnomalyPlaystyleDef chosenAnomalyPlaystyleDef = DefDatabase<AnomalyPlaystyleDef>.GetNamed(settings.anomalyPlaystyle);
                if (chosenAnomalyPlaystyleDef.overrideThreatFraction)
                {
                    difficulty.overrideAnomalyThreatsFraction = settings.overrideAnomalyThreatsFraction;
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

            OverallRainfall rainfall = (OverallRainfall)settings.rainfall;
            if (settings.randomiseRainfall)
                rainfall = (OverallRainfall)Rand.Range(0, 6);

            OverallTemperature temperature = (OverallTemperature)settings.temperature;
            if (settings.randomiseTemperature)
                temperature = (OverallTemperature)Rand.Range(0, 6);

            OverallPopulation population = (OverallPopulation)settings.population;
            if (settings.randomisePopulation)
                population = (OverallPopulation)Rand.Range(0, 6);

            float pollution = settings.pollution;
            if (settings.randomisePollution)
                pollution = Rand.Range(0f, 1f);

            List<FactionDef> selectedFactions = DefDatabase<FactionDef>.AllDefsListForReading.Where((FactionDef f) => !settings.disabledFactions.Contains(f.defName)).ToList();
            List<FactionDef> worldFactions = new List<FactionDef>();

            if (settings.randomiseFactions)
            {
                IEnumerable<FactionDef> allFactionDefs = DefDatabase<FactionDef>.AllDefsListForReading.Where((FactionDef x) => x.isPlayer);

                foreach (string factionDefName in settings.factionsAlwaysAdd)
                {
                    FactionDef faction = DefDatabase<FactionDef>.GetNamed(factionDefName, false);
                    if (faction == null)
                        continue;
                    worldFactions.Add(faction);
                }
                if (settings.factionsRandomlyAdd.Count > 0)
                {
                    int factionCount = worldFactions.Count((FactionDef x) => !x.hidden);
                    int diff = 11 - factionCount;
                    if (diff > 0)
                    {
                        for (int i = 0; i < diff; i++)
                        {
                            FactionDef faction = DefDatabase<FactionDef>.GetNamed(settings.factionsRandomlyAdd[Rand.Range(0, settings.factionsRandomlyAdd.Count)], false);
                            if (faction == null)
                                continue;
                            worldFactions.Add(faction);
                        }
                    }

                }

            }
            else
            {
                worldFactions = null;
            }

            if (Util.IsModRunning("Vanilla Factions Expanded - Empire"))
            {
                Compat.VFEECompat.EnsureScenarioFactions(worldFactions);
            }

            if (Util.IsModRunning("Vanilla Factions Expanded - Deserters"))
            {
                Compat.VFEDCompat.EnsureScenarioFactions(worldFactions);
            }

            Current.Game.World = WorldGenerator.GenerateWorld(settings.planetCoverage, GenText.RandomSeedString(), rainfall, temperature, population, worldFactions, pollution);

            Find.GameInitData.ChooseRandomStartingTile();

            Season startingSeason = (Season)settings.startingSeason;
            if (settings.randomiseSeason)
                startingSeason = (Season)Rand.Range(1, 6);

            Find.GameInitData.startingSeason = startingSeason;
            Find.GameInitData.mapSize = settings.mapSize;
            Find.GameInitData.permadeath = settings.permadeath;
            
            if (Util.IsModRunning("Vanilla Expanded Framework"))
            {
                Compat.VECoreCompat.SetupForKCSG();
            }

            Find.Scenario.PostIdeoChosen();

            Find.GameInitData.startedFromEntry = true;

            PageUtility.InitGameStart();
        }
    }
}
