using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RandomStartMod.Compat
{
    internal static class ScenarioFactionRequirements
    {
        internal static void Apply(List<FactionDef> factions, IEnumerable<ScenPart> parts)
        {
            foreach (ScenPart part in parts)
            {
                FactionDef declared = part.def?.preventRemovalOfFaction;
                if (declared != null && !factions.Contains(declared)) factions.Add(declared);

                // These upstream 1.6 parts do not declare their required factions.
                // Match the actual scenario content, including forks with other
                // package IDs, and ensure each dependency independently.
                string name = part.def?.defName;
                if (name != "VFEE_SpawnRaid" && name != "VFED_StartDeserting") continue;
                foreach (string required in new[] { "Empire", "VFEE_Deserters" })
                {
                    FactionDef faction = DefDatabase<FactionDef>.GetNamedSilentFail(required)
                        ?? throw new InvalidOperationException($"Scenario part {name} requires missing faction {required}.");
                    if (!factions.Contains(faction)) factions.Add(faction);
                }
            }
        }
    }
}
