using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RandomStartMod
{
    [HarmonyPatch(typeof(ResearchUtility), "ApplyPlayerStartingResearch")]
    internal class ResearchUtility_Patch
    {
        static public void FinishProjectOptionalPrequisites(ResearchProjectDef proj, bool doCompletionDialog = false, Pawn researcher = null, bool doCompletionLetter = true, bool doPrerequisites = true)
        {
            Util.LogMessage($"Unlocking {proj.LabelCap}, Prerequisites: {doPrerequisites}");

            if (proj.prerequisites != null && doPrerequisites)
            {
                for (int i = 0; i < proj.prerequisites.Count; i++)
                {
                    if (!proj.prerequisites[i].IsFinished)
                    {
                        Find.ResearchManager.FinishProject(proj.prerequisites[i], doCompletionDialog, researcher, doCompletionLetter);
                    }
                }
            }


            int num = Find.ResearchManager.GetTechprints(proj);
            if (num < proj.TechprintCount)
            {
                Find.ResearchManager.AddTechprints(proj, proj.TechprintCount - num);
            }

            if (proj.RequiredAnalyzedThingCount > 0)
            {
                for (int j = 0; j < proj.requiredAnalyzed.Count; j++)
                {
                    CompProperties_CompAnalyzableUnlockResearch compProperties = proj.requiredAnalyzed[j].GetCompProperties<CompProperties_CompAnalyzableUnlockResearch>();
                    Find.AnalysisManager.ForceCompleteAnalysisProgress(compProperties.analysisID);
                }
            }

            if (proj.baseCost > 0f)
            {
                Find.ResearchManager.progress[proj] = proj.baseCost;
            }
            else if (ModsConfig.AnomalyActive && proj.knowledgeCost > 0f)
            {
                Find.ResearchManager.anomalyKnowledge.SetOrAdd(proj, proj.knowledgeCost);
                Find.SignalManager.SendSignal(new Signal("ThingStudied", global: true));
            }

            if (researcher != null)
            {
                TaleRecorder.RecordTale(TaleDefOf.FinishedResearchProject, researcher, proj);
            }

            Find.ResearchManager.ReapplyAllMods();
            if (proj.recalculatePower)
            {
                try
                {
                    foreach (Map map in Find.Maps)
                    {
                        foreach (Thing item in map.listerThings.ThingsInGroup(ThingRequestGroup.PowerTrader))
                        {
                            CompPowerTrader compPowerTrader;
                            if ((compPowerTrader = item.TryGetComp<CompPowerTrader>()) != null)
                            {
                                compPowerTrader.SetUpPowerVars();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }

            if (doCompletionDialog)
            {
                DiaNode diaNode = new DiaNode((string)("ResearchFinished".Translate(proj.LabelCap) + "\n\n" + proj.description));
                diaNode.options.Add(DiaOption.DefaultOK);
                DiaOption diaOption = new DiaOption("ResearchScreen".Translate());
                diaOption.resolveTree = true;
                diaOption.action = delegate
                {
                    Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
                    MainTabWindow_Research mainTabWindow_Research;
                    if ((mainTabWindow_Research = MainButtonDefOf.Research.TabWindow as MainTabWindow_Research) != null && proj.tab != null)
                    {
                        mainTabWindow_Research.CurTab = proj.tab;
                    }
                };
                diaNode.options.Add(diaOption);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true));
            }

            if (doCompletionLetter && !proj.discoveredLetterTitle.NullOrEmpty() && Find.Storyteller.difficulty.AllowedBy(proj.discoveredLetterDisabledWhen))
            {
                Find.LetterStack.ReceiveLetter(proj.discoveredLetterTitle, proj.discoveredLetterText, LetterDefOf.NeutralEvent);
            }

            if (proj.teachConcept != null)
            {
                LessonAutoActivator.TeachOpportunity(proj.teachConcept, OpportunityType.Important);
            }

            if (Find.ResearchManager.currentProj == proj)
            {
                Find.ResearchManager.currentProj = null;
            }
            else if (ModsConfig.AnomalyActive && proj.knowledgeCategory != null)
            {
                foreach (ResearchManager.KnowledgeCategoryProject currentAnomalyKnowledgeProject in Find.ResearchManager.CurrentAnomalyKnowledgeProjects)
                {
                    if (currentAnomalyKnowledgeProject.project == proj)
                    {
                        currentAnomalyKnowledgeProject.project = null;
                        break;
                    }
                }
            }

            foreach (Def unlockedDef in proj.UnlockedDefs)
            {
                ThingDef thingDef;
                if ((thingDef = unlockedDef as ThingDef) != null)
                {
                    thingDef.Notify_UnlockedByResearch();
                }
            }

            Find.SignalManager.SendSignal(new Signal("ResearchCompleted", global: true));
        }

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

            Util.LogMessage("Patching ResearchUtility");

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
            else
            {
                Util.LogMessage("Skipping starting research");
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
                        FinishProjectOptionalPrequisites(
                            projectDef,
                            doCompletionDialog: false,
                            null,
                            doCompletionLetter: false,
                            doPrerequisites: settings.doRandomResearchPrerequisites
                        );
                    }
                }
            }

            return false;
        }
    }
}
