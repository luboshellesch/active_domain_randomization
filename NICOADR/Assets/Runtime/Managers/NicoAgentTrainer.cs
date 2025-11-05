using System.Collections.Generic;
using UnityEngine;

public class NicoAgentTrainer : MonoBehaviour
{
    [Header("Training Setup")]
    [Tooltip("Prefab containing the NICO robot with NicoAgentNew and target cube.")]
    public GameObject NicoPrefab;

    [Tooltip("Number of training instances to spawn.")]
    public int NumInstances = 10;

    [Tooltip("Spacing between spawned agents (in meters).")]
    public float InstanceSpacing = 2.0f;

    [Tooltip("Arrange agents in a grid pattern instead of a single line.")]
    public bool SpawnInGrid = true;

    [Tooltip("Number of agents per row (only used if SpawnInGrid = true).")]
    public int AgentsPerRow = 5;

    [Tooltip("Automatically start training on play.")]
    public bool AutoStart = true;

    private readonly Vector3 _baseOffset = Vector3.zero;
    private readonly float _heightOffset = 0f;
    private readonly List<GameObject> _spawnedAgents = new();

    private void Start()
    {
        if (AutoStart)
        {
            StartTrainingInstances();
        }
    }

    public void StartTrainingInstances()
    {
        if (NicoPrefab == null)
        {
            Debug.LogError("[Trainer] Nico prefab not assigned!");
            return;
        }

        Debug.Log($"[Trainer] Spawning {NumInstances} Nico agents for training...");

        for (int i = 0; i < NumInstances; i++)
        {
            Vector3 spawnPos = CalculateSpawnPosition(i);
            GameObject instance = Instantiate(NicoPrefab, spawnPos, Quaternion.identity);
            instance.name = $"NicoTrain_{i}";
            _spawnedAgents.Add(instance);

            NicoAgentNew agent = instance.GetComponentInChildren<NicoAgentNew>();
            if (agent != null)
            {
                agent.EndEpisode(); // Ensure proper reset
            }
        }

        Debug.Log("[Trainer] All training instances spawned successfully.");
    }

    private Vector3 CalculateSpawnPosition(int index)
    {
        if (SpawnInGrid)
        {
            int row = index / AgentsPerRow;
            int col = index % AgentsPerRow;

            float x = col * InstanceSpacing;
            float z = row * InstanceSpacing;
            return _baseOffset + new Vector3(x, _heightOffset, z);
        }
        else
        {
            float x = index * InstanceSpacing;
            return _baseOffset + new Vector3(x, _heightOffset, 0);
        }
    }

    public void ClearTrainingInstances()
    {
        foreach (var agent in _spawnedAgents)
        {
            if (agent) Destroy(agent);
        }
        _spawnedAgents.Clear();

        Debug.Log("[Trainer] All training instances cleared.");
    }
}
