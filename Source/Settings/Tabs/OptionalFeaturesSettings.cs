using RandomStartMod.Compat;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RandomStartMod
{
    public partial class RandomStartMod
    {

        private void DoOptionalFeaturesTabContents(Rect inRect)
        {
            if (settings == null)
            {
                Util.LogMessage("Settings is null in DoOptionalFeaturesTabContents");
                return;
            }

            Rect rect = new Rect(0f, 60f, inRect.width, optionalFeaturesListingHeight);
            optionalFeaturesListingHeight = 0f;
            Widgets.BeginScrollView(inRect, ref mainScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            if (ModsConfig.BiotechActive)
            {
                Text.Font = GameFont.Medium;
                listingStandard.Label("RandomStartMod.Misc.GenesTitle".Translate());
                listingStandard.GapLine();
                optionalFeaturesListingHeight += 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                Text.Font = GameFont.Tiny;
                Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.GenesDescription".Translate());
                optionalFeaturesListingHeight += Text.LineHeight;
                Text.Font = GameFont.Small;
                DoSettingToggle(listingStandard.GetRect(24f), $"{"Randomize".Translate()}: {"Xenotype".Translate()}", "RandomStartMod.TooltipTitles.RandomizeXenotype".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.RandomizeXenotypeTooltip".Translate(), ref settings.enableRandomXenotypes);
                optionalFeaturesListingHeight += 24f;
                DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.RespectMemberXenotypes".Translate(), "RandomStartMod.TooltipTitles.RespectFactionXenotypes".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.RespectFactionXenotypesTooltip".Translate(), ref settings.respectFactionXenotypes);
                optionalFeaturesListingHeight += 24f;
                DoSettingToggle(listingStandard.GetRect(24f), $"{"Randomize".Translate()}: {"Genes".Translate().CapitalizeFirst()}", "RandomStartMod.TooltipTitles.RandomizeCustomGenes".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.RandomizeCustomGenesTooltip".Translate(), ref settings.enableRandomCustomXenotypes);
                optionalFeaturesListingHeight += 24f;
                if (settings.enableRandomCustomXenotypes)
                {
                    listingStandard.Gap();
                    listingStandard.Indent(8);
                    listingStandard.Label("RandomStartMod.Misc.CountLabel".Translate());
                    listingStandard.Outdent(8);
                    optionalFeaturesListingHeight += Text.LineHeight + 12f;
                    Widgets.IntRange(listingStandard.GetRect(32f), 382399865, ref settings.randomGeneRange, 0, 20);
                    optionalFeaturesListingHeight += 32f;
                    listingStandard.Gap(2f);
                    DoSettingToggle(listingStandard.GetRect(24f), $"{"minimum".Translate().CapitalizeFirst()}: {"MetabolismTotal".Translate()}", "RandomStartMod.TooltipTitles.MinimumMetabolicEfficiency".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.MinimumMetabolicEfficiencyTooltip".Translate(), ref settings.enableMetabolicEfficiencyMinimum);
                    optionalFeaturesListingHeight += 2f + 24f;
                    if (settings.enableMetabolicEfficiencyMinimum)
                    {
                        listingStandard.Gap(2f);
                        settings.minimumMetabolicEfficiency = Mathf.RoundToInt(Widgets.HorizontalSlider(listingStandard.GetRect(24f), settings.minimumMetabolicEfficiency, -5, 5, true, $"{settings.minimumMetabolicEfficiency.ToStringWithSign()} ({"HungerRate".Translate()} x{GeneTuning.MetabolismToFoodConsumptionFactorCurve.Evaluate(settings.minimumMetabolicEfficiency).ToStringPercent()})", null, null, 1f));
                        optionalFeaturesListingHeight += 2f + 24f;
                    }
                }
            }
            if (ModsConfig.IdeologyActive)
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("RandomStartMod.Misc.IdeologyTitle".Translate());
                listingStandard.GapLine();
                optionalFeaturesListingHeight += 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                Text.Font = GameFont.Tiny;
                Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.IdeologyDescription".Translate());
                optionalFeaturesListingHeight += Text.LineHeight;
                Text.Font = GameFont.Small;
                DoSettingToggle(listingStandard.GetRect(24f), "PlayClassic".Translate(), "RandomStartMod.TooltipTitles.PlayClassicMode".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.PlayClassicModeTooltip".Translate(), ref settings.disableIdeo);
                optionalFeaturesListingHeight += 24f;
                if (!settings.disableIdeo)
                {
                    DoSettingToggle(listingStandard.GetRect(24f), "CreateFluid".Translate(), "FluidIdeoTip".Translate(), ref settings.fluidIdeo);
                    optionalFeaturesListingHeight += 24f;

                    if (settings.fluidIdeo)
                    {
                        Widgets.Label(listingStandard.GetRect(42f), "RandomStartMod.FluidMemeCountInfo".Translate());
                        optionalFeaturesListingHeight += 42f;
                    }
                    else
                    {
                        // Random Meme Range
                        listingStandard.Indent(8);
                        listingStandard.Label("RandomStartMod.Misc.RandomMemeCountLabel".Translate());
                        listingStandard.Outdent(8);
                        optionalFeaturesListingHeight += Text.LineHeight + 12f;
                        Widgets.IntRange(listingStandard.GetRect(32f), 382399866, ref settings.randomMemeRange, 1, 10);
                        optionalFeaturesListingHeight += 32f;
                        listingStandard.Gap(2f);
                        optionalFeaturesListingHeight += 2f;
                    }

                    // Ensure ideology lists are initialized
                    if (settings.forcedMemes == null) settings.forcedMemes = new List<string>();
                    if (settings.disallowedMemes == null) settings.disallowedMemes = new List<string>();
                    if (settings.disallowedPrecepts == null) settings.disallowedPrecepts = new List<string>();

                    // Forced Memes Section
                    listingStandard.Gap();
                    Text.Font = GameFont.Medium;
                    listingStandard.Label("RandomStartMod.Misc.ForcedMemesTitle".Translate());
                    listingStandard.GapLine();
                    optionalFeaturesListingHeight += 24f + Text.LineHeight;
                    Text.Font = GameFont.Small;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.ForcedMemesDescription".Translate());
                    optionalFeaturesListingHeight += Text.LineHeight;
                    Text.Font = GameFont.Small;

                    List<MemeDef> forcedMemes = new List<MemeDef>();
                    foreach (string memeDefName in settings.forcedMemes)
                    {
                        MemeDef meme = DefDatabase<MemeDef>.GetNamed(memeDefName, false);
                        if (meme != null)
                            forcedMemes.Add(meme);
                    }
                    for (int i = 0; i < forcedMemes.Count; i++)
                    {
                        listingStandard.Gap(4f);
                        if (DoMemeRow(listingStandard.GetRect(24f), forcedMemes[i], settings.forcedMemes, i))
                        {
                            forcedMemes.RemoveAt(i);
                            i--;
                        }
                        listingStandard.Gap(4f);
                        optionalFeaturesListingHeight += 32f;
                    }

                    List<FloatMenuOption> forcedMemeOptions = new List<FloatMenuOption>();
                    foreach (MemeDef memeDef in DefDatabase<MemeDef>.AllDefs.OrderBy(x => x.LabelCap.ToString()))
                    {
                        MemeDef localDef = memeDef;
                        string text = localDef.LabelCap;
                        Action action = () => settings.forcedMemes.Add(localDef.defName);

                        AcceptanceReport acceptanceReport = CanAddForcedMeme(localDef);
                        if (!acceptanceReport)
                        {
                            action = null;
                            if (!acceptanceReport.Reason.NullOrEmpty())
                            {
                                text = text + " (" + acceptanceReport.Reason + ")";
                            }
                        }

                        Texture2D icon = localDef.Icon ?? GetSourceIcon(localDef);
                        FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, icon, Color.white, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                        floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.description;
                        forcedMemeOptions.Add(floatMenuOption);
                    }
                    if (forcedMemeOptions.Count > 0)
                    {
                        if (listingStandard.ButtonText("Add".Translate().CapitalizeFirst() + "..."))
                        {
                            Find.WindowStack.Add(new FloatMenu(forcedMemeOptions));
                        }
                        optionalFeaturesListingHeight += 28f;
                    }

                    // Disallowed Memes Section
                    listingStandard.Gap();
                    Text.Font = GameFont.Medium;
                    listingStandard.Label("RandomStartMod.Misc.DisallowedMemesTitle".Translate());
                    listingStandard.GapLine();
                    optionalFeaturesListingHeight += 24f + Text.LineHeight;
                    Text.Font = GameFont.Small;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.DisallowedMemesDescription".Translate());
                    optionalFeaturesListingHeight += Text.LineHeight;
                    Text.Font = GameFont.Small;

                    List<MemeDef> disallowedMemes = new List<MemeDef>();
                    foreach (string memeDefName in settings.disallowedMemes)
                    {
                        MemeDef meme = DefDatabase<MemeDef>.GetNamed(memeDefName, false);
                        if (meme != null)
                            disallowedMemes.Add(meme);
                    }
                    for (int i = 0; i < disallowedMemes.Count; i++)
                    {
                        listingStandard.Gap(4f);
                        if (DoMemeRow(listingStandard.GetRect(24f), disallowedMemes[i], settings.disallowedMemes, i))
                        {
                            disallowedMemes.RemoveAt(i);
                            i--;
                        }
                        listingStandard.Gap(4f);
                        optionalFeaturesListingHeight += 32f;
                    }

                    List<FloatMenuOption> disallowedMemeOptions = new List<FloatMenuOption>();
                    foreach (MemeDef memeDef in DefDatabase<MemeDef>.AllDefs.OrderBy(x => x.LabelCap.ToString()))
                    {
                        if (!settings.disallowedMemes.Contains(memeDef.defName))
                        {
                            MemeDef localDef = memeDef;
                            string text = localDef.LabelCap;
                            Action action = () => settings.disallowedMemes.Add(localDef.defName);
                            Texture2D icon = localDef.Icon ?? GetSourceIcon(localDef);
                            FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, icon, Color.white, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                            floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.description;
                            disallowedMemeOptions.Add(floatMenuOption);
                        }
                    }
                    if (disallowedMemeOptions.Count > 0)
                    {
                        if (listingStandard.ButtonText("Add".Translate().CapitalizeFirst() + "..."))
                        {
                            Find.WindowStack.Add(new FloatMenu(disallowedMemeOptions));
                        }
                        optionalFeaturesListingHeight += 28f;
                    }

                    // Disallowed Precepts Section
                    listingStandard.Gap();
                    Text.Font = GameFont.Medium;
                    listingStandard.Label("RandomStartMod.Misc.DisallowedPrecepts".Translate());
                    listingStandard.GapLine();
                    optionalFeaturesListingHeight += 24f + Text.LineHeight;
                    Text.Font = GameFont.Small;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.DisallowedPreceptsDescription".Translate());
                    optionalFeaturesListingHeight += Text.LineHeight;
                    Text.Font = GameFont.Small;
                    Text.Font = GameFont.Small;

                    List<PreceptDef> disallowedPrecepts = new List<PreceptDef>();
                    foreach (string preceptDefName in settings.disallowedPrecepts)
                    {
                        PreceptDef precept = DefDatabase<PreceptDef>.GetNamed(preceptDefName, false);
                        if (precept != null)
                            disallowedPrecepts.Add(precept);
                    }
                    for (int i = 0; i < disallowedPrecepts.Count; i++)
                    {
                        listingStandard.Gap(4f);
                        if (DoPreceptRow(listingStandard.GetRect(24f), disallowedPrecepts[i], settings.disallowedPrecepts, i))
                        {
                            disallowedPrecepts.RemoveAt(i);
                            i--;
                        }
                        listingStandard.Gap(4f);
                        optionalFeaturesListingHeight += 32f;
                    }

                    List<FloatMenuOption> disallowedPreceptOptions = new List<FloatMenuOption>();
                    foreach (PreceptDef preceptDef in DefDatabase<PreceptDef>.AllDefs.OrderBy(x => x.issue?.LabelCap.ToString() ?? x.LabelCap.ToString()))
                    {
                        if (!settings.disallowedPrecepts.Contains(preceptDef.defName) && preceptDef.visibleOnAddFloatMenu)
                        {
                            PreceptDef localDef = preceptDef;
                            string text = IdeoUIUtility.GetPreceptLabel(localDef, localDef.ritualPatternBase);
                            bool flag = IdeoUIUtility.IsBasicPrecept(localDef);
                            if (flag)
                            {
                                text = localDef.issue.LabelCap + ": " + text;
                                if (localDef.issue == null || text == "")
                                {
                                    continue; // Skip if something is empty
                                }
                            }
                            Action action = () => settings.disallowedPrecepts.Add(localDef.defName);
                            Texture2D icon = localDef.Icon ?? GetSourceIcon(localDef);
                            FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, icon, Color.white, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                            floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.description;
                            disallowedPreceptOptions.Add(floatMenuOption);
                        }
                    }
                    if (disallowedPreceptOptions.Count > 0)
                    {
                        if (listingStandard.ButtonText("Add".Translate().CapitalizeFirst() + "..."))
                        {
                            Find.WindowStack.Add(new FloatMenu(disallowedPreceptOptions));
                        }
                        optionalFeaturesListingHeight += 28f;
                    }

                    if (GenFilePaths.AllCustomIdeoFiles.Any())
                    {
                        DoSettingToggle(listingStandard.GetRect(24f), "LoadExistingIdeoligion".Translate(), "RandomStartMod.TooltipTitles.LoadExistingIdeology".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.LoadExistingIdeologyTooltip".Translate(), ref settings.overrideIdeo);
                        optionalFeaturesListingHeight += 24f;
                        if (settings.overrideIdeo)
                        {

                            string text = "ModClickToSelect".Translate();
                            if (File.Exists(settings.customIdeoOverrideFile))
                            {
                                FileInfo currentFile = new FileInfo(settings.customIdeoOverrideFile);
                                text = currentFile.Name.Replace(".rid", "");
                            }
                            if (listingStandard.ButtonText(text))
                            {

                                List<FloatMenuOption> list = new List<FloatMenuOption>();
                                foreach (System.IO.FileInfo fileInfo in GenFilePaths.AllCustomIdeoFiles)
                                {
                                    text = fileInfo.Name.Replace(".rid", "");
                                    FloatMenuOption item = new FloatMenuOption(text, delegate
                                    {
                                        if (settings.customIdeoOverrideFile != fileInfo.FullPath)
                                        {
                                            settings.customIdeoOverrideFile = fileInfo.FullPath;
                                        }
                                    });
                                    list.Add(item);
                                }
                                Find.WindowStack.Add(new FloatMenu(list));
                            }
                            optionalFeaturesListingHeight += 32f;
                        }
                    }
                }

            }

            listingStandard.Gap();
            optionalFeaturesListingHeight += 12f;
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetOptionalFeatures();
            }
            optionalFeaturesListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();
        }

        AcceptanceReport CanAddForcedMeme(MemeDef memeDef)
        {
            if (settings.forcedMemes.Contains(memeDef.defName))
            {
                return "Already added";
            }

            // Check if this is a Structure meme and if there's already one in the list
            if (memeDef.category == MemeCategory.Structure)
            {
                foreach (string existingMemeDefName in settings.forcedMemes)
                {
                    MemeDef existingMeme = DefDatabase<MemeDef>.GetNamed(existingMemeDefName, false);
                    if (existingMeme != null && existingMeme.category == MemeCategory.Structure)
                    {
                        return "Only one structure meme allowed";
                    }
                }
            }

            return AcceptanceReport.WasAccepted;
        }



        public bool DoMemeRow(Rect rect, MemeDef memeDef, List<string> memeDefNames, int index)
        {
            bool result = false;
            Rect rect2 = new Rect(rect.x, rect.y - 4f, rect.width, rect.height + 8f);
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect2);
            }
            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);

            // Get icon from meme itself or source mod
            Texture2D icon = memeDef.Icon ?? GetSourceIcon(memeDef);
            GUI.color = Color.white;
            widgetRow.Icon(icon);
            GUI.color = Color.white;
            widgetRow.Gap(4f);
            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(memeDef.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                memeDefNames.Remove(memeDef.defName);
                result = true;
            }
            Widgets.EndGroup();

            if (Mouse.IsOver(rect2))
            {
                string tooltip = memeDef.LabelCap.AsTipTitle() + "\n" + memeDef.description;
                if (memeDef.modContentPack != null)
                {
                    tooltip += "\n\n" + GetSourceModName(memeDef).AsTipTitle();
                }
                TooltipHandler.TipRegion(rect2, tooltip);
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

        public bool DoPreceptRow(Rect rect, PreceptDef preceptDef, List<string> preceptDefNames, int index)
        {
            bool result = false;
            string text = IdeoUIUtility.GetPreceptLabel(preceptDef, preceptDef.ritualPatternBase);
            bool flag = IdeoUIUtility.IsBasicPrecept(preceptDef);
            if (flag)
            {
                text = preceptDef.issue.LabelCap + ": " + text;
                if (preceptDef.issue == null || text == "")
                {
                    return false;
                }
            }
            Rect rect2 = new Rect(rect.x, rect.y - 4f, rect.width, rect.height + 8f);
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect2);
            }
            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);

            // Get icon from precept itself or source mod
            Texture2D icon = preceptDef.Icon ?? GetSourceIcon(preceptDef);
            GUI.color = Color.white;
            widgetRow.Icon(icon);
            GUI.color = Color.white;
            widgetRow.Gap(4f);

            Text.Anchor = TextAnchor.MiddleCenter;

            widgetRow.Label(text);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                preceptDefNames.Remove(preceptDef.defName);
                result = true;
            }
            Widgets.EndGroup();

            if (Mouse.IsOver(rect2))
            {
                string tooltip = text.AsTipTitle() + "\n" + preceptDef.description;
                if (preceptDef.modContentPack != null)
                {
                    tooltip += "\n\n" + GetSourceModName(preceptDef).AsTipTitle();
                }
                TooltipHandler.TipRegion(rect2, tooltip);
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

    }
}
