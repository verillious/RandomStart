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

        private void DoCharactersSettingsTabContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 60f, inRect.width, planetListingHeight);
            planetListingHeight = 0f;
            Widgets.BeginScrollView(inRect, ref planetScrollPosition, rect, showScrollbars: false);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Characters".Translate());

            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Misc.StartingColonistsDescription".Translate());
            planetListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            // PrepareModerately Integration
            if (PrepareModeratelyCompat.IsAvailable)
            {
                DoSettingToggle(listingStandard.GetRect(24f), "PrepareModerately Integration", "Enable integration with PrepareModerately mod for advanced pawn filtering during random start generation. When enabled, this will hide and disable the manual age, gender, name, and violence filtering options below in favor of using PrepareModerately's filter system.", ref settings.enablePrepareModeratelyIntegration);
                planetListingHeight += 24f;

                if (settings.enablePrepareModeratelyIntegration)
                {
                    listingStandard.Gap();
                    planetListingHeight += 12f;

                    // Filter selection dropdown
                    var filterNames = PrepareModeratelyCompat.GetAvailableFilterNames();
                    if (filterNames.Count > 0)
                    {
                        var currentFilterName = settings.selectedPrepareModeratelyFilter;
                        if (string.IsNullOrEmpty(currentFilterName) || !filterNames.Contains(currentFilterName))
                        {
                            currentFilterName = filterNames.FirstOrDefault() ?? "";
                            settings.selectedPrepareModeratelyFilter = currentFilterName;
                        }

                        var labelRect = listingStandard.GetRect(24f);
                        var dropdownRect = new Rect(labelRect.x + 200f, labelRect.y, labelRect.width - 200f, labelRect.height);
                        labelRect.width = 200f;

                        Widgets.Label(labelRect, "Selected Filter:");
                        if (Widgets.ButtonText(dropdownRect, currentFilterName.NullOrEmpty() ? "None" : currentFilterName))
                        {
                            var options = new List<FloatMenuOption>();
                            foreach (var filterName in filterNames)
                            {
                                var filterDescription = PrepareModeratelyCompat.GetFilterDescription(filterName);
                                var option = new FloatMenuOption(filterName, () => {
                                    settings.selectedPrepareModeratelyFilter = filterName;
                                    PrepareModeratelyCompat.SetCurrentFilter(filterName);
                                });

                                // Add tooltip if description is available
                                if (!string.IsNullOrEmpty(filterDescription))
                                {
                                    option.tooltip = filterDescription;
                                }

                                options.Add(option);
                            }
                            Find.WindowStack.Add(new FloatMenu(options));
                        }
                        planetListingHeight += 24f;
                    }
                    else
                    {
                        Widgets.Label(listingStandard.GetRect(24f), "No PrepareModerately filters found.");
                        planetListingHeight += 24f;
                    }
                }
            }

            // Only show manual filters if PrepareModerately integration is not enabled
            if (!settings.enablePrepareModeratelyIntegration)
            {
                DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.TooltipTitles.StartingPawnsMustBeCapableOfViolence".Translate(), "RandomStartMod.TooltipTitles.StartingPawnsMustBeCapableOfViolence".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Misc.StartingPawnsMustBeCapableOfViolenceTooltip".Translate(), ref settings.startingPawnForceViolence);
                planetListingHeight += 24f;
                DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.PawnNotDisabledWorkTags".Translate(), "RandomStartMod.PawnNotDisabledWorkTags".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.PawnNotDisabledWorkTagsTooltip".Translate(), ref settings.PawnNotDisabledWorkTags);
                planetListingHeight += 24f;
            }

            // Only show the manual filters if PrepareModerately integration is not enabled
            if (!settings.enablePrepareModeratelyIntegration)
            {
                listingStandard.Gap();

                bool overrideName = !settings.randomisePawnName;
                DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.Characters.OverrideName".Translate(), "RandomStartMod.Characters.OverrideNameTooltip".Translate(), ref overrideName);
                settings.randomisePawnName = !overrideName;
                planetListingHeight += 36f;
                if (overrideName)
                {
                    // Ensure pawnNames list is initialized
                    if (settings.pawnNames == null)
                        settings.pawnNames = new List<PawnNameData>();

                    // Add space for column headers
                    if (settings.pawnNames.Count > 0)
                    {
                        listingStandard.Gap(14f); // Extra gap before headers (increased from 8f)
                        planetListingHeight += 32f; // Increased space for headers (increased from 26f)
                    }

                    // Display existing pawn name rows
                    for (int i = 0; i < settings.pawnNames.Count; i++)
                    {
                        if (i == 0)
                        {
                            listingStandard.Gap(2f); // Small gap for first row
                        }
                        else
                        {
                            listingStandard.Gap(4f); // Normal gap for subsequent rows
                        }
                        if (DoPawnNameRow(listingStandard.GetRect(32f), settings.pawnNames[i], i))
                        {
                            settings.pawnNames.RemoveAt(i);
                            i--; // Adjust index after removal
                        }
                        listingStandard.Gap(4f);
                        planetListingHeight += 40f;
                    }

                    // Add new pawn name row button
                    listingStandard.Gap();
                    if (listingStandard.ButtonText("RandomStartMod.Characters.AddPawnName".Translate()))
                    {
                        settings.pawnNames.Add(new PawnNameData());
                    }
                    planetListingHeight += 44f;
                }

                listingStandard.Gap();
                planetListingHeight += 36f;
                bool overrideAge = !settings.randomisePawnAge;
                DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.Characters.OverrideAge".Translate(), "RandomStartMod.Characters.OverrideAgeTooltip".Translate(), ref overrideAge);
                settings.randomisePawnAge = !overrideAge;
                planetListingHeight += 36f + Text.LineHeight;
                if (overrideAge)
                {
                    Rect rect9 = listingStandard.GetRect(32f);
                    Widgets.IntRange(rect9, 1623498651, ref settings.randomisePawnAgeRange, 0, 100, Util.GetAgeRangeLabelPercent(settings.randomisePawnAgeRange));
                    planetListingHeight += 32f;
                }

                listingStandard.Gap();
                planetListingHeight += 36f + Text.LineHeight;
                bool overrideSex = !settings.randomisePawnSex;
                DoSettingToggle(listingStandard.GetRect(24f), "RandomStartMod.Characters.OverrideSex".Translate(), "RandomStartMod.Characters.OverrideSexTooltip".Translate(), ref overrideSex);
                settings.randomisePawnSex = !overrideSex;
                planetListingHeight += 44f;
                if (overrideSex)
                {
                    List<string> list = Enum.GetNames(typeof(Gender)).Where(name => name != "None").ToList();
                    List<Gender> genderValues = Enum.GetValues(typeof(Gender)).Cast<Gender>().Where(g => g != Gender.None).ToList();
                    int num = settings.PawnSex;

                    // Find the index in the filtered list
                    int displayIndex = genderValues.FindIndex(g => (int)g == num);
                    if (displayIndex == -1)
                    {
                        displayIndex = 0;
                        settings.PawnSex = (int)genderValues[0];
                    }

                    if (listingStandard.ButtonText(list[displayIndex]))
                    {
                        List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            int index = i;
                            FloatMenuOption item = new FloatMenuOption(list[i], delegate
                            {
                                settings.PawnSex = (int)genderValues[index];
                            });
                            list2.Add(item);
                        }
                        Find.WindowStack.Add(new FloatMenu(list2));
                    }
                    planetListingHeight += 32f;
                }

                planetListingHeight += 36f + Text.LineHeight;
            }

            listingStandard.Gap();
            planetListingHeight += 12f;
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetStartingColonists();
            }
            planetListingHeight += 32f;
            listingStandard.End();
            Widgets.EndScrollView();
        }

        private bool DoPawnNameRow(Rect rect, PawnNameData pawnNameData, int index)
        {
            bool remove = false;

            // Create segments for the three text fields and remove button
            float buttonWidth = 24f;
            float spacing = 4f;
            float fieldWidth = (rect.width - buttonWidth - (3 * spacing)) / 3f;

            Rect firstRect = new Rect(rect.x, rect.y, fieldWidth, rect.height);
            Rect nickRect = new Rect(rect.x + fieldWidth + spacing, rect.y, fieldWidth, rect.height);
            Rect lastRect = new Rect(rect.x + (fieldWidth + spacing) * 2, rect.y, fieldWidth, rect.height);
            Rect removeRect = new Rect(rect.x + rect.width - buttonWidth, rect.y, buttonWidth, rect.height);

            // Draw labels above the text fields (only for the first row)
            if (index == 0)
            {
                Rect labelRect = new Rect(rect.x, rect.y - 18f, rect.width, 16f);
                GUI.color = Color.gray;
                Text.Font = GameFont.Tiny;

                Widgets.Label(new Rect(firstRect.x, labelRect.y, fieldWidth, 16f), "First");
                Widgets.Label(new Rect(nickRect.x, labelRect.y, fieldWidth, 16f), "Nick");
                Widgets.Label(new Rect(lastRect.x, labelRect.y, fieldWidth, 16f), "Last");

                Text.Font = GameFont.Small;
                GUI.color = Color.white;
            }

            // Draw text fields
            pawnNameData.firstName = Widgets.TextField(firstRect, pawnNameData.firstName ?? "");
            pawnNameData.nickName = Widgets.TextField(nickRect, pawnNameData.nickName ?? "");
            pawnNameData.lastName = Widgets.TextField(lastRect, pawnNameData.lastName ?? "");

            // Remove button
            if (Widgets.ButtonText(removeRect, "X"))
            {
                remove = true;
            }

            return remove;
        }

    }
}
