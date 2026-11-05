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

    public static class VFEECompat
    {
        public static void EnsureScenarioFactions(List<FactionDef> factions)
        {
            if (factions.ContainsAny((FactionDef f) => f.defName == "VFEE_Deserters"))
                return;

            if (
                Current.Game.Scenario.AllParts.Count(
                    (ScenPart s) => s.def.defName == "VFEE_SpawnRaid"
                ) > 0
            )
            {
                Util.LogMessage("[VFEECompat] Adding Mandatory Empire, Deserter factions");
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
