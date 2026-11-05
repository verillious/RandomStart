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

        private static void SetupRandomGenes(RandomStartSettings settings)
        {
            var pawns = Find.GameInitData.startingAndOptionalPawns.Where(pawn => pawn.genes != null).ToList();
            if (settings.enableRandomXenotypes)
            {
                var xenotypes = DefDatabase<XenotypeDef>.AllDefsListForReading.AsEnumerable();
                var factionSet = Faction.OfPlayer.def.basicMemberKind?.xenotypeSet;
                if (settings.respectFactionXenotypes && factionSet != null)
                    xenotypes = xenotypes.Where(xenotype => factionSet.Contains(xenotype));
                var pool = xenotypes.ToList();
                if (pool.Count == 0)
                    Log.Warning("[Random Start] No eligible xenotypes; keeping scenario-generated xenotypes.");
                else
                    foreach (Pawn pawn in pawns) pawn.genes.SetXenotype(pool.RandomElement());
            }
            if (!settings.enableRandomCustomXenotypes) return;
            var eligible = DefDatabase<GeneDef>.AllDefsListForReading.Where(gene => gene.canGenerateInGeneSet).ToList();
            var invalid = new HashSet<GeneDef>();
            foreach (Pawn pawn in pawns)
            {
                var pool = new List<GeneDef>(eligible);
                int count = Math.Min(Math.Max(0, settings.randomGeneRange.RandomInRange), pool.Count);
                bool changed = false;
                for (int i = 0; i < count; i++)
                {
                    GeneDef root = pool.RandomElement();
                    pool.Remove(root);
                    List<GeneDef> ordered;
                    if (!DependencyGraph.TryOrder(root,
                        gene => gene.prerequisite == null ? null : new[] { gene.prerequisite }, out ordered))
                    {
                        if (invalid.Add(root)) Log.Warning($"[Random Start] Skipping cyclic gene prerequisites for {root.defName}.");
                        continue;
                    }
                    bool xenogene = Rand.Bool;
                    changed |= GeneBatch.TryApply(ordered,
                        gene => pawn.genes.GenesListForReading.Any(existing => existing.def == gene),
                        gene => pawn.genes.AddGene(gene, xenogene), gene => pawn.genes.RemoveGene(gene),
                        () => !settings.enableMetabolicEfficiencyMinimum
                            || pawn.genes.GenesListForReading.Where(gene => gene.Active).Sum(gene => gene.def.biostatMet)
                                >= settings.minimumMetabolicEfficiency);
                }
                if (!changed) continue;
                var active = pawn.genes.GenesListForReading.Where(gene => gene.Active).Select(gene => gene.def).Distinct().ToList();
                if (active.Count == 0) continue;
                pawn.genes.xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(active);
                if (DefDatabase<XenotypeIconDef>.AllDefsListForReading.TryRandomElement(out var icon))
                    pawn.genes.iconDef = icon;
            }
        }

    }
}
