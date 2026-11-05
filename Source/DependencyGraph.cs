using System;
using System.Collections.Generic;

namespace RandomStartMod
{
    internal static class DependencyGraph
    {
        internal static HashSet<T> Descendants<T>(T root, Func<T, IEnumerable<T>> children)
        {
            var visited = new HashSet<T> { root };
            var pending = new Stack<T>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var next = children(pending.Pop());
                if (next == null) continue;
                foreach (T child in next)
                    if (child != null && visited.Add(child)) pending.Push(child);
            }
            visited.Remove(root);
            return visited;
        }
    }
}
