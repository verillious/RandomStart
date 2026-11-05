using System;
using System.Collections.Generic;

namespace RandomStartMod
{
    internal static class Pairwise
    {
        internal static void ForEach<T>(IReadOnlyList<T> items, Predicate<T> excluded, Action<T, T> update)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (excluded(items[i])) continue;
                for (int j = i + 1; j < items.Count; j++)
                    if (!excluded(items[j])) update(items[i], items[j]);
            }
        }
    }
}
