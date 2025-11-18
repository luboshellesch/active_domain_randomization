using System.Collections;
using UnityEngine;

public class NicoAgentEvaluator : MonoBehaviour
{
    [Header("Evaluation Setup")]
    [Tooltip("Prefab containing the NICO robot with NicoAgentNew and a target cube")]
    public GameObject NicoPrefab;

    [Tooltip("Number of tests (episodes) to run")]
    public int NumTests = 20;

    [Tooltip("Maximum duration of a single episode (s)")]
    public float MaxEpisodeTime = 30f;

    [Tooltip("Success distance (in meters)")]
    public float SuccessDistance = 0.03f;

    [Tooltip("Effector alignment with target direction (dot), 1=centered")]
    public float SuccessPointCenter = 0.9f;

    [Tooltip("Run multiple agents in parallel (side by side)")]
    public bool RunParallel = false;

    [Tooltip("If enabled, stops the instance once success or failure is reached.")]
    public bool StopOnSuccess = false;

    [Header("Spawn Layout")]
    [Tooltip("Spacing between agents (meters).")]
    public float InstanceSpacing = 2.0f;

    [Tooltip("Number of agents per row (only used if SpawnInGrid = true).")]
    public int AgentsPerRow = 5;

    private int _finishedCount = 0;
    private int _successCount = 0;
    private float _totalTime = 0f;
    private float _totalDist = 0f;

    private static Material _lineMaterial;

    private NicoAgentNew _nicoPrefabAgent;

    private void Awake()
    {
        if (NicoPrefab == null)
        {
            Debug.LogError("[Evaluator] Nico prefab not assigned!");
            return;
        }
        _nicoPrefabAgent = NicoPrefab.GetComponentInChildren<NicoAgentNew>();
        if (_nicoPrefabAgent == null)
        {
            Debug.LogError("[Evaluator] NicoAgentNew not found on NicoPrefab!");
            return;
        }

        if (!_nicoPrefabAgent.evaluationMode)
        {
            _nicoPrefabAgent.evaluationMode = true;
            Debug.Log("[Evaluator] Switched NicoAgentNew Evaulation Mode on!");
        }
        

        _nicoPrefabAgent.MaxStep = 0;
    }
    private void Start()
    {
        if (_lineMaterial == null)
            _lineMaterial = new Material(Shader.Find("Sprites/Default"));
        if (_nicoPrefabAgent == null)
            return;
        StartCoroutine(EvaluateAgents());
    }

    private IEnumerator EvaluateAgents()
    {
        

        Debug.Log($"[Evaluator] Starting evaluation: {NumTests} tests...");

        if (RunParallel)
        {
            for (int i = 0; i < NumTests; i++)
            {
                Vector3 spawnPos = CalculateSpawnPosition(i);
                GameObject instance = Instantiate(NicoPrefab, spawnPos, Quaternion.identity);
                instance.name = $"NicoEval_{i}";
                StartCoroutine(RunSingleEpisode(instance, i + 1));
                yield return new WaitForSeconds(0.01f);
            }
        }
        else
        {
            for (int i = 0; i < NumTests; i++)
            {
                Vector3 spawnPos = CalculateSpawnPosition(i);
                GameObject instance = Instantiate(NicoPrefab, spawnPos, Quaternion.identity);
                instance.name = $"NicoEval_{i}";

                yield return RunSingleEpisode(instance, i + 1);

                Destroy(instance);
                yield return new WaitForSeconds(0.5f);
            }

            PrintResults();
        }
    }

    private IEnumerator RunSingleEpisode(GameObject instance, int episodeIndex)
    {
        NicoAgentNew agent = instance.GetComponentInChildren<NicoAgentNew>();
        if (agent == null)
        {
            Debug.LogError($"[Evaluator] NicoAgentNew not found in prefab {instance.name}");
            yield break;
        }

        GameObject effector = agent.Effector;
        agent.OnEpisodeBegin();

        float startTime = Time.time;
        float minDist = float.MaxValue;
        float maxAlignment = 0f;
        bool success = false;

        LineRenderer arrowLine = effector.AddComponent<LineRenderer>();
        arrowLine.useWorldSpace = true;
        arrowLine.startWidth = 0.002f;
        arrowLine.endWidth = 0.002f;
        arrowLine.material = _lineMaterial;
        arrowLine.startColor = Color.blue;
        arrowLine.endColor = Color.cyan;
        arrowLine.positionCount = 2;

        Transform fingerTip = effector.transform;
        Transform target = agent.Target.transform;

        while (Time.time - startTime < MaxEpisodeTime)
        {
            float dist = EffectorTargeting.GetDistanceToTarget(fingerTip.position, target.position);
            minDist = Mathf.Min(minDist, dist);

            float facingAlignment = EffectorTargeting.GetAlignmentScore(
                fingerTip,
                target.position
            );
            maxAlignment = Mathf.Max(maxAlignment, facingAlignment);

            Vector3 startPoint = fingerTip.position;
            Vector3 direction = EffectorTargeting.GetPointingDirection(fingerTip);
            Vector3 endPoint = startPoint + direction * 1f;
            arrowLine.SetPosition(0, startPoint);
            arrowLine.SetPosition(1, endPoint);

            // Debug visualization using consistent direction calculation
            Debug.DrawRay(
                fingerTip.position,
                EffectorTargeting.GetDirectionToTarget(fingerTip, target.position),
                Color.green
            );

            if (dist <= SuccessDistance && facingAlignment >= SuccessPointCenter)
            {
                success = true;
                if (StopOnSuccess)
                {
                    StopMLAgent(instance);
                    break;
                }
            }
            yield return null;
        }

        float elapsed = Time.time - startTime;
        _totalDist += minDist;
        if (success)
        {
            _successCount++;
            _totalTime += elapsed;
        }

        _finishedCount++;

        Debug.Log($"[Evaluator] Episode {episodeIndex}/{NumTests}: {(success ? "SUCCESS" : "FAIL")} | minDist={minDist} | maxAlignment={maxAlignment} | time={elapsed:F2}s");
        Debug.Log($"Episode {episodeIndex} finished with StepCount={agent.StepCount} and CompletedEpisodes={agent.CompletedEpisodes}");

        if (RunParallel && _finishedCount >= NumTests)
            PrintResults();
    }

    private void PrintResults()
    {
        float successRate = (float)_successCount / NumTests;
        float avgTime = _successCount > 0 ? _totalTime / _successCount : MaxEpisodeTime;
        float avgDist = _totalDist / NumTests;

        Debug.Log(
            $"========== [NICO EVALUATION FINISHED] ==========\n" +
            $"Tests: {NumTests}\n" +
            $"Success rate: {successRate * 100f:F1}%\n" +
            $"Avg time to success: {avgTime:F2}s\n" +
            $"Avg min distance: {avgDist:F3} m\n" +
            $"==============================================="
        );
    }

    private void StopMLAgent(GameObject instance)
    {
        var agentComponent = instance.GetComponentInChildren<NicoAgentNew>();
        if (agentComponent != null) agentComponent.enabled = false;
    }

    private Vector3 CalculateSpawnPosition(int index)
    {
        if (RunParallel)
        {
            int row = index / AgentsPerRow;
            int col = index % AgentsPerRow;
            float x = col * InstanceSpacing;
            float z = row * InstanceSpacing;
            return new Vector3(x, 0, z);
        }
        return new Vector3(0, 0, 0);
    }
}
