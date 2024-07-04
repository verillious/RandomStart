using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RandomStartMod
{
    [HarmonyPatch(typeof(ScenPart_PlayerPawnsArriveMethod), "GenerateIntoMap")]
    internal class ScenPart_PlayerPawnsArriveMethod_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Map map, ScenPart_PlayerPawnsArriveMethod __instance)
        {
            if (!RandomStartData.startedFromRandom)
            {
                return true;
            }

            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();
            if (!settings.removeStartingItems && !settings.addRandomItems)
            {
                return true;
            }

            Util.LogMessage("Patching PlayerPawnsArriveMethod");
            Util.LogMessage($"Using Total Market Value Limit: {settings.randomItemTotalMarketValueLimit}");


            if (Find.GameInitData == null)
            {
                return false;
            }

            List<List<Thing>> list = new List<List<Thing>>();

            float totalRandomItemMarketValue = 0.0f;


            foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
            {
                List<Thing> list3 = new List<Thing>();
                List<Thing> list4 = new List<Thing>();
                list3.Add(startingAndOptionalPawn);
                list.Add(list3);
                foreach (ThingDefCount item in Find.GameInitData.startingPossessions[startingAndOptionalPawn])
                {
                    list4.Add(StartingPawnUtility.GenerateStartingPossession(item));
                }
                int num = 0;
                foreach (Thing item in list4)
                {
                    if (item.def.CanHaveFaction)
                    {
                        item.SetFactionDirect(Faction.OfPlayer);
                    }
                    list[num].Add(item);
                    totalRandomItemMarketValue += item.MarketValue;
                    num++;
                    if (num >= list.Count)
                    {
                        num = 0;
                    }
                }
            }


            if (!settings.removeStartingItems)
            {
                List<Thing> list2 = new List<Thing>();
                foreach (ScenPart allPart in Find.Scenario.AllParts)
                {
                    list2.AddRange(allPart.PlayerStartingThings());
                }
                int num = 0;
                foreach (Thing item2 in list2)
                {
                    if (item2.def.CanHaveFaction)
                    {
                        item2.SetFactionDirect(Faction.OfPlayer);
                    }
                    list[num].Add(item2);
                    totalRandomItemMarketValue += item2.MarketValue;
                    num++;
                    if (num >= list.Count)
                    {
                        num = 0;
                    }
                }
            }


            if (settings.addRandomItems)
            {
                int techLevelLimit = settings.randomItemTechLevelLimit;
                int num = 0;
                var attempts = 0;
                for (int i = 0; i < settings.randomItemRange.RandomInRange; i++)
                {
                    //ThingSetMakerDef thingSetMakerDef = ThingSetMakerDefOf.MapGen_DefaultStockpile;
                    //randomItems.AddRange(thingSetMakerDef.root.Generate(default(ThingSetMakerParams)));
                    attempts = 0;
                    ThingDef newThing;
                    Thing newItem;

                    IEnumerable<ThingDef> possibleItems = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.category == ThingCategory.Item && (int)x.techLevel <= techLevelLimit && (int)x.techLevel > 0);
                    if (settings.enableMarketValueLimit)
                    {
                        do
                        {
                            newThing = possibleItems.RandomElement();
                            newItem = ThingMaker.MakeThing(newThing, GenStuff.RandomStuffFor(newThing));
                            attempts++;
                        }
                        while (totalRandomItemMarketValue + (newItem.MarketValue * newThing.stackLimit) > settings.randomItemTotalMarketValueLimit && attempts <= 20);
                    }
                    else
                    {
                        newThing = possibleItems.RandomElement();
                        newItem = ThingMaker.MakeThing(newThing, GenStuff.RandomStuffFor(newThing));
                    }

                    if (attempts >= 20)
                    {
                        break;
                    }

                    for (int j = 0; j < newThing.stackLimit; j++)
                    {
                        if (newItem.def.CanHaveFaction)
                        {
                            newItem.SetFactionDirect(Faction.OfPlayer);
                        }
                        list[num].Add(newItem);
                        totalRandomItemMarketValue += newItem.MarketValue;
                    }
                    num++;
                    if (num >= list.Count)
                    {
                        num = 0;
                    }
                    if (attempts >= 20)
                    {
                        break;
                    }
                }
                Util.LogMessage($"Total item market cost: {totalRandomItemMarketValue}");
            }

            DropPodUtility.DropThingGroupsNear(MapGenerator.PlayerStartSpot, map, list, 110, Find.GameInitData.QuickStarted || __instance.method != PlayerPawnsArriveMethod.DropPods, leaveSlag: true, canRoofPunch: true, forbid: true, allowFogged: false);
            return false;
        }
    }
}
