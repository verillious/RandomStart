using System;
using System.Collections.Generic;

namespace RandomStartMod
{
    internal sealed class StartingTileFilters<TBiome, THilliness>
    {
        private readonly HashSet<TBiome> biomes;
        private readonly HashSet<THilliness> hills;
        private readonly bool limitTemperature;
        private readonly float minTemperature, maxTemperature;
        internal bool Active => limitTemperature || biomes.Count > 0 || hills.Count > 0;

        internal StartingTileFilters(IEnumerable<TBiome> allowedBiomes, IEnumerable<THilliness> allowedHills,
            bool limitTemperature, float minTemperature, float maxTemperature)
        {
            biomes = new HashSet<TBiome>(allowedBiomes);
            hills = new HashSet<THilliness>(allowedHills);
            this.limitTemperature = limitTemperature;
            this.minTemperature = minTemperature;
            this.maxTemperature = maxTemperature;
        }

        internal bool Matches(TBiome biome, THilliness hilliness, float temperature) =>
            (biomes.Count == 0 || biomes.Contains(biome))
            && (hills.Count == 0 || hills.Contains(hilliness))
            && (!limitTemperature || (temperature >= minTemperature && temperature <= maxTemperature));
    }

    internal static class RandomSelection
    {
        // Random candidates without replacement: full cheap filtering, but only
        // validate settlement placement until a usable candidate is found.
        internal static bool TryChooseIndex(int count, Predicate<int> matches, Predicate<int> usable,
            Func<int, int> next, out int selected)
        {
            var candidates = new List<int>();
            for (int index = 0; index < count; index++) if (matches(index)) candidates.Add(index);
            while (candidates.Count > 0)
            {
                int slot = next(candidates.Count);
                int index = candidates[slot];
                if (usable(index)) { selected = index; return true; }
                candidates[slot] = candidates[candidates.Count - 1];
                candidates.RemoveAt(candidates.Count - 1);
            }
            selected = -1;
            return false;
        }
    }
}
