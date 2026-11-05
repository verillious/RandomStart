using System;
using System.Collections.Generic;

namespace RandomStartMod
{
    internal static class DependencyGraph
    {
        internal static bool TryOrder<T>(T root, Func<T, IEnumerable<T>> children, out List<T> ordered) where T : class
        {
            ordered = new List<T>();
            var active = new HashSet<T>();
            var finished = new HashSet<T>();
            var pending = new Stack<Tuple<T, bool>>();
            pending.Push(Tuple.Create(root, false));
            while (pending.Count > 0)
            {
                var frame = pending.Pop();
                T node = frame.Item1;
                if (node == null || finished.Contains(node)) continue;
                if (frame.Item2)
                {
                    active.Remove(node);
                    finished.Add(node);
                    ordered.Add(node);
                    continue;
                }
                if (!active.Add(node)) { ordered.Clear(); return false; }
                pending.Push(Tuple.Create(node, true));
                var dependencies = children(node);
                if (dependencies != null)
                    foreach (T dependency in dependencies) pending.Push(Tuple.Create(dependency, false));
            }
            return true;
        }

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
