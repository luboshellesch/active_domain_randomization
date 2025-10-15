using System.Collections;
using UnityEngine;

public class NicoAgentTrainer : MonoBehaviour
{
    [Header("Training Setup")]
    [Tooltip("Prefab containing the NICO robot with NicoAgentNew and target cube.")]
    public GameObject nicoPrefab;

    [Tooltip("Number of training instances to spawn.")]
    public int numInstances = 10;

    [Tooltip("Spacing between spawned agents (in meters).")]
    public float instanceSpacing = 2.0f;

    [Tooltip("Arrange agents in a grid pattern instead of a single line.")]
    public bool spawnInGrid = true;

    [Tooltip("Number of agents per row (only used if spawnInGrid = true).")]
    public int agentsPerRow = 5;

    [Tooltip("Automatically start training on play.")]
    public bool autoStart = true;

    private readonly Vector3 baseOffset = Vector3.zero;
    private readonly float heightOffset = 0f;

    void Start()
    {
        if (autoStart)
        {
            StartTrainingInstances();
        }
    }
    public void StartTrainingInstances()
    {
        if (nicoPrefab == null)
        {
            Debug.LogError("[Trainer] Nico prefab not assigned!");
            return;
        }

        Debug.Log($"[Trainer] Spawning {numInstances} Nico agents for training...");

        for (int i = 0; i < numInstances; i++)
        {
            Vector3 spawnPos = CalculateSpawnPosition(i);
            GameObject instance = Instantiate(nicoPrefab, spawnPos, Quaternion.identity);
            instance.name = $"NicoTrain_{i}";

            NicoAgentNew agent = instance.GetComponentInChildren<NicoAgentNew>();
            if (agent != null)
            {
                agent.OnEpisodeBegin(); // ensure proper reset
            }
        }

        Debug.Log("[Trainer] All training instances spawned successfully.");
    }

    private Vector3 CalculateSpawnPosition(int index)
    {
        if (spawnInGrid)
        {
            int row = index / agentsPerRow;
            int col = index % agentsPerRow;

            float x = col * instanceSpacing;
            float z = row * instanceSpacing;
            return baseOffset + new Vector3(x, heightOffset, z);
        }
        else
        {
            float x = index * instanceSpacing;
            return baseOffset + new Vector3(x, heightOffset, 0);
        }
    }

    public void ClearTrainingInstances()
    {
        foreach (var agent in GameObject.FindGameObjectsWithTag("NicoAgent"))
        {
            Destroy(agent);
        }

        foreach (var trainer in GameObject.FindGameObjectsWithTag("NicoTrain"))
        {
            Destroy(trainer);
        }

        Debug.Log("[Trainer] All training instances cleared.");
    }
}
