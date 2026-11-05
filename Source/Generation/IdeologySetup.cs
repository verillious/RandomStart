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
                if (settings.overrideIdeo && !File.Exists(settings.customIdeoOverrideFile))
                    throw new InvalidOperationException("RandomStartMod.SavedIdeoMissing".Translate());
                if (settings.overrideIdeo)
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
                            $"[Random Start] Couldn't load ideo file {Path.GetFileName(settings.customIdeoOverrideFile)} - was it saved with different mods?"
                        );
                    }
                }
                else
                {
                    var disallowedPreceptDefs = ResolveOptionalDefs<PreceptDef>(settings.disallowedPrecepts);
                    var disallowedMemeDefs = ResolveOptionalDefs<MemeDef>(settings.disallowedMemes);
                    var forcedMemeDefs = ResolveOptionalDefs<MemeDef>(settings.forcedMemes);

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
                    int memeCount = settings.fluidIdeo ? IdeoUtility.MinFluidIdeoNormalMemes
                        : settings.randomMemeRange.RandomInRange;
                    genParms.forcedMemes = IdeoUtility.GenerateRandomMemes(memeCount, genParms);
                    newIdeo = IdeoGenerator.GenerateIdeo(genParms);
                    // forNewFluidIdeo constrains generation; the resulting ideology
                    // still needs its fluid state/development lifecycle initialized.
                    newIdeo.Fluid = settings.fluidIdeo;
                    if (newIdeo.Fluid && newIdeo.development == null)
                        newIdeo.development = new IdeoDevelopmentTracker(newIdeo);
                }

                if (newIdeo == null)
                    throw new InvalidOperationException("RandomStartMod.SavedIdeoLoadFailed".Translate());
                Find.FactionManager.OfPlayer.ideos.RemoveAll();
                Find.FactionManager.OfPlayer.ideos.SetPrimary(newIdeo);
                Find.IdeoManager.RemoveUnusedStartingIdeos();
                newIdeo.initialPlayerIdeo = true;
                Find.IdeoManager.Add(newIdeo);
            }
        }

        private static List<T> ResolveOptionalDefs<T>(IEnumerable<string> names) where T : Def, new()
        {
            var resolved = (names ?? Enumerable.Empty<string>()).Select(name => DefDatabase<T>.GetNamedSilentFail(name))
                .Where(definition => definition != null).Distinct().ToList();
            return resolved.Count == 0 ? null : resolved;
        }
    }
}
