using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinder
{
    class Node
    {
        public Vector2Int pos;
        public Node parent;
        public int g; // cost from start
        public int h; // heuristic to goal
        public int f => g + h;
    }

    static int Manhattan(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    public static bool FindPath(GridNav grid, Vector2Int start, Vector2Int goal, List<Vector2Int> outPath)
    {
        outPath.Clear();
        if (!grid.Walkable(start) || !grid.Walkable(goal)) return false;

        var open = new List<Node>();
        var all = new Dictionary<Vector2Int, Node>();
        var closed = new HashSet<Vector2Int>();

        Node startNode = new Node { pos = start, g = 0, h = Manhattan(start, goal), parent = null };
        open.Add(startNode);
        all[start] = startNode;

        while (open.Count > 0)
        {
            // Find node with smallest f
            int bestIdx = 0;
            for (int i = 1; i < open.Count; i++)
                if (open[i].f < open[bestIdx].f) bestIdx = i;

            var cur = open[bestIdx];
            open.RemoveAt(bestIdx);

            if (cur.pos == goal)
            {
                // Reconstruct
                var n = cur;
                var rev = new List<Vector2Int>();
                while (n != null) { rev.Add(n.pos); n = n.parent; }
                rev.Reverse();
                outPath.AddRange(rev);
                return true;
            }

            closed.Add(cur.pos);

            foreach (var npos in grid.Neighbors4(cur.pos))
            {
                if (closed.Contains(npos)) continue;

                int tentativeG = cur.g + 1; // cost per move is 1 (grid)
                if (!all.TryGetValue(npos, out var n))
                {
                    n = new Node { pos = npos };
                    all[npos] = n;
                }

                bool inOpen = false;
                for (int i = 0; i < open.Count; i++)
                    if (open[i].pos == npos) { inOpen = true; break; }

                if (!inOpen || tentativeG < n.g)
                {
                    n.parent = cur;
                    n.g = tentativeG;
                    n.h = Manhattan(npos, goal);
                    if (!inOpen) open.Add(n);
                }
            }
        }
        return false;
    }
}
