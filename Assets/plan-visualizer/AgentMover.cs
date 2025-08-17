using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AgentMover : MonoBehaviour
{
    public GridNav grid;
    public float moveSpeed = 4f;
    public float arriveEpsilon = 0.05f;

    CharacterController cc;
    readonly List<Vector2Int> path = new List<Vector2Int>();

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cc == null) cc = gameObject.AddComponent<CharacterController>();
        // Make capsule height small for grid world
        cc.height = 1.8f;
        cc.radius = 0.3f;
    }

    public Coroutine MoveTo(Vector2Int target)
        => StartCoroutine(MoveRoutine(target));

    IEnumerator MoveRoutine(Vector2Int target)
    {
        path.Clear();
        Vector2Int start = grid.WorldToGrid(transform.position);
        if (!AStarPathfinder.FindPath(grid, start, target, path))
            yield break;

        // Skip the first node (current cell)
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 wp = grid.GridToWorld(path[i]);
            while ((transform.position - wp).sqrMagnitude > arriveEpsilon * arriveEpsilon)
            {
                Vector3 dir = (wp - transform.position);
                dir.y = 0;
                Vector3 step = dir.normalized * moveSpeed * Time.deltaTime;
                cc.Move(step);
                yield return null;
            }
            // Snap to center
            Vector3 snap = grid.GridToWorld(path[i]);
            transform.position = new Vector3(snap.x, transform.position.y, snap.z);
            yield return null;
        }
    }
}
