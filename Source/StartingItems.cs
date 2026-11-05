using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RandomStartMod
{
    // Uses the normal item's generation/serialization/arrival implementation.
    public sealed class ScenPart_RandomStartThing : ScenPart_StartingThing_Defined
    {
        public ScenPart_RandomStartThing() { }
        internal static IEnumerable<ThingDef> EligibleThings(ScenPartDef definition) => new ScenPart_RandomStartThing { def = definition }.PossibleThingDefs();
        internal static ScenPart Create(ScenPartDef definition, ThingDef thing, ThingDef material, int amount)
        {
            return new ScenPart_RandomStartThing { def = definition, thingDef = thing, stuff = material, count = amount };
        }
    }

    internal static class StartingItems
    {
        internal static void Prepare(Scenario scenario, RandomStartSettings settings)
        {
            if (settings.removeStartingItems)
            {
                // Remove standard item parts only; preserve custom mod-owned parts and animals.
                scenario.parts.RemoveAll(part => part.GetType().Assembly == typeof(ScenPart).Assembly
                    && (part is ScenPart_StartingThing_Defined || part is ScenPart_ScatterThings));
            }
        }

        internal static void AddRandomItems(Scenario scenario, RandomStartSettings settings)
        {
            if (!settings.addRandomItems) return;
            ScenPartDef definition = DefDatabase<ScenPartDef>.AllDefs.FirstOrDefault(
                candidate => candidate.scenPartClass == typeof(ScenPart_StartingThing_Defined));
            if (definition == null)
                throw new InvalidOperationException("No standard starting-item scenario definition is available.");
            var pool = ScenPart_RandomStartThing.EligibleThings(definition).Where(thing =>
                (int)thing.techLevel > 0 && (int)thing.techLevel <= settings.randomItemTechLevelLimit).ToList();
            float remaining = settings.enableMarketValueLimit ? settings.randomItemTotalMarketValueLimit : float.PositiveInfinity;
            int requested = settings.randomItemRange.RandomInRange;
            for (int i = 0; i < requested && pool.Count > 0; i++)
            {
                ThingDef thing = pool.RandomElement();
                ThingDef material = GenStuff.RandomStuffFor(thing);
                float value = thing.GetStatValueAbstract(StatDefOf.MarketValue, material);
                int amount = StartingItemBudget.AffordableCount(thing.stackLimit, value, remaining);
                if (amount == 0) { pool.Remove(thing); i--; continue; }
                scenario.parts.Add(ScenPart_RandomStartThing.Create(definition, thing, material, amount));
                remaining -= value * amount;
            }
        }
    }
}
