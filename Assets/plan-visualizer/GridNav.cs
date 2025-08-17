using System.Collections.Generic;
using UnityEngine;

public class GridNav : MonoBehaviour
{
    [Header("Grid")]
    public int width = 50;
    public int height = 50;
    public float cellSize = 1f;
    public Vector3 origin = Vector3.zero;

    // Optional: simple obstacles for testing
    public List<Vector2Int> blocked = new List<Vector2Int>();

    public bool InBounds(Vector2Int g) => g.x >= 0 && g.x < width && g.y >= 0 && g.y < height;
    public bool Walkable(Vector2Int g) => InBounds(g) && !blocked.Contains(g);

    public Vector3 GridToWorld(Vector2Int g)
        => origin + new Vector3(g.x * cellSize + cellSize * 0.5f, 0f, g.y * cellSize + cellSize * 0.5f);

    public Vector2Int WorldToGrid(Vector3 w)
    {
        Vector3 local = w - origin;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.z / cellSize);
        return new Vector2Int(x, y);
    }

    public IEnumerable<Vector2Int> Neighbors4(Vector2Int g)
    {
        var dirs = new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
        foreach (var d in dirs)
        {
            var n = g + d;
            if (Walkable(n)) yield return n;
        }
    }

    void OnDrawGizmos()
    {
        // Draw grid lines lightly
        Gizmos.color = new Color(1,1,1,0.1f);
        for (int x = 0; x <= width; x++)
        {
            var a = origin + new Vector3(x * cellSize, 0, 0);
            var b = origin + new Vector3(x * cellSize, 0, height * cellSize);
            Gizmos.DrawLine(a, b);
        }
        for (int y = 0; y <= height; y++)
        {
            var a = origin + new Vector3(0, 0, y * cellSize);
            var b = origin + new Vector3(width * cellSize, 0, y * cellSize);
            Gizmos.DrawLine(a, b);
        }

        // Draw blocked cells darker
        Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        foreach (var b in blocked)
        {
            var c = GridToWorld(b);
            Gizmos.DrawCube(new Vector3(c.x, 0, c.z), new Vector3(cellSize * 0.95f, 0.01f, cellSize * 0.95f));
        }
    }
}
