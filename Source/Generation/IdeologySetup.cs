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

        private static void SetupRandomIdeology(RandomStartSettings settings)
        {
            if (settings.disableIdeo)
            {
                Find.IdeoManager.classicMode = true;
                IdeoGenerationParms genParms = new IdeoGenerationParms(
                    Find.FactionManager.OfPlayer.def
                );
                if (
                    !DefDatabase<CultureDef>
                        .AllDefs.Where(
                            (CultureDef x) =>
                                Find.FactionManager.OfPlayer.def.allowedCultures.Contains(x)
                        )
                        .TryRandomElement(out var result)
                )
                {
                    result = DefDatabase<CultureDef>.AllDefs.RandomElement();
                }
                Ideo classicIdeo = IdeoGenerator.GenerateClassicIdeo(
                    result,
                    genParms,
                    noExpansionIdeo: false
                );
                foreach (Faction allFaction in Find.FactionManager.AllFactions)
                {
                    if (allFaction.ideos != null)
                    {
                        allFaction.ideos.RemoveAll();
                        allFaction.ideos.SetPrimary(classicIdeo);
                    }
                }
                Find.IdeoManager.RemoveUnusedStartingIdeos();
                classicIdeo.initialPlayerIdeo = true;
                Find.IdeoManager.Add(classicIdeo);
            }
            else
            {
                Ideo newIdeo = null;
                if (settings.overrideIdeo && File.Exists(settings.customIdeoOverrideFile))
                {
                    Util.LogMessage("Attempting to load ideo file");
                    PreLoadUtility.CheckVersionAndLoad(
                        settings.customIdeoOverrideFile,
                        ScribeMetaHeaderUtility.ScribeHeaderMode.Ideo,
                        delegate
                        {
                            if (
                                GameDataSaveLoader.TryLoadIdeo(
                                    settings.customIdeoOverrideFile,
                                    out var ideo
                                )
                            )
                            {
                                newIdeo = IdeoGenerator.InitLoadedIdeo(ideo);
                            }
                        }
                    );

                    if (newIdeo != null)
                    {
                        Util.LogMessage("Loaded ideo file");
                    }
                    else
                    {
                        Log.Error(
                            $"[Random Start] Couldn't load ideo file {settings.customIdeoOverrideFile.Split('\\').Last()} - was it saved with different mods?"
                        );
                    }
                }
                else
                {
                    // Convert string lists to actual defs
                    List<PreceptDef> disallowedPreceptDefs = null;
                    if (settings.disallowedPrecepts?.Count > 0)
                    {
                        disallowedPreceptDefs = new List<PreceptDef>();
                        foreach (string preceptDefName in settings.disallowedPrecepts)
                        {
                            PreceptDef preceptDef = DefDatabase<PreceptDef>.GetNamed(
                                preceptDefName,
                                false
                            );
                            if (preceptDef != null)
                            {
                                disallowedPreceptDefs.Add(preceptDef);
                            }
                        }
                    }

                    List<MemeDef> disallowedMemeDefs = null;
                    if (settings.disallowedMemes?.Count > 0)
                    {
                        disallowedMemeDefs = new List<MemeDef>();
                        foreach (string memeDefName in settings.disallowedMemes)
                        {
                            MemeDef memeDef = DefDatabase<MemeDef>.GetNamed(memeDefName, false);
                            if (memeDef != null)
                            {
                                disallowedMemeDefs.Add(memeDef);
                            }
                        }
                    }

                    List<MemeDef> forcedMemeDefs = null;
                    if (settings.forcedMemes?.Count > 0)
                    {
                        forcedMemeDefs = new List<MemeDef>();
                        foreach (string memeDefName in settings.forcedMemes)
                        {
                            MemeDef memeDef = DefDatabase<MemeDef>.GetNamed(memeDefName, false);
                            if (memeDef != null)
                            {
                                forcedMemeDefs.Add(memeDef);
                            }
                        }
                    }

                    IdeoGenerationParms genParms = new IdeoGenerationParms(
                        Find.FactionManager.OfPlayer.def,
                        forceNoExpansionIdeo: false,
                        disallowedPreceptDefs,
                        disallowedMemeDefs,
                        forcedMemeDefs, // this overrides memes, rather than adding to them
                        classicExtra: false,
                        forceNoWeaponPreference: false,
                        settings.fluidIdeo
                    );
                    newIdeo = IdeoGenerator.GenerateIdeo(genParms);
                    newIdeo.memes.Clear();
                    int memeCount = settings.randomMemeRange.RandomInRange;
                    newIdeo.memes.AddRange(GenerateRandomMemes(memeCount, genParms));
                    newIdeo.SortMemesInDisplayOrder();
                    newIdeo.foundation.RandomizePrecepts(true, genParms);
                }

                Find.FactionManager.OfPlayer.ideos.RemoveAll();
                Find.FactionManager.OfPlayer.ideos.SetPrimary(newIdeo);
                Find.IdeoManager.RemoveUnusedStartingIdeos();
                newIdeo.initialPlayerIdeo = true;
                Find.IdeoManager.Add(newIdeo);
            }
        }

        private static List<MemeDef> GenerateRandomMemes(int count, IdeoGenerationParms parms)
        {
            FactionDef forFaction = parms.forFaction;
            bool forPlayerFaction = forFaction != null && forFaction.isPlayer;
            List<MemeDef> memes = new List<MemeDef>();
            bool flag = false;
            if (forFaction != null && forFaction.requiredMemes != null)
            {
                for (int i = 0; i < forFaction.requiredMemes.Count; i++)
                {
                    memes.Add(forFaction.requiredMemes[i]);
                    if (forFaction.requiredMemes[i].category == MemeCategory.Normal)
                    {
                        count--;
                    }
                    else if (forFaction.requiredMemes[i].category == MemeCategory.Structure)
                    {
                        flag = true;
                    }
                }
            }

            if (parms.forcedMemes != null)
            {
                foreach (MemeDef forcedMeme in parms.forcedMemes)
                {
                    if (forcedMeme.category == MemeCategory.Structure)
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (forFaction != null && forFaction.structureMemeWeights != null && !flag)
            {
                MemeWeight result2;
                if (
                    forFaction
                        .structureMemeWeights.Where(
                            (MemeWeight x) =>
                                IdeoUtility.CanAdd(x.meme, memes, forFaction, parms.forNewFluidIdeo)
                                && (forPlayerFaction || !IdeoUtility.AnyIdeoHas(x.meme))
                        )
                        .TryRandomElementByWeight(
                            (MemeWeight x) =>
                                x.selectionWeight * x.meme.randomizationSelectionWeightFactor,
                            out var result
                        )
                )
                {
                    memes.Add(result.meme);
                    flag = true;
                }
                else if (
                    forFaction
                        .structureMemeWeights.Where(
                            (MemeWeight x) =>
                                IdeoUtility.CanAdd(x.meme, memes, forFaction, parms.forNewFluidIdeo)
                        )
                        .TryRandomElementByWeight(
                            (MemeWeight x) =>
                                x.selectionWeight * x.meme.randomizationSelectionWeightFactor,
                            out result2
                        )
                )
                {
                    memes.Add(result2.meme);
                    flag = true;
                }
            }

            if (!flag)
            {
                MemeDef result4;
                if (
                    DefDatabase<MemeDef>
                        .AllDefs.Where(
                            (MemeDef x) =>
                                x.category == MemeCategory.Structure
                                && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo)
                                && (forPlayerFaction || !IdeoUtility.AnyIdeoHas(x))
                        )
                        .TryRandomElement(out var result3)
                )
                {
                    memes.Add(result3);
                }
                else if (
                    DefDatabase<MemeDef>
                        .AllDefs.Where(
                            (MemeDef x) =>
                                x.category == MemeCategory.Structure
                                && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo)
                        )
                        .TryRandomElementByWeight(
                            (MemeDef x) => x.randomizationSelectionWeightFactor,
                            out result4
                        )
                )
                {
                    memes.Add(result4);
                }
            }

            if (parms.forcedMemes != null)
            {
                memes.AddRange(parms.forcedMemes);
            }

            for (int num = memes.Count; num <= count; num++)
            {
                MemeDef result6;
                if (
                    DefDatabase<MemeDef>
                        .AllDefs.Where(
                            (MemeDef x) =>
                                x.category == MemeCategory.Normal
                                && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo)
                                && (forPlayerFaction || !IdeoUtility.AnyIdeoHas(x))
                                && (
                                    parms.disallowedMemes == null
                                    || !parms.disallowedMemes.Contains(x)
                                )
                        )
                        .TryRandomElementByWeight(
                            (MemeDef x) => x.randomizationSelectionWeightFactor,
                            out var result5
                        )
                )
                {
                    memes.Add(result5);
                }
                else if (
                    DefDatabase<MemeDef>
                        .AllDefs.Where(
                            (MemeDef x) =>
                                x.category == MemeCategory.Normal
                                && IdeoUtility.CanAdd(x, memes, forFaction, parms.forNewFluidIdeo)
                                && (
                                    parms.disallowedMemes == null
                                    || !parms.disallowedMemes.Contains(x)
                                )
                        )
                        .TryRandomElementByWeight(
                            (MemeDef x) => x.randomizationSelectionWeightFactor,
                            out result6
                        )
                )
                {
                    memes.Add(result6);
                }
            }
            return memes;
        }

    }
}
