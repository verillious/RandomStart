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

        private void DoPlanetSettingsTabContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 60f, inRect.width, planetListingHeight);
            planetListingHeight = 0;
            Widgets.BeginScrollView(inRect, ref planetScrollPosition, rect, false);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            //Planet Coverage
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Planet.CoverageTitle".Translate());
            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Planet.CoverageDescription".Translate());
            planetListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            if (listingStandard.ButtonText(settings.planetCoverage.ToStringPercent()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (float coverage in PlanetCoverages)
                {
                    string text = coverage.ToStringPercent();
                    if (coverage <= 0.1f)
                    {
                        text += " (dev)";
                    }
                    FloatMenuOption item = new FloatMenuOption(text, delegate
                    {
                        if (settings.planetCoverage != coverage)
                        {
                            settings.planetCoverage = coverage;
                            if (settings.planetCoverage == 1f)
                            {
                                Messages.Message("MessageMaxPlanetCoveragePerformanceWarning".Translate(), MessageTypeDefOf.CautionInput, historical: false);
                            }
                        }
                    });
                    list.Add(item);
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            planetListingHeight += 32f;

            //Map Size
            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Planet.MapSizeTitle".Translate());
            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Planet.MapSizeDescription".Translate());
            planetListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;

            IEnumerable<int> enumerable = MapSizes.AsEnumerable();
            if (listingStandard.ButtonText("MapSizeDesc".Translate(settings.mapSize, settings.mapSize * settings.mapSize)))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (int size in enumerable)
                {
                    string text = "MapSizeDesc".Translate(size, size * size);
                    FloatMenuOption item = new FloatMenuOption(text, delegate
                    {
                        if (settings.mapSize != size)
                        {
                            settings.mapSize = size;
                        }
                    });
                    list.Add(item);
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            planetListingHeight += 32f;

            listingStandard.Gap();
            Text.Font = GameFont.Medium;
            listingStandard.Label("RandomStartMod.Planet.GenerationTitle".Translate());
            listingStandard.GapLine();
            planetListingHeight += 24f + Text.LineHeight;
            Text.Font = GameFont.Small;
            Text.Font = GameFont.Tiny;
            Widgets.Label(listingStandard.GetRect(Text.LineHeight), "RandomStartMod.Planet.GenerationDescription".Translate());
            planetListingHeight += Text.LineHeight;
            Text.Font = GameFont.Small;
            listingStandard.Gap();
            planetListingHeight += 12f;
            listingStandard.Label("RandomStartMod.Planet.RandomizeLabel".Translate());
            listingStandard.Gap();
            planetListingHeight += 12f;

            DoSettingToggle(listingStandard.GetRect(24f), "WorldSeed".Translate(), "RandomStartMod.TooltipTitles.RandomiseWorldSeed".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomiseWorldSeedTooltip".Translate(), ref settings.randomiseWorldSeed);
            planetListingHeight += 24f + Text.LineHeight;

            if (!settings.randomiseWorldSeed)
            {
                listingStandard.Gap(5f);
                Rect worldSeedRect = listingStandard.GetRect(30f);
                settings.worldSeed = Widgets.TextField(worldSeedRect, settings.worldSeed);
                planetListingHeight += 35f;
                listingStandard.Gap(2f);
                if (listingStandard.ButtonText("RandomizeSeed".Translate()))
                {
                    settings.worldSeed = GenText.RandomSeedString();
                }
                planetListingHeight += 34f;
            }

            listingStandard.Gap();

            DoSettingToggle(listingStandard.GetRect(24f), "PlanetRainfall".Translate(), "RandomStartMod.TooltipTitles.RandomizeRainfall".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizeRainfallTooltip".Translate(), ref settings.randomiseRainfall);
            planetListingHeight += 24f + Text.LineHeight;


            if (!settings.randomiseRainfall)
            {
                listingStandard.Gap(2f);
                Rect rainfallRect = listingStandard.GetRect(30f);
                if (ModsConfig.IsActive("koth.RealisticPlanets1.6") || ModsConfig.IsActive("koth.RealisticPlanets1.6_Steam"))
                {
                    settings.rainfall = Mathf.RoundToInt(Widgets.HorizontalSlider(rainfallRect, settings.rainfall, 0, 4, middleAlignment: true, Util.GetIntLabelShort(settings.rainfall), null, null, 1f));
                }
                else
                {
                    settings.rainfall = Mathf.RoundToInt(Widgets.HorizontalSlider(rainfallRect, settings.rainfall, 0, OverallRainfallUtility.EnumValuesCount - 1, middleAlignment: true, Util.GetIntLabel(settings.rainfall), null, null, 1f));
                }
                planetListingHeight += 32f;
            }
            else
            {
                Rect rainfallRect = listingStandard.GetRect(32f);
                if (ModsConfig.IsActive("koth.RealisticPlanets1.6") || ModsConfig.IsActive("koth.RealisticPlanets1.6_Steam"))
                {
                    Widgets.IntRange(rainfallRect, 1623498654, ref settings.randomiseRainfallRange, 0, 4, Util.GetIntRangeLabelShort(settings.randomiseRainfallRange));
                }
                else
                {
                    Widgets.IntRange(rainfallRect, 1623498654, ref settings.randomiseRainfallRange, 0, OverallRainfallUtility.EnumValuesCount - 1, Util.GetIntRangeLabel(settings.randomiseRainfallRange));
                }
                planetListingHeight += 32f;
            }

            listingStandard.Gap();

            DoSettingToggle(listingStandard.GetRect(24f), "PlanetTemperature".Translate(), "RandomStartMod.TooltipTitles.RandomizeTemperature".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizeTemperatureTooltip".Translate(), ref settings.randomiseTemperature);
            planetListingHeight += 12f + 24;

            if (!settings.randomiseTemperature)
            {
                listingStandard.Gap(2f);
                Rect temperatureRect = listingStandard.GetRect(30f);
                settings.temperature = Mathf.RoundToInt(Widgets.HorizontalSlider(temperatureRect, (float)settings.temperature, 0f, OverallTemperatureUtility.EnumValuesCount - 1, middleAlignment: true, Util.GetIntLabel(settings.temperature), null, null, 1f));
                planetListingHeight += 32f;
            }
            else
            {
                Rect temperatureRect = listingStandard.GetRect(32f);
                Widgets.IntRange(temperatureRect, 1623498655, ref settings.randomiseTemperatureRange, 0, 6, Util.GetIntRangeLabel(settings.randomiseTemperatureRange));
                planetListingHeight += 32f;
            }

            listingStandard.Gap();
            DoSettingToggle(listingStandard.GetRect(24f), "PlanetPopulation".Translate(), "RandomStartMod.TooltipTitles.RandomizePopulation".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizePopulationTooltip".Translate(), ref settings.randomisePopulation);
            planetListingHeight += 12f + 24f;

            if (!settings.randomisePopulation)
            {
                listingStandard.Gap(2f);
                Rect populationRect = listingStandard.GetRect(30f);
                settings.population = Mathf.RoundToInt(Widgets.HorizontalSlider(populationRect, (float)settings.population, 0f, OverallPopulationUtility.EnumValuesCount - 1, middleAlignment: true, Util.GetIntLabel(settings.population), null, null, 1f));
                planetListingHeight += 32f;
            }
            else
            {
                Rect populationRect = listingStandard.GetRect(32f);
                Widgets.IntRange(populationRect, 1623498656, ref settings.randomisePopulationRange, 0, 6, Util.GetIntRangeLabel(settings.randomisePopulationRange));
                planetListingHeight += 32f;
            }

            listingStandard.Gap();
            DoSettingToggle(listingStandard.GetRect(24f), "PlanetLandmarkDensity".Translate(), "RandomStartMod.TooltipTitles.RandomizeLandmarkDensity".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizeLandmarkDensityTooltip".Translate(), ref settings.randomiseLandmarkDensity);
            planetListingHeight += 12f + 24f;

            if (!settings.randomiseLandmarkDensity)
            {
                listingStandard.Gap(2f);
                Rect landmarkRect = listingStandard.GetRect(30f);
                settings.landmarkDensity = Mathf.RoundToInt(Widgets.HorizontalSlider(landmarkRect, (float)settings.landmarkDensity, 0f, LandmarkDensityUtility.EnumValuesCount - 1, middleAlignment: true, Util.GetIntLabel(settings.landmarkDensity), null, null, 1f));
                planetListingHeight += 32f;
            }
            else
            {
                Rect landmarkRect = listingStandard.GetRect(32f);
                Widgets.IntRange(landmarkRect, 1623498668, ref settings.randomiseLandmarkDensityRange, 0, LandmarkDensityUtility.EnumValuesCount - 1, Util.GetIntRangeLabel(settings.randomiseLandmarkDensityRange));
                planetListingHeight += 32f;
            }

            listingStandard.Gap();
            DoSettingToggle(listingStandard.GetRect(24f), "Pollution_Label".Translate(), "RandomStartMod.TooltipTitles.RandomizePollution".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizePollutionTooltip".Translate(), ref settings.randomisePollution);
            planetListingHeight += 12f + 24f;

            if (!settings.randomisePollution)
            {
                listingStandard.Gap(2f);
                Rect pollutionRect = listingStandard.GetRect(30f);
                settings.pollution = Widgets.HorizontalSlider(pollutionRect, settings.pollution, 0f, 1f, middleAlignment: true, settings.pollution.ToStringPercent(), null, null, 0.05f);
                planetListingHeight += 32f;
            }
            else
            {
                Rect pollutionRect = listingStandard.GetRect(32f);
                Widgets.FloatRange(pollutionRect, 1623498651, ref settings.randomisePollutionRange, 0.0f, 1.0f, Util.GetFloatRangeLabelPercent(settings.randomisePollutionRange));
                planetListingHeight += 32f;
            }

            listingStandard.Gap();

            DoSettingToggle(listingStandard.GetRect(24f), "MapStartSeason".Translate(), "RandomStartMod.TooltipTitles.RandomizeStartingSeason".Translate().AsTipTitle() + "\n\n" + "RandomStartMod.Planet.RandomizeStartingSeasonTooltip".Translate(), ref settings.randomiseSeason);
            planetListingHeight += 12f + 32f;

            if (!settings.randomiseSeason)
            {
                listingStandard.Gap();
                IEnumerable<int> seasons = Seasons.AsEnumerable();
                Season settingSeason = (Season)settings.startingSeason;
                if (listingStandard.ButtonText(settingSeason.LabelCap()))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (int season in seasons)
                    {
                        Season labelSeason = (Season)season;
                        string text = labelSeason.LabelCap();
                        FloatMenuOption item = new FloatMenuOption(text, delegate
                        {
                            if (settings.startingSeason != season)
                            {
                                settings.startingSeason = season;
                            }
                        });
                        list.Add(item);
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                planetListingHeight += 32f;
            }

            if (ModsConfig.IsActive("Oblitus.MyLittlePlanet") || ModsConfig.IsActive("Oblitus.MyLittlePlanet_Steam"))
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("My Little Planet");
                listingStandard.GapLine();
                planetListingHeight += 12f + 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                listingStandard.Label("MLPWorldPlanetSize".Translate());
                planetListingHeight += Text.LineHeight;
                Rect mlpRect = listingStandard.GetRect(30f);
                Compat.MLPCompat.DrawMLPSlider(mlpRect);
                planetListingHeight += 30f;
            }

            if (ModsConfig.IsActive("koth.RealisticPlanets1.6") || ModsConfig.IsActive("koth.RealisticPlanets1.6_Steam"))
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("Realistic Planets");
                listingStandard.GapLine();
                planetListingHeight += 36f + Text.LineHeight;
                Text.Font = GameFont.Small;
                listingStandard.Label("RandomStartMod.Planet.RandomizeLabel".Translate());
                listingStandard.Gap();
                planetListingHeight += 12f;
                DoSettingToggle(listingStandard.GetRect(24f), "Planets.WorldPresets".Translate(), null, ref settings.randomiseRealisticPlanets);
                TooltipHandler.TipRegion(new Rect(0f, planetListingHeight - 18, inRect.width, 30), "RandomStartMod.RandomiseWorldPresetsTip".Translate());
                planetListingHeight += 44f;
                if (!settings.randomiseRealisticPlanets)
                {
                    RealisticPlanetsCompat.DoWorldTypeSelectionButton(listingStandard);
                    planetListingHeight += 32f;
                }
                listingStandard.Gap();
                DoSettingToggle(listingStandard.GetRect(24f), "Planets.OceanType".Translate(), null, ref settings.randomiseOceanType);
                planetListingHeight += 36f;
                if (!settings.randomiseOceanType)
                {
                    listingStandard.Gap(2f);
                    Rect rect4 = listingStandard.GetRect(30f);
                    settings.realisticPlanetsOceanType = Mathf.RoundToInt(Widgets.HorizontalSlider(rect4, settings.realisticPlanetsOceanType, 0, RealisticPlanetsCompat.GetWorldTypeEnumCount() - 1, middleAlignment: true, Util.GetIntLabel(settings.realisticPlanetsOceanType), null, null, 1f));
                    planetListingHeight += 32f;
                }
                else
                {
                    Rect rect5 = listingStandard.GetRect(32f);
                    Widgets.IntRange(rect5, 1623498657, ref settings.randomiseOceanTypeRange, 0, RealisticPlanetsCompat.GetWorldTypeEnumCount() - 1, Util.GetIntRangeLabel(settings.randomiseOceanTypeRange));
                    planetListingHeight += 32f;
                }
                DoSettingToggle(listingStandard.GetRect(24f), "Planets.AxialTilt".Translate(), null, ref settings.randomiseAxialTilt);
                planetListingHeight += 36f;
                if (!settings.randomiseAxialTilt)
                {
                    listingStandard.Gap(2f);
                    Rect rect4 = listingStandard.GetRect(30f);
                    settings.realisticPlanetsAxialTilt = Mathf.RoundToInt(Widgets.HorizontalSlider(rect4, settings.realisticPlanetsAxialTilt, 0, RealisticPlanetsCompat.GetAxialTiltEnumCount() - 1, middleAlignment: true, Util.GetIntLabelShort(settings.realisticPlanetsAxialTilt), null, null, 1f));
                    planetListingHeight += 32f;
                }
                else
                {
                    Rect rect5 = listingStandard.GetRect(32f);
                    Widgets.IntRange(rect5, 1623498659, ref settings.randomiseAxialTiltRange, 0, RealisticPlanetsCompat.GetAxialTiltEnumCount() - 1, Util.GetIntRangeLabelShort(settings.randomiseAxialTiltRange));
                    planetListingHeight += 32f;
                }
                listingStandard.Gap();
            }

            if (ModsConfig.IsActive("Woolstrand.RealRuins") || ModsConfig.IsActive("Woolstrand.RealRuins_Steam"))
            {
                listingStandard.Gap();
                Text.Font = GameFont.Medium;
                listingStandard.Label("Real Ruins");
                listingStandard.GapLine();
                planetListingHeight += 12f + 24f + Text.LineHeight;
                Text.Font = GameFont.Small;
                DoSettingToggle(listingStandard.GetRect(24f), "RunInBackground".Translate(), "Auto Real Ruins".AsTipTitle() + "\n\n" + "Automatically enable Real Ruins generation to populate the world with ruins from other players' colonies, adding variety and exploration opportunities.", ref settings.enableAutoRealRuins);
                planetListingHeight += 24f;
                DoSettingToggle(listingStandard.GetRect(24f), "RealRuins.BiomeFiltering".Translate(), "Real Ruins Biome Filtering".AsTipTitle() + "\n\n" + "Filter Real Ruins based on biome compatibility, ensuring ruins appear in appropriate environments that match their original settings.", ref settings.realRuinsBiomeFilter);
                planetListingHeight += 24f;
            }

            listingStandard.Gap();
            planetListingHeight += 12f;
            if (listingStandard.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                settings.ResetPlanet();
            }
            planetListingHeight += 32f;

            listingStandard.End();
            Widgets.EndScrollView();
        }

    }
}
