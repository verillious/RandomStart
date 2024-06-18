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
            if(!RandomStartData.startedFromRandom)
            {
                return true;
            }

            RandomStartSettings settings = LoadedModManager.GetMod<RandomStartMod>().GetSettings<RandomStartSettings>();
            int techLevelLimit = settings.randomItemTechLevelLimit;
            if (!settings.removeStartingItems && !settings.addRandomItems)
            {
                return true;
            }

            if (Find.GameInitData == null)
            {
                return false;
            }
            List<List<Thing>> list = new List<List<Thing>>();

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
                    num++;
                    if (num >= list.Count)
                    {
                        num = 0;
                    }
                }
            }

            if (settings.addRandomItems)
            {
                int num = 0;
                for (int i = 0; i < settings.randomItemRange.RandomInRange; i++)
                {
                    Util.LogMessage($"Creating Item with tech level less than {(TechLevel)techLevelLimit}");
                    //ThingSetMakerDef thingSetMakerDef = ThingSetMakerDefOf.MapGen_DefaultStockpile;
                    //randomItems.AddRange(thingSetMakerDef.root.Generate(default(ThingSetMakerParams)));
                    ThingDef newThing = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.category == ThingCategory.Item && (int)x.techLevel <= techLevelLimit && (int)x.techLevel > 0).RandomElement();
                    Util.LogMessage($"Created {newThing.LabelCap}, tech level {newThing.techLevel}");
                    for (int j = 0; j < newThing.stackLimit; j++)
                    {
                        Thing newItem = ThingMaker.MakeThing(newThing, GenStuff.RandomStuffFor(newThing));
                        if (newItem.def.CanHaveFaction)
                        {
                            newItem.SetFactionDirect(Faction.OfPlayer);
                        }
                        list[num].Add(newItem);
                    }
                    num++;
                    if (num >= list.Count)
                    {
                        num = 0;
                    }
                }
            }



            DropPodUtility.DropThingGroupsNear(MapGenerator.PlayerStartSpot, map, list, 110, Find.GameInitData.QuickStarted || __instance.method != PlayerPawnsArriveMethod.DropPods, leaveSlag: true, canRoofPunch: true, forbid: true, allowFogged: false);
            return false;
        }
    }
}
