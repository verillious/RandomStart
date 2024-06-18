using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RandomStartMod
{
    [HarmonyPatch(typeof(ResearchUtility), "ApplyPlayerStartingResearch")]
    internal class ResearchUtility_Patch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            if (!RandomStartData.startedFromRandom)
            {
                return true;
            }
            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();
            if (!settings.removeStartingResearch && !settings.addRandomResearch)
            {
                return true;
            }

            if (!settings.removeStartingResearch)
            {
                if (Faction.OfPlayer.def.startingResearchTags != null)
                {
                    foreach (ResearchProjectTagDef startingResearchTag in Faction.OfPlayer.def.startingResearchTags)
                    {
                        foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
                        {
                            if (allDef.HasTag(startingResearchTag))
                            {
                                Find.ResearchManager.FinishProject(allDef, doCompletionDialog: false, null, doCompletionLetter: false);
                            }
                        }
                    }
                }
                foreach (ResearchProjectTagDef startingTechprintsResearchTag in Faction.OfPlayer.def.startingTechprintsResearchTags)
                {
                    foreach (ResearchProjectDef allDef2 in DefDatabase<ResearchProjectDef>.AllDefs)
                    {
                        if (allDef2.HasTag(startingTechprintsResearchTag))
                        {
                            int techprints = Find.ResearchManager.GetTechprints(allDef2);
                            if (techprints < allDef2.TechprintCount)
                            {
                                Find.ResearchManager.AddTechprints(allDef2, allDef2.TechprintCount - techprints);
                            }
                        }
                    }
                }
            }
            Ideo ideo;
            if (ModLister.IdeologyInstalled && (ideo = Faction.OfPlayer.ideos?.PrimaryIdeo) != null)
            {
                foreach (MemeDef meme in ideo.memes)
                {
                    foreach (ResearchProjectDef startingResearchProject in meme.startingResearchProjects)
                    {
                        Find.ResearchManager.FinishProject(startingResearchProject, doCompletionDialog: false, null, doCompletionLetter: false);
                    }
                }
            }
            if (settings.addRandomResearch)
            {
                List<ResearchProjectDef> possibleProjects = DefDatabase<ResearchProjectDef>
                    .AllDefsListForReading.Where(def =>
                        (
                            (int)def.techLevel <= settings.randomResearchTechLevelLimit
                            && def.tab.label != "anomaly"
                        )
                    )
                    .ToList();
                if (possibleProjects.Count > 0)
                {
                    for (int i = 0; i < settings.randomResearchRange.RandomInRange; i++)
                    {
                        ResearchProjectDef projectDef = possibleProjects.RandomElement();
                        possibleProjects.Remove(projectDef);
                        Find.ResearchManager.FinishProject(
                            projectDef,
                            doCompletionDialog: false,
                            null,
                            doCompletionLetter: false
                        );
                    }
                }
            }
            return false;
        }
    }
}
