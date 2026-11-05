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

        private void DoStartingTileSettingsTabContents(Rect inRect)
        {
            if (settings == null)
            {
                Util.LogMessage("Settings is null in DoStartingTileSettingsTabContents");
                return;
            }

            // Clean up any excluded biomes from the settings
            CleanUpExcludedBiomes();
            CleanUpInvalidHilliness();

            Rect rect = new Rect(0f, 60f, inRect.width, startingTileListingHeight);
            startingTileListingHeight = 0f;
            Widgets.BeginScrollView(inRect, ref startingTileScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.StartingTile.Title".Translate());
            listingStandard.GapLine();
            startingTileListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            listingStandard.Gap();
            startingTileListingHeight += 12f;

            // Filter Starting Biome toggle
            DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.StartingTile.FilterStartingBiome".Translate(), "RandomStartMod.TooltipTitles.FilterStartingBiome".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.StartingTile.FilterStartingBiomeTooltip".Translate(), ref settings.filterStartingBiome);
            startingTileListingHeight += 24f;

            if (settings.filterStartingBiome)
            {
                listingStandard.Gap();
                listingStandard.Label("RandomStartMod.StartingTile.AllowedBiomes".Translate());
                startingTileListingHeight += 12f + Text.LineHeight;

                // Ensure allowedBiomes list is not null
                if (settings.allowedBiomes == null)
                {
                    settings.allowedBiomes = new List<string>();
                }

                // Display current allowed biomes (only those with canBuildBase=true)
                List<BiomeDef> biomesAllowed = new List<BiomeDef>();
                foreach (string biomeDefName in settings.allowedBiomes)
                {
                    BiomeDef biome = DefDatabase<BiomeDef>.GetNamed(biomeDefName, false);
                    if (biome == null || !biome.canBuildBase || !biome.canAutoChoose)
                        continue;
                    biomesAllowed.Add(biome);
                }
                for (int i = 0; i < biomesAllowed.Count; i++)
                {
                    listingStandard.Gap(4f);
                    if (DoBiomeRow(listingStandard.GetRect(24f), biomesAllowed[i], settings.allowedBiomes, i))
                    {
                        biomesAllowed.RemoveAt(i);
                        i--;
                    }
                    listingStandard.Gap(4f);
                    startingTileListingHeight += 32f;
                }

                if (biomesAllowed.Count == 0)
                {
                    listingStandard.Label("RandomStartMod.StartingTile.NoBiomesSelected".Translate());
                    startingTileListingHeight += Text.LineHeight;
                }

                listingStandard.Gap();
                startingTileListingHeight += 12f;

                // Add biome button
                if (listingStandard.ButtonText("RandomStartMod.StartingTile.AddBiome".Translate()))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    // Manually filter out underground biomes that don't appear on planet surface
                    string[] excludedBiomes = { "Underground", "Labyrinth", "MetalHell", "Undercave" };
                    IEnumerable<BiomeDef> availableBiomes = DefDatabase<BiomeDef>.AllDefs.Where(b => b != null && b.canBuildBase && b.implemented && b.canAutoChoose && !excludedBiomes.Contains(b.defName));

                    foreach (BiomeDef biomeDef in availableBiomes)
                    {
                        BiomeDef localDef = biomeDef;
                        string text = localDef.LabelCap;
                        Action action = delegate
                        {
                            settings.allowedBiomes.Add(localDef.defName);
                        };
                        AcceptanceReport acceptanceReport = CanAddBiome(localDef);
                        if (!acceptanceReport)
                        {
                            action = null;
                            if (!acceptanceReport.Reason.NullOrEmpty())
                            {
                                text = text + " (" + acceptanceReport.Reason + ")";
                            }
                        }
                        else
                        {
                            int num = biomesAllowed.Count((BiomeDef x) => x == localDef);
                            if (num > 0)
                            {
                                text = text + " (" + num + ")";
                            }
                        }
                        Texture2D biomeIcon = BaseContent.BadTex; // Simple fallback for now
                        if (!biomeDef.texture.NullOrEmpty())
                        {
                            biomeIcon = ContentFinder<Texture2D>.Get(biomeDef.texture, false) ?? BaseContent.BadTex;
                        }
                        FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, biomeIcon, Color.white, MenuOptionPriority.Default, null, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, localDef), null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true);
                        floatMenuOption.tooltip = text.AsTipTitle() + "\n" + localDef.description;
                        list.Add(floatMenuOption);
                    }

                    if (list.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }
                startingTileListingHeight += 32f + 12f;
            }

            listingStandard.Gap();
            startingTileListingHeight += 12f;

            // Filter Starting Hilliness toggle
            DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.StartingTile.FilterStartingHilliness".Translate(), "RandomStartMod.TooltipTitles.FilterStartingHilliness".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.StartingTile.FilterStartingHillinessTooltip".Translate(), ref settings.filterStartingHilliness);
            startingTileListingHeight += 24f;

            if (settings.filterStartingHilliness)
            {
                listingStandard.Gap();
                listingStandard.Label("RandomStartMod.StartingTile.AllowedHilliness".Translate());
                startingTileListingHeight += 12f + Text.LineHeight;

                // Ensure allowedHilliness list is not null
                if (settings.allowedHilliness == null)
                {
                    settings.allowedHilliness = new List<int>();
                }

                // Display current allowed hilliness types
                List<int> hillinessTypes = new List<int> { 1, 2, 3, 4 }; // Flat, SmallHills, LargeHills, Mountainous
                List<int> hillinessAllowed = new List<int>();
                foreach (int hillinessValue in settings.allowedHilliness)
                {
                    if (hillinessTypes.Contains(hillinessValue))
                    {
                        hillinessAllowed.Add(hillinessValue);
                    }
                }

                for (int i = 0; i < hillinessAllowed.Count; i++)
                {
                    listingStandard.Gap(4f);
                    if (DoHillinessRow(listingStandard.GetRect(24f), hillinessAllowed[i], settings.allowedHilliness, i))
                    {
                        hillinessAllowed.RemoveAt(i);
                        i--;
                    }
                    listingStandard.Gap(4f);
                    startingTileListingHeight += 32f;
                }

                if (hillinessAllowed.Count == 0)
                {
                    listingStandard.Label("RandomStartMod.StartingTile.NoHillinessSelected".Translate());
                    startingTileListingHeight += Text.LineHeight;
                }

                listingStandard.Gap();
                startingTileListingHeight += 12f;

                // Add hilliness button
                if (listingStandard.ButtonText("RandomStartMod.StartingTile.AddHilliness".Translate()))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    List<int> availableHilliness = new List<int> { 1, 2, 3, 4 }; // Flat, SmallHills, LargeHills, Mountainous

                    foreach (int hillinessValue in availableHilliness)
                    {
                        int localValue = hillinessValue;
                        string hillinessKey = "";
                        switch (hillinessValue)
                        {
                            case 1: hillinessKey = "Hilliness_Flat"; break;
                            case 2: hillinessKey = "Hilliness_SmallHills"; break;
                            case 3: hillinessKey = "Hilliness_LargeHills"; break;
                            case 4: hillinessKey = "Hilliness_Mountainous"; break;
                        }

                        string text = hillinessKey.Translate();
                        Action action = delegate
                        {
                            settings.allowedHilliness.Add(localValue);
                        };

                        AcceptanceReport acceptanceReport = CanAddHilliness(localValue);
                        if (!acceptanceReport)
                        {
                            action = null;
                            if (!acceptanceReport.Reason.NullOrEmpty())
                            {
                                text = text + " (" + acceptanceReport.Reason + ")";
                            }
                        }
                        else
                        {
                            int num = hillinessAllowed.Count((int x) => x == localValue);
                            if (num > 0)
                            {
                                text = text + " (" + num + ")";
                            }
                        }

                        FloatMenuOption floatMenuOption = new FloatMenuOption(text, action, BaseContent.BadTex, Color.white);
                        list.Add(floatMenuOption);
                    }

                    if (list.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }
                startingTileListingHeight += 32f + 12f;
            }

            listingStandard.Gap();
            startingTileListingHeight += 36f + Text.LineHeight;
            Text.Font = GameFont.Small;
            DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.StartingTile.FilterStartingTemperature".Translate(), "RandomStartMod.TooltipTitles.FilterStartingTemperature".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.StartingTile.FilterStartingTemperatureTooltip".Translate(), ref settings.limitStartingTileTemperature);
            startingTileListingHeight += 36f + Text.LineHeight;
            if (settings.limitStartingTileTemperature)
            {
                Widgets.FloatRange(listingStandard.GetRect(32f), 1623498652, ref settings.limitStartingTileTemperatureRange, -50f, 50f);
                startingTileListingHeight += 32f + 12f;
            }

            listingStandard.Gap();
            startingTileListingHeight += 12f;

            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetStartingTile();
            }
            startingTileListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();
        }

        public bool DoBiomeRow(Rect rect, BiomeDef biomeDef, List<string> biomeDefNames, int index)
        {
            bool result = false;
            Rect rect2 = new Rect(rect.x, rect.y - 4f, rect.width, rect.height + 8f);
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect2);
            }
            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);

            // Draw biome icon if available, otherwise use a default texture
            Texture2D biomeIcon = BaseContent.BadTex; // Simple fallback for now
            if (!biomeDef.texture.NullOrEmpty())
            {
                biomeIcon = ContentFinder<Texture2D>.Get(biomeDef.texture, false) ?? BaseContent.BadTex;
            }
            GUI.color = Color.white;
            widgetRow.Icon(biomeIcon);
            widgetRow.Gap(4f);

            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(biomeDef.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                biomeDefNames.Remove(biomeDef.defName);
                result = true;
            }
            Widgets.EndGroup();

            if (Mouse.IsOver(rect2))
            {
                string tooltip = biomeDef.LabelCap.AsTipTitle() + "\n" + biomeDef.description;
                if (biomeDef.modContentPack != null)
                {
                    tooltip += "\n\n" + GetSourceModName(biomeDef).AsTipTitle();
                }
                TooltipHandler.TipRegion(rect2, tooltip);
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

        public bool DoHillinessRow(Rect rect, int hillinessValue, List<int> hillinessValues, int index)
        {
            bool result = false;
            Rect rect2 = new Rect(rect.x, rect.y - 4f, rect.width, rect.height + 8f);
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect2);
            }
            Widgets.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(6f, 0f);

            GUI.color = Color.white;
            widgetRow.Icon(BaseContent.BadTex); // Simple icon for hilliness
            widgetRow.Gap(4f);

            // Get the translated hilliness name
            string hillinessName = "";
            switch (hillinessValue)
            {
                case 1: hillinessName = "Hilliness_Flat".Translate(); break;
                case 2: hillinessName = "Hilliness_SmallHills".Translate(); break;
                case 3: hillinessName = "Hilliness_LargeHills".Translate(); break;
                case 4: hillinessName = "Hilliness_Mountainous".Translate(); break;
                default: hillinessName = "Unknown"; break;
            }

            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(hillinessName);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonImage(new Rect(rect.width - 24f - 6f, 0f, 24f, 24f), TexButton.Delete))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                hillinessValues.Remove(hillinessValue);
                result = true;
            }
            Widgets.EndGroup();

            if (Mouse.IsOver(rect2))
            {
                string tooltip = hillinessName.AsTipTitle();
                TooltipHandler.TipRegion(rect2, tooltip);
                Widgets.DrawHighlight(rect2);
            }
            return result;
        }

        private void CleanUpExcludedBiomes()
        {
            if (settings.allowedBiomes == null)
                return;

            // Remove any excluded biomes from the settings
            string[] excludedBiomes = { "Underground", "Labyrinth", "MetalHell", "Undercave" };

            List<string> biomesToRemove = new List<string>();
            foreach (string biomeName in settings.allowedBiomes)
            {
                if (excludedBiomes.Contains(biomeName))
                {
                    biomesToRemove.Add(biomeName);
                }
            }

            foreach (string biomeToRemove in biomesToRemove)
            {
                settings.allowedBiomes.Remove(biomeToRemove);
            }
        }

        private void CleanUpInvalidHilliness()
        {
            if (settings.allowedHilliness == null)
                return;

            // Remove any invalid hilliness values (only allow 1-4: Flat, SmallHills, LargeHills, Mountainous)
            List<int> validHilliness = new List<int> { 1, 2, 3, 4 };

            List<int> hillinessToRemove = new List<int>();
            foreach (int hillinessValue in settings.allowedHilliness)
            {
                if (!validHilliness.Contains(hillinessValue))
                {
                    hillinessToRemove.Add(hillinessValue);
                }
            }

            foreach (int hillinessToRemove_item in hillinessToRemove)
            {
                settings.allowedHilliness.Remove(hillinessToRemove_item);
            }
        }

        AcceptanceReport CanAddBiome(BiomeDef b)
        {
            if (settings.allowedBiomes.Contains(b.defName))
            {
                return "Already added";
            }

            // Manually filter out underground biomes that don't appear on planet surface
            string[] excludedBiomes = { "Underground", "Labyrinth", "MetalHell", "Undercave" };
            if (excludedBiomes.Contains(b.defName))
            {
                return "Not available on planet surface";
            }

            return AcceptanceReport.WasAccepted;
        }

        AcceptanceReport CanAddHilliness(int hillinessValue)
        {
            if (settings.allowedHilliness.Contains(hillinessValue))
            {
                return "Already added";
            }

            return AcceptanceReport.WasAccepted;
        }

    }
}
