using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RandomStartMod
{
    [HarmonyPatch(typeof(ResearchUtility), nameof(ResearchUtility.ApplyPlayerStartingResearch))]
    internal static class ResearchUtility_Patch
    {
        [HarmonyPrefix]
        static void Prefix(out IDisposable __state)
        {
            var state = RandomStartData.State;
            __state = null;
            if (state == null || !state.Settings.removeStartingResearch) return;
            __state = state.GrantingResearch.Enter(true);
            state.ProtectedIdeologyResearch.Clear();
            var memes = Faction.OfPlayer?.ideos?.PrimaryIdeo?.memes;
            if (memes == null) return;
            foreach (var meme in memes)
                foreach (var project in meme.startingResearchProjects ?? new List<ResearchProjectDef>())
                {
                    state.ProtectedIdeologyResearch.Add(project);
                    state.ProtectedIdeologyResearch.UnionWith(DependencyGraph.Descendants(project, item => item.prerequisites));
                }
        }

        [HarmonyPostfix]
        static void Postfix(IDisposable __state)
        {
            __state?.Dispose();
            var state = RandomStartData.State;
            if (state == null || state.ResearchApplied || !state.Settings.addRandomResearch) return;
            state.ResearchApplied = true;
            var settings = state.Settings;
            var pool = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(project =>
                (int)project.techLevel <= settings.randomResearchTechLevelLimit
                && project.knowledgeCategory == null && !project.IsFinished).ToList();
            int count = Math.Min(Math.Max(0, settings.randomResearchRange.RandomInRange), pool.Count);
            for (int i = 0; i < count; i++)
            {
                ResearchProjectDef project = pool.RandomElement();
                pool.Remove(project);
                if (project.IsFinished) continue;
                var blocked = settings.doRandomResearchPrerequisites ? null
                    : DependencyGraph.Descendants(project, candidate => candidate.prerequisites);
                using (state.BlockedPrerequisites.Enter(blocked))
                    Find.ResearchManager.FinishProject(project, false, null, false);
            }
        }

        [HarmonyFinalizer]
        static void Finalizer(IDisposable __state) => __state?.Dispose();
    }

    // The original starting-grant method always executes. Only the specific
    // faction grants requested for removal are withheld, and only during that call.
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.FinishProject))]
    internal static class StartingResearchGrant_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ResearchManager __instance, ResearchProjectDef __0)
        {
            var state = RandomStartData.State;
            if (state == null || !ReferenceEquals(__instance, Find.ResearchManager)) return true;
            if (state.BlockedPrerequisites.Value?.Contains(__0) == true) return false;
            if (!state.GrantingResearch.Value || !state.Settings.removeStartingResearch) return true;
            var tags = Faction.OfPlayer.def.startingResearchTags;
            if (tags == null || !tags.Any(tag => __0.HasTag(tag))) return true;
            // Preserve the existing feature's ideology grants.
            return state.ProtectedIdeologyResearch.Contains(__0);
        }
    }

    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.AddTechprints))]
    internal static class StartingTechprintsGrant_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ResearchManager __instance, ResearchProjectDef __0)
        {
            var state = RandomStartData.State;
            if (state == null || !ReferenceEquals(__instance, Find.ResearchManager)
                || !state.GrantingResearch.Value || !state.Settings.removeStartingResearch) return true;
            if (state.ProtectedIdeologyResearch.Contains(__0)) return true;
            var tags = Faction.OfPlayer.def.startingTechprintsResearchTags;
            return tags == null || !tags.Any(tag => __0.HasTag(tag));
        }
    }
}
