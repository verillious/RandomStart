using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RandomStartMod.Compat
{

    public static class VFEDCompat
    {
        public static void EnsureScenarioFactions(List<FactionDef> factions)
        {
            if (factions.ContainsAny((FactionDef f) => f.defName == "VFEE_Deserters"))
                return;

            if (
                Current.Game.Scenario.AllParts.Count(
                    (ScenPart s) => s.def.defName == "VFED_StartDeserting"
                ) > 0
            )
            {
                Util.LogMessage("[VFEDCompat] Adding Mandatory Deserter, Empire factions");
                FactionDef empireFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First(
                    (FactionDef f) => f.defName == "Empire"
                );
                if (empireFactionDef != null)
                {
                    factions.Add(empireFactionDef);
                }
                FactionDef deserterFactionDef = DefDatabase<FactionDef>.AllDefsListForReading.First(
                    (FactionDef f) => f.defName == "VFEE_Deserters"
                );
                if (deserterFactionDef != null)
                {
                    factions.Add(deserterFactionDef);
                }
            }
        }
    }

}
