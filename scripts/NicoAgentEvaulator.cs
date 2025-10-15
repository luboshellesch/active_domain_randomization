using System.Collections;
using UnityEngine;

public class NicoAgentEvaluator : MonoBehaviour
{
    [Header("Evaluation Setup")]
    [Tooltip("Prefab containing the NICO robot with NicoAgentNew and a target cube")]
    public GameObject nicoPrefab;

    [Tooltip("Number of tests (episodes) to run")]
    public int numTests = 20;

    [Tooltip("Maximum duration of a single episode (s)")]
    public float maxEpisodeTime = 30f;

    [Tooltip("Success distance (in meters)")]
    public float successDistance = 0.03f;

    [Tooltip("Effector offset from cube center: 1 = exactly centered, 0 = perpendicular to center (off-target)")]
    public float successPointCenter = 0.9f;

    [Tooltip("Run multiple agents in parallel (side by side)")]
    public bool runParallel = false;

    [Tooltip("If enabled, stops the instance once success or failure is reached.")]
    public bool stopOnSuccess = false;

    [Header("Spawn Layout")]
    [Tooltip("If enabled, agents will spawn in a grid instead of a straight line.")]
    public bool spawnInGrid = true;

    [Tooltip("Spacing between agents (meters).")]
    public float instanceSpacing = 2.0f;

    [Tooltip("Number of agents per row (only used if spawnInGrid = true).")]
    public int agentsPerRow = 5;

    private int successCount = 0;
    private float totalTime = 0f;
    private float totalDist = 0f;
    private Vector3 dirCorrectionLocal = new Vector3(1.6f, 0, 0);

    void Start()
    {
        StartCoroutine(EvaluateAgents());
    }

    IEnumerator EvaluateAgents()
    {
        if (nicoPrefab == null)
        {
            Debug.LogError("[Evaluator] Nico prefab not assigned!");
            yield break;
        }

        Debug.Log($"[Evaluator] Starting evaluation: {numTests} tests...");

        if (runParallel)
        {
            // Run all agents at once
            for (int i = 0; i < numTests; i++)
            {
                Vector3 spawnPos = CalculateSpawnPosition(i);
                GameObject instance = Instantiate(nicoPrefab, spawnPos, Quaternion.identity);
                instance.name = $"NicoEval_{i}";
                StartCoroutine(RunSingleEpisode(instance, i + 1));
                yield return new WaitForSeconds(0.01f);
            }
        }
        else
        {
            // Run agents one at a time 
            for (int i = 0; i < numTests; i++)
            {
                Vector3 spawnPos = CalculateSpawnPosition(i);
                GameObject instance = Instantiate(nicoPrefab, spawnPos, Quaternion.identity);
                instance.name = $"NicoEval_{i}";

                yield return RunSingleEpisode(instance, i + 1);

                Destroy(instance);
                yield return new WaitForSeconds(0.5f);
            }

            PrintResults();
        }
    }

    IEnumerator RunSingleEpisode(GameObject instance, int episodeIndex)
    {
        NicoAgentNew agent = instance.GetComponentInChildren<NicoAgentNew>();
        if (agent == null)
        {
            Debug.LogError($"[Evaluator] NicoAgentNew not found in prefab {instance.name}");
            yield break;
        }

        GameObject effector = agent.effector;
        agent.OnEpisodeBegin();

        float start = Time.time;
        float minDist = float.MaxValue;
        float maxAlignment = 0f;
        bool success = false;

        // Debug line
        LineRenderer arrowLine = effector.AddComponent<LineRenderer>();
        arrowLine.startWidth = 0.002f;
        arrowLine.endWidth = 0.002f;
        arrowLine.material = new Material(Shader.Find("Sprites/Default"));
        arrowLine.startColor = Color.blue;
        arrowLine.endColor = Color.cyan;
        arrowLine.positionCount = 2;

        Transform fingerTip = effector.transform;
        Transform target = agent.target.transform;

        while (Time.time - start < maxEpisodeTime)
        {
            // Measure distance between effector and target
            float dist = Vector3.Distance(fingerTip.position, target.position);
            minDist = Mathf.Min(minDist, dist);

            // Compute direction from effector to target
            Vector3 toTarget = (target.position - fingerTip.position).normalized;

            Vector3 effectorForward = fingerTip.up;  

            // Compute alignment between effector facing direction and target direction
            float facingAlignment = Vector3.Dot(effectorForward, toTarget);
            maxAlignment = Mathf.Max(maxAlignment, facingAlignment);

            // Debug line (for visualizing effector direction)
            Vector3 startPoint = fingerTip.position;
            Vector3 direction = effectorForward; // use same orientation axis consistently
            float lineLength = 1f;
            Vector3 endPoint = startPoint + direction * lineLength + fingerTip.TransformDirection(dirCorrectionLocal);
            arrowLine.useWorldSpace = true;
            arrowLine.SetPosition(0, startPoint);
            arrowLine.SetPosition(1, endPoint);
            Debug.DrawLine(fingerTip.position, target.position, Color.green);



            if (dist <= successDistance && facingAlignment >= successPointCenter)
            {
                success = true;

                arrowLine.startColor = Color.green;
                arrowLine.endColor = Color.green;
                if (stopOnSuccess)
                {
                    // Disable the Agent so ML-Agents stops sending actions
                    StopMLAgent(instance);
                    break;
                }
            }
            yield return null;
        }

        float elapsed = Time.time - start;
        totalDist += minDist;

        if (success)
        {
            successCount++;
            totalTime += elapsed;
        }

        Debug.Log($"[Evaluator] Episode {episodeIndex}/{numTests}: {(success ? "SUCCESS" : "FAIL")} | minDist={minDist} | maxAlignment={maxAlignment} | time={elapsed}s");

        if (stopOnSuccess) StopMLAgent(instance);

        // Ak beží sekvenène a je posledná epizóda
        if (runParallel && episodeIndex == numTests) PrintResults();
    }

    void PrintResults()
    {
        float successRate = (float)successCount / numTests;
        float avgTime = successCount > 0 ? totalTime / successCount : maxEpisodeTime;
        float avgDist = totalDist / numTests;

        Debug.Log($"========== [NICO EVALUATION FINISHED] ==========\n" +
                  $"Tests: {numTests}\n" +
                  $"Success rate: {successRate * 100f:F1}%\n" +
                  $"Avg time to success: {avgTime:F2}s\n" +
                  $"Avg min distance: {avgDist:F3} m\n" +
                  $"===============================================");
    }

    void StopMLAgent(GameObject instance)
    {
        var agentComponent = instance.GetComponentInChildren<NicoAgentNew>();
        if (agentComponent != null)
            agentComponent.enabled = false;
    }

    private Vector3 CalculateSpawnPosition(int index)
    {
        if (spawnInGrid)
        {
            int row = index / agentsPerRow;
            int col = index % agentsPerRow;
            float x = col * instanceSpacing;
            float z = row * instanceSpacing;
            return new Vector3(x, 0, z);
        }
        else
        {
            float x = index * instanceSpacing;
            return new Vector3(x, 0, 0);
        }
    }
}
