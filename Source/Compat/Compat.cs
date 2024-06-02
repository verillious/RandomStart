using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RandomStartMod.Compat
{
    public static class VECoreCompat
    {
        public static void SetupForKCSG()
        {
            Util.LogMessage("[VECoreCompat] Setting up for KCSG AddStartingStructure");
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
                Util.LogMessage("[VFEECompat] Adding Mandatory Deserter faction");
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
                Util.LogMessage("[VFEDCompat] Adding Mandatory Deserter faction");
                FactionDef deserterFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First((FactionDef f) => f.defName == "VFEE_Deserters");
                if (deserterFactionDef != null)
                {
                    factions.Add(deserterFactionDef);

                }
            }
        }
    }

    public static class SOS2Compat
    {
        public static void SetupForStartInSpace()
        {
            Util.LogMessage("[SOS2Compat] Running DoEarlyInit on SOS2 ScenParts");
            foreach (ScenPart part in Find.Scenario.AllParts)
            {
                if (part is SaveOurShip2.ScenPart_LoadShip p && p.HasValidFilename())
                {
                    p.DoEarlyInit();
                }
                else if (part is SaveOurShip2.ScenPart_StartInSpace s)
                {
                    s.DoEarlyInit();
                }
            }
        }
    }

    public static class MLPCompat
    {
        public static void DrawMLPSlider(Rect rect)
        {
            WorldGenRules.RulesOverrider.subcount = Mathf.RoundToInt(Widgets.HorizontalSlider(rect, WorldGenRules.RulesOverrider.subcount, 6f, 10f, middleAlignment: true, null, "MLPWorldTiny".Translate(), "MLPWorldDefault".Translate(), 1f));
        }
    }
}
