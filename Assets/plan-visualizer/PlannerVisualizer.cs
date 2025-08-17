using System.Collections;
using System.Collections.Generic;
using System.Linq;
using brutus_planner.abstractions;
using UnityEngine;

public class PlannerVisualizer : MonoBehaviour
{
    [System.Serializable]
    public struct LocationDef
    {
        public string title;        // e.g., "SkinAnimal"
        public Vector2Int gridPos;  // e.g., (5, 12)
        public Color color;         // marker color
    }

    [System.Serializable]
    public struct PlanStep
    {
        public string title;
    }

    [Header("Scene Refs")]
    public GridNav grid;
    public AgentMover agentPrefab;

    //[Header("Locations (title -> position)")]
    //public List<LocationDef> locations = new List<LocationDef>();

    //[Header("Plan to Play")]
    //public List<PlanStep> plan = new List<PlanStep>();

    [Header("Agent Setup")]
    public Vector2Int agentStart = new Vector2Int(1, 1);

    AgentMover agentInstance;
    Dictionary<string, Vector2Int> map;

    [Header("Agent Provider (from BE)")]
    [SerializeField] private AgentProvider agentProvider; // assign SO asset in Inspector
    void Reset()
    {
        // Example defaults
        if (grid == null)
        {
            var gridGO = new GameObject("GridNav");
            grid = gridGO.AddComponent<GridNav>();
            grid.width = 50;
            grid.height = 50;
            grid.cellSize = 1f;
        }
    }

    Dictionary<string, System.Numerics.Vector3> fullPlan;
    void Start()
    {
        var agent = agentProvider.CreateAgent();
        
        fullPlan = agent.CreatePlan();

        
        if (grid == null)
        {
            Debug.LogError("GridNav reference missing.");
            return;
        }

        // Build dictionary
        map = new Dictionary<string, Vector2Int>();
        foreach (var loc in fullPlan)
        {
            if (!map.ContainsKey(loc.Key))
                map.Add(loc.Key, new Vector2Int((int)loc.Value.X, (int)loc.Value.Z));
        }

        // Spawn visual markers for each location
        foreach (var loc in fullPlan)
        {
            var def = new LocationDef();
            def.title = loc.Key;
            def.gridPos = new Vector2Int((int)loc.Value.X, (int)loc.Value.Z);
            SpawnMarker(def);
        }

        // Spawn agent
        if (agentPrefab == null)
        {
            // Auto-create a simple capsule agent if prefab not assigned
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Agent";
            agentInstance = capsule.AddComponent<AgentMover>();
            agentInstance.grid = grid;
        }
        else
        {
            agentInstance = Instantiate(agentPrefab);
            agentInstance.grid = grid;
        }

        var startWorld = grid.GridToWorld(agentStart);
        agentInstance.transform.position = new Vector3(startWorld.x, 0.9f, startWorld.z);

        LogPlan(fullPlan);
        // Kick off plan runner
        StartCoroutine(RunPlan());
    }

    void LogPlan(Dictionary<string, System.Numerics.Vector3> plan)
    {
        string planLog = "Full Plan: \n";

        foreach (var planStep in plan)
        {
            planLog += $"{planStep.Key} at {planStep.Value} \n";
        }
        
        Debug.Log(planLog);
        
    }

    IEnumerator RunPlan()
    {
        
        // Basic display in Console
        //for (int i = 0; i < fullPlan.Count; i++)
            //Debug.Log($"{i + 1}: {fullPlan.ElementAt(i).Key} at {fullPlan.ElementAt(i).Value}");

        // Step through tasks
        for (int i = 0; i < fullPlan.Count; i++)
        {
            var step = fullPlan.ElementAt(i).Key;
            if (!map.TryGetValue(step, out var target))
            {
                //Debug.LogWarning($"No location defined for '{step}'. Skipping.");
                continue;
            }
            yield return agentInstance.MoveTo(target);
            // Small pause at each location
            yield return new WaitForSeconds(0.2f);
        }

        //Debug.Log("Plan complete.");
    }

    void SpawnMarker(LocationDef loc)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Loc_{loc.title}";
        go.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        var p = grid.GridToWorld(loc.gridPos);
        go.transform.position = new Vector3(p.x, 0.3f, p.z);
        go.GetComponent<Collider>().enabled = false;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = loc.color == default ? Color.yellow : loc.color;
            mr.material = mat;
        }

        // Simple floating text label (3D TextMesh so no TMP dependency)
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        textGO.transform.localPosition = new Vector3(0, 1.0f, 0);

        var tm = textGO.AddComponent<TextMesh>();
        tm.text = loc.title;
        tm.characterSize = 0.2f;
        tm.fontSize = 64;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
    }
}
