using System;
using System.Collections.Generic;

namespace RandomStartMod
{
    internal static class GeneBatch
    {
        internal static bool TryApply<TDef, TGene>(IEnumerable<TDef> definitions, Predicate<TDef> exists,
            Func<TDef, TGene> add, Action<TGene> remove, Func<bool> acceptable) where TGene : class
        {
            var added = new List<TGene>();
            try
            {
                foreach (TDef definition in definitions)
                {
                    if (exists(definition)) continue;
                    TGene gene = add(definition);
                    if (gene == null) throw new InvalidOperationException("Gene generation returned no gene.");
                    added.Add(gene);
                }
                if (added.Count > 0 && acceptable()) return true;
            }
            catch (Exception original)
            {
                try { Rollback(added, remove); }
                catch (Exception cleanup) { throw new AggregateException("Gene generation and rollback both failed.", original, cleanup); }
                throw;
            }
            Rollback(added, remove);
            return false;
        }

        private static void Rollback<TGene>(List<TGene> added, Action<TGene> remove)
        {
            for (int i = added.Count - 1; i >= 0; i--) remove(added[i]);
        }
    }
}
