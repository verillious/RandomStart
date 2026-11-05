using System;
using System.Collections.Generic;

namespace RandomStartMod
{
    internal static class AdditiveOptions
    {
        internal static void InsertBefore<T>(IList<T> options, Predicate<T> anchor, Predicate<T> owned, Func<T> create)
        {
            for (int i = 0; i < options.Count; i++) if (owned(options[i])) return;
            for (int i = 0; i < options.Count; i++)
                if (anchor(options[i])) { options.Insert(i, create()); return; }
        }

        internal static IEnumerable<T> AppendOnce<T>(IEnumerable<T> options, Predicate<T> owned, Func<T> create)
        {
            bool found = false;
            foreach (T option in options)
            {
                if (owned(option)) found = true;
                yield return option;
            }
            if (!found) yield return create();
        }
    }
}
