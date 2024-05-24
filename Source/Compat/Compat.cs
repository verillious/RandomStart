using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RandomStartMod.Compat
{
    public static class VECoreCompat
    {
        public static void SetupForKCSG()
        {
            Util.LogMessage("[Compat] Setting up for KCSG AddStartingStructure");
            if (Current.Game.Scenario.AllParts.ToList().Any((ScenPart s) => s.def.defName == "VFEC_AddStartingStructure"))
            {
                KCSG.PrepareCarefully_Util.pcScenariosSave.Clear();
                KCSG.ScenPart_AddStartingStructure scenPart_AddStartingStructure = (KCSG.ScenPart_AddStartingStructure)Current.Game.Scenario.AllParts.ToList().Find((ScenPart s) => s.def.defName == "VFEC_AddStartingStructure");
                List<KCSG.StructureLayoutDef> chooseFrom = scenPart_AddStartingStructure.chooseFrom;
                if (chooseFrom != null && chooseFrom.Count > 0)
                {
                    KCSG.PrepareCarefully_Util.pcScenariosSave.Add(scenPart_AddStartingStructure.chooseFrom.RandomElement(), scenPart_AddStartingStructure.nearMapCenter);
                }
            }
        }
    }
    public static class VFEECompat
    {
        public static void EnsureScenarioFactions(List<FactionDef> factions)
        {
            if (factions.ContainsAny((FactionDef f) => f.defName == "VFEE_Deserters"))
                return;

            if (Current.Game.Scenario.AllParts.Count((ScenPart s) => s.def.defName == "VFEE_SpawnRaid") > 0)
            {
                Util.LogMessage("[Compat] Adding Mandatory Deserter faction");
                FactionDef deserterFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First((FactionDef f) => f.defName == "VFEE_Deserters");
                if (deserterFactionDef != null)
                {
                    factions.Add(deserterFactionDef);

                }
            }
        }
    }

    public static class VFEDCompat
    {
        public static void EnsureScenarioFactions(List<FactionDef> factions)
        {
            if (factions.ContainsAny((FactionDef f) => f.defName == "VFEE_Deserters"))
                return;

            if (Current.Game.Scenario.AllParts.Count((ScenPart s) => s.def.defName == "VFED_StartDeserting") > 0)
            {
                Util.LogMessage("[Compat] Adding Mandatory Deserter faction");
                FactionDef deserterFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First((FactionDef f) => f.defName == "VFEE_Deserters");
                if (deserterFactionDef != null)
                {
                    factions.Add(deserterFactionDef);

                }
            }
        }
    }
}
