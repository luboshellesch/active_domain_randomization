using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class NicoAgentNew : Agent
{
    public enum LocalAxis { X, Y, Z }

    // --- Config ---
    [Tooltip("The target object")]
    public GameObject target;

    [Tooltip("End effector")]
    public GameObject effector;

    [Header("Pointing Axis (must match evaluator/visual)")]
    public LocalAxis pointingAxis = LocalAxis.Y;

    // --- Robot state ---
    private ArticulationBody nico;

    private readonly List<float> inititalTargets = new List<float>();
    private readonly List<float> initialPositions = new List<float>();
    private readonly List<float> initialVelocities = new List<float>();
    private List<float> targets = new List<float>();
    private readonly List<float> initialChanges = new List<float>();
    private List<float> changes = new List<float>();
    private readonly List<int> dofIndices = new List<int>();

    private List<float> low_limits;
    private List<float> high_limits;

    private int dofs;
    private float lastDistanceToTarget;

    // weights
    private float timeWeight = 0.001f;
    private float offTargetWeight = 3f;

    private static Vector3 AxisToLocal(LocalAxis axis)
    {
        switch (axis)
        {
            case LocalAxis.X: return Vector3.right;
            case LocalAxis.Y: return Vector3.up;
            case LocalAxis.Z: return Vector3.forward;
            default: return Vector3.up;
        }
    }

    private void GetLimits(ArticulationBody root, List<float> llimits, List<float> hlimits)
    {
        GameObject curr_obj = root.gameObject;
        int num_ch = curr_obj.transform.childCount;
        for (int i = 0; i < num_ch; ++i)
        {
            GameObject child = curr_obj.transform.GetChild(i).gameObject;
            ArticulationBody child_ab = child.GetComponent<ArticulationBody>();
            if (child_ab != null)
            {
                int j = child_ab.index;
                llimits[j - 1] = child_ab.xDrive.lowerLimit;
                hlimits[j - 1] = child_ab.xDrive.upperLimit;
                GetLimits(child_ab, llimits, hlimits);
            }
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        nico = GetComponent<ArticulationBody>();
        nico.GetDofStartIndices(dofIndices);
        dofs = nico.GetDriveTargets(inititalTargets);
        nico.GetJointPositions(initialPositions);
        nico.GetJointVelocities(initialVelocities);

        low_limits = new List<float>(new float[dofs]);
        high_limits = new List<float>(new float[dofs]);

        GetLimits(nico, low_limits, high_limits);
        for (int i = 0; i < dofs; ++i) initialChanges.Add(0f);

        changes = new List<float>(initialChanges);
        targets = new List<float>(inititalTargets);

        lastDistanceToTarget = Vector3.Distance(target.transform.position, effector.transform.position);
    }

    public override void OnEpisodeBegin()
    {
        Vector3 offset = new Vector3(
            Random.Range(-0.6f, -0.2f),
            Random.Range(-0.2f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );
        target.transform.position = nico.transform.position + offset;
        target.transform.localRotation = Quaternion.identity;

        nico.SetDriveTargets(inititalTargets);
        nico.SetJointPositions(initialPositions);
        nico.SetJointVelocities(initialVelocities);

        changes = new List<float>(initialChanges);
        targets = new List<float>(inititalTargets);

        lastDistanceToTarget = Vector3.Distance(target.transform.position, effector.transform.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Joint drive targets
        List<float> observation = new List<float>();
        nico.GetDriveTargets(observation);
        sensor.AddObservation(observation);

        // Vector to target
        Vector3 toTarget = target.transform.position - effector.transform.position;
        sensor.AddObservation(toTarget);

        // Effector orientation (quaternion)
        sensor.AddObservation(effector.transform.localRotation);

        // Effector position (world)
        sensor.AddObservation(effector.transform.position);

        // Distance to target
        sensor.AddObservation(toTarget.magnitude);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float max_range = Mathf.Deg2Rad * 0.1f;
        float change_magnitude = Mathf.Deg2Rad * 0.05f;

        int actionCount = actions.ContinuousActions.Length;
        for (int i = 0; i < dofs; ++i)
        {
            float delta = (i < actionCount) ? actions.ContinuousActions[i] : 0f;
            changes[i] = Mathf.Clamp(changes[i] + delta * change_magnitude, -max_range, max_range);
        }

        for (int i = 0; i < dofs; ++i)
        {
            targets[i] = Mathf.Clamp(
                targets[i] + changes[i],
                Mathf.Deg2Rad * low_limits[i],
                Mathf.Deg2Rad * high_limits[i]
            );
        }

        nico.SetDriveTargets(targets);

        // Distances
        float new_dist = Vector3.Distance(target.transform.position, effector.transform.position);

        // Movement smoothness (penalize large changes)
        float movementReward = 0f;
        for (int i = 0; i < dofs; ++i)
            movementReward += -0.5f * Mathf.Abs(changes[i]);

        // Progress toward target
        float gotCloser = (lastDistanceToTarget - new_dist) * 0.1f;
        if (gotCloser <= 0f) gotCloser = -3f;

        // Proximity (closer is better)
        float proximity = -1f * new_dist;

        float offTargetPenalty = 0f;
        if (new_dist > lastDistanceToTarget)
            offTargetPenalty = offTargetWeight * (new_dist - lastDistanceToTarget);

        //nico_new_agent_1, nico_new_agent_2 
        //---------------------------------------------------------------------------\\

        // Time penalty
        float timePenalty = timeWeight * Time.fixedDeltaTime;

        // Pointing reward uses the SAME axis definition as visualizer/evaluator
        Vector3 localAxis = AxisToLocal(pointingAxis);
        Vector3 forwardDir = effector.transform.TransformDirection(localAxis);
        Vector3 toTargetDir = (target.transform.position - effector.transform.position).normalized;

        float pointingAngle = Vector3.Angle(forwardDir, toTargetDir);
        float pointingReward = 0f;
        float maxPointingAngle = 5f; // degrees threshold
        if (pointingAngle < maxPointingAngle)
            pointingReward = -1f; // reward (negative cost framework)

        // Staying inside (optional shaping)
        float insideReward = 0f;
        float insideDistanceThreshold = 0.5f;
        if (new_dist < insideDistanceThreshold && pointingAngle < maxPointingAngle)
            insideReward = -2f;

        // Now that we've used it, update lastDistanceToTarget
        lastDistanceToTarget = new_dist;

        //nico_new_agent_3 
        //---------------------------------------------------------------------------\\

        float totalReward =
              gotCloser
            + proximity
            + movementReward
            + pointingReward
            + insideReward
            - timePenalty
            - offTargetPenalty;

        AddReward(totalReward);
    }
}
