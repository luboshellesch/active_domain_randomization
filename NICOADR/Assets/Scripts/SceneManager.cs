using UnityEngine;
using Unity.MLAgents.Policies;

public class SceneManager : MonoBehaviour
{
    public enum RunMode
    {
        Training,
        Evaluation
    }

    [Header("Mode")]
    [Tooltip("Select whether this scene runs training or evaluation.")]
    public RunMode Mode = RunMode.Training;

    [Header("Shared Nico Setup")]
    [Tooltip("Prefab containing NicoAgent used by Evaulator and Trainer.")]
    public GameObject NicoPrefab;

    [Tooltip("NICO.onnx imported as an NNModel asset. Used ONLY in Evaluation mode.")]
    public Unity.Sentis.ModelAsset NicoModel;

    [Header("Scene Settings")]
    [Tooltip("Nico placeholder prefab.")]
    public GameObject NicoPlaceholderPrefab;

    [Header("Sub-Controllers")]
    public NicoAgentTrainer Trainer;
    public NicoAgentEvaluator Evaluator;

    private void Awake()
    {
        // Make sure both scripts use the same prefab
        if (Trainer != null)
            Trainer.NicoPrefab = NicoPrefab;

        if (Evaluator != null)
            Evaluator.NicoPrefab = NicoPrefab;

        if (NicoPlaceholderPrefab != null)
        {
            Destroy(NicoPlaceholderPrefab);
            NicoPlaceholderPrefab = null;
        }
    }

    private void Start()
    {
        ApplyMode();
    }

    public void ApplyMode()
    {
        switch (Mode)
        {
            case RunMode.Training:
                ConfigureForTraining();
                break;

            case RunMode.Evaluation:
                ConfigureForEvaluation();
                break;
        }
    }

    private void ConfigureForTraining()
    {
        Debug.Log("[SceneManager] Configuring for TRAINING");

        if (Trainer != null)
        {
            Trainer.enabled = true;
        }

        if (Evaluator != null)
        {
            Evaluator.enabled = false;
        }

        if (NicoPrefab == null)
            return;

        var agent = NicoPrefab.GetComponentInChildren<NicoAgentNew>();
        if (agent == null)
            return;

        // Make sure agent is in training mode
        agent.evaluationMode = false;

        var bp = agent.GetComponent<BehaviorParameters>();
        if (bp != null)
        {
            bp.Model = null;
        }
        Trainer.StartTraining();

    }

    private void ConfigureForEvaluation()
    {
        Debug.Log("[SceneManager] Configuring for EVALUATION");

        if (Trainer != null)
        {
            Trainer.enabled = false;
        }

        if (Evaluator != null)
        {
            Evaluator.enabled = true;
        }

        if (NicoPrefab == null)
            return;

        var agent = NicoPrefab.GetComponentInChildren<NicoAgentNew>();
        if (agent == null)
            return;

        // Make sure agent is in evaluation mode
        agent.evaluationMode = true;
        agent.MaxStep = 0;

        var bp = agent.GetComponent<BehaviorParameters>();
        if (bp != null)
        {
            bp.Model = NicoModel;                    // assign NICO.onnx
        }
        Debug.Log("[SceneManager]: " + Evaluator.NicoPrefab);
        Evaluator.NicoPrefab = NicoPrefab;
        Debug.Log("[SceneManager 2]: " + Evaluator.NicoPrefab);
        Evaluator.StartEvaluation();
    }
}
