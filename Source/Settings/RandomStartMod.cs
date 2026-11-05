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
    public partial class RandomStartMod : Mod
    {
        RandomStartSettings settings;

        private static readonly int[] MapSizes = new int[6] { 200, 225, 250, 275, 300, 325 };
        private static readonly int[] Seasons = new int[6] { 1, 2, 3, 4, 5, 6 };
        private static readonly float[] PlanetCoverages = new float[3] { 0.3f, 0.5f, 1f };

        private static Vector2 mainScrollPosition;
        private static Vector2 factionScrollPosition;
        private static Vector2 planetScrollPosition;
        private static Vector2 scenariosScrollPosition;
        private static Vector2 startingTileScrollPosition;

        private static float mainListingHeight;
        private static float factionListingHeight;
        private static float planetListingHeight;
        private static float storytellerListingHeight;
        private static float scenarioListingHeight;
        private static float optionalFeaturesListingHeight;
        private static float startingTileListingHeight;
        private static float sectionHeightThreats = 0f;
        private static float sectionHeightGeneral = 0f;
        private static float sectionHeightPlayerTools = 0f;
        private static float sectionHeightEconomy = 0f;
        private static float sectionHeightAdaptation = 0f;
        private static float sectionHeightIdeology = 0f;
        private static float sectionHeightChildren = 0f;

        public int currentTab = 0;
        public string randomItemTotalMarketValueLimitTextBuffer;
        public RandomStartMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RandomStartSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.openedSettings = true;

            Rect tabRect = new Rect(inRect)
            {
                y = inRect.y + 40f
            };
            Rect mainRect = new Rect(inRect)
            {
                height = inRect.height - 40f,
                y = inRect.y + 40f
            };

            Widgets.DrawMenuSection(mainRect);
            List<TabRecord> tabs = new List<TabRecord>
            {
                new TabRecord("DifficultyGeneralSection".Translate(), () =>
                {
                    currentTab = 0;
                    WriteSettings();
                }, currentTab == 0),
                new TabRecord("Storyteller".Translate(), () =>
                {
                    currentTab = 3;
                    WriteSettings();
                }, currentTab == 3),
                new TabRecord("StatsReport_ScenarioFactor".Translate(), () =>
                {
                    currentTab = 4;
                    WriteSettings();
                }, currentTab == 4),
                new TabRecord("TabPlanet".Translate(), () =>
                {
                    currentTab = 2;
                    WriteSettings();
                }, currentTab == 2),
                new TabRecord("RandomStartMod.Tabs.StartingTile".Translate(), () =>
                {
                    currentTab = 6;
                    WriteSettings();
                }, currentTab == 6),
                new TabRecord("Factions".Translate(), () =>
                {
                    currentTab = 1;
                    WriteSettings();
                }, currentTab == 1),
                new TabRecord("RandomStartMod.Characters".Translate(),  () =>
                {
                    currentTab = 7;
                    WriteSettings();
                }, currentTab == 7)
        };
            if (ModsConfig.BiotechActive || ModsConfig.IdeologyActive)
            {
                tabs.Add(
                    new TabRecord("MiscRecordsCategory".Translate(), () =>
                    {
                        currentTab = 5;
                        WriteSettings();
                    }, currentTab == 5)
                );
            }
            TabDrawer.DrawTabs(tabRect, tabs);

            if (currentTab == 0)
            {
                DoDifficultySettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 1)
            {
                DoFactionSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 2)
            {
                DoPlanetSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 3)
            {
                DoStorytellerSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 4)
            {
                DoScenarioSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 5)
            {
                DoOptionalFeaturesTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 6)
            {
                DoStartingTileSettingsTabContents(mainRect.ContractedBy(15f));
            }
            else if (currentTab == 7)
            {
                DoCharactersSettingsTabContents(mainRect.ContractedBy(15f));
            }
        }

        public override string SettingsCategory()
        {
            return "RandomStartMod.Title".Translate();
        }

        public bool CheckFirstTimeDialog()
        {
            if (!settings.openedSettings)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("RandomStartMod.LaunchingWithoutConfiguring".Translate("<b>" + "RandomStartMod.Title".Translate() + "</b>"));
                sb.AppendInNewLine(" ");
                sb.AppendInNewLine("AreYouSure".Translate());
                sb.AppendInNewLine(" ");
                sb.AppendInNewLine(" ");
                sb.AppendInNewLine("RandomStartMod.SettingsAccess".Translate());
                Find.WindowStack.Add(
                    new Dialog_MessageBox(
                        sb.ToString(),
                        "ModSettings".Translate(),
                        () => Find.WindowStack.Add(new Dialog_ModSettings(this)),
                        "Start".Translate(),
                        () => LongEventHandler.QueueLongEvent(delegate
                            {
                                RandomScenario.SetupForRandomPlay();
                            }, "GeneratingMap", doAsynchronously: false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap),
                        "RandomStartMod.Title".Translate()
                        )
                    );
                settings.openedSettings = true;
                WriteSettings();
                return false;
            }
            return true;
        }
    }
}