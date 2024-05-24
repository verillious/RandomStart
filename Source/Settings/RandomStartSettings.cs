using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RandomStartMod
{
    public class RandomStartSettings : ModSettings
    {
        public string difficulty = "Rough";
        public int mapSize = 250;
        public float planetCoverage = 0.3f;

        public bool permadeath = false;
        public bool randomiseRainfall = true;
        public bool randomiseTemperature = true;
        public bool randomisePopulation = true;
        public bool randomisePollution = false;
        public bool randomiseSeason = false;
        public bool randomiseFactions = true;

        public int rainfall = 3;
        public int temperature = 3;
        public int population = 3;
        public float pollution = 0.05f;
        public int startingSeason = 2;

        public int minimumFactions = 5;
        public int maximumFactions = 15;

        public bool enableCustomScenarios = true;
        public bool enableSteamWorkshopScenarios = true;

        public List<string> disabledStorytellers = new List<string>();
        public List<string> disabledScenarios = new List<string>() { "Tutorial" };
        public List<string> factionsAlwaysAdd = new List<string>()
        {
            "Ancients",
            "AncientsHostile",
            "Mechanoid",
            "Insect",
            "Empire",
            "HoraxCult",
            "Entities",
            "VFEE_Deserters"
        };
        public List<string> factionsRandomlyAdd = new List<string>()
        {
            "OutlanderCivil",
            "OutlanderRough",
            "TribeCivil",
            "TribeRough",
            "TribeSavage",
            "Pirate",
            "Empire",
            "CannibalPirate",
            "NudistTribe",
            "TribeCannibal",
            "TribeRoughNeanderthal",
            "PirateYttakin",
            "TribeSavageImpid",
            "OutlanderRoughPig",
            "PirateWaster",
            "Sanguophages",
        };

        public Vector2 scrollPosition;

        public string anomalyPlaystyle = "Standard";
        public float anomalyThreatsInactiveFraction = 0.08f;
        public float anomalyThreatsActiveFraction = 0.3f;
        public float studyEfficiencyFactor = 1.0f;
        public float overrideAnomalyThreatsFraction = 0.15f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref difficulty, "difficulty", "Rough");
            Scribe_Values.Look(ref mapSize, "mapSize", 250);
            Scribe_Values.Look(ref planetCoverage, "planetCoverage", 0.3f);
            Scribe_Values.Look(ref permadeath, "permadeath", false);
            Scribe_Values.Look(ref randomiseRainfall, "randomiseRainfall", true);
            Scribe_Values.Look(ref randomiseTemperature, "randomiseTemperature", true);
            Scribe_Values.Look(ref randomisePopulation, "randomisePopulation", true);
            Scribe_Values.Look(ref randomisePollution, "randomisePollution", false);
            Scribe_Values.Look(ref randomiseSeason, "randomiseSeason", false);
            Scribe_Values.Look(ref randomiseFactions, "randomiseFactions", true);

            Scribe_Values.Look(ref enableCustomScenarios, "enableCustomScenarios", true);
            Scribe_Values.Look(
                ref enableSteamWorkshopScenarios,
                "enableSteamWorkshopScenarios",
                true
            );

            Scribe_Values.Look(ref rainfall, "rainfall", 3);
            Scribe_Values.Look(ref temperature, "temperature", 3);
            Scribe_Values.Look(ref population, "population", 3);
            Scribe_Values.Look(ref pollution, "pollution", 0.05f);
            Scribe_Values.Look(ref startingSeason, "startingSeason", 2);

            Scribe_Collections.Look(
                ref disabledStorytellers,
                "disabledStorytellers",
                LookMode.Value,
                new List<string>()
            );
            Scribe_Collections.Look(
                ref disabledScenarios,
                "disabledScenarios",
                LookMode.Value,
                new List<string>() { "Tutorial" }
            );
            Scribe_Collections.Look(
                ref factionsAlwaysAdd,
                "factionsAlwaysAdd",
                LookMode.Value,
                new List<string>()
                {
                    "Ancients",
                    "AncientsHostile",
                    "Mechanoid",
                    "Insect",
                    "Empire",
                    "HoraxCult",
                    "Entities",
                    "VFEE_Deserters"
                }
            );
            Scribe_Collections.Look(
                ref factionsRandomlyAdd,
                "factionsRandomlyAdd",
                LookMode.Value,
                new List<string>()
                {
                    "OutlanderCivil",
                    "OutlanderRough",
                    "TribeCivil",
                    "TribeRough",
                    "TribeSavage",
                    "Pirate",
                    "Empire",
                    "CannibalPirate",
                    "NudistTribe",
                    "TribeCannibal",
                    "TribeRoughNeanderthal",
                    "PirateYttakin",
                    "TribeSavageImpid",
                    "OutlanderRoughPig",
                    "PirateWaster",
                    "Sanguophages",
                }
            );

            Scribe_Values.Look(ref anomalyPlaystyle, "anomalyPlaystyle", "Standard");
            Scribe_Values.Look(
                ref anomalyThreatsInactiveFraction,
                "anomalyThreatsInactiveFraction",
                0.08f
            );
            Scribe_Values.Look(
                ref anomalyThreatsActiveFraction,
                "anomalyThreatsActiveFraction",
                0.3f
            );
            Scribe_Values.Look(ref studyEfficiencyFactor, "studyEfficiencyFactor", 0.05f);
            Scribe_Values.Look(
                ref overrideAnomalyThreatsFraction,
                "overrideAnomalyThreatsFraction",
                0.15f
            );

            base.ExposeData();
        }
    }
}
