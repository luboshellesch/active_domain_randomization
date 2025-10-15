using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class NicoAgentNew : Agent
{
    // initialize used variables
    private ArticulationBody nico;

    private List<float> inititalTargets = new List<float>();
    private List<float> initialPositions = new List<float>();
    private List<float> initialVelocities = new List<float>();
    private List<float> targets = new List<float>();
    private List<float> initialChanges = new List<float>();
    private List<float> changes = new List<float>();

    private List<int> dofIndices = new List<int>();

    private List<float> low_limits;
    private List<float> high_limits;

    private int dofs;

    [Tooltip("The target object")]
    public GameObject target;

    [Tooltip("End effector")]
    public GameObject effector;

    private float lastDistanceToTarget;

    //  weights for smoothness & effort
    [Header("Smoothness & Effort Penalties")]
    [Tooltip("Weight for per-step time penalty (discourages long episodes).")]
    [SerializeField] private float timeWeight = -0.001f;
    [Tooltip("Penalty weight when the end-effector moves farther from the target.")]
    [SerializeField] private float offTargetWeight = -3f;

    private Vector3 dirCorrectionLocal = new Vector3(1.6f, 0, 0);


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
        for (int i = 0; i < dofs; ++i)
            initialChanges.Add(0f);

        changes = new List<float>(initialChanges);
        targets = new List<float>(inititalTargets);

        lastDistanceToTarget = (target.transform.position - effector.transform.position).magnitude;
    }

    public override void OnEpisodeBegin()
    {
        Vector3 offset = new Vector3(
            Random.Range(-0.2f, -0.6f),
            Random.Range(-0.2f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );
        target.transform.position = nico.transform.position + offset;
        target.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        nico.SetDriveTargets(inititalTargets);
        nico.SetJointPositions(initialPositions);
        nico.SetJointVelocities(initialVelocities);

        changes = new List<float>(initialChanges);
        targets = new List<float>(inititalTargets);
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        List<float> observation = new List<float>();
        nico.GetDriveTargets(observation);

        sensor.AddObservation(observation);

        // get vector from end effector to target
        sensor.AddObservation(target.transform.position - effector.transform.position);

        //get end efector orientation
        sensor.AddObservation(effector.transform.localRotation);

        // end effector possition
        sensor.AddObservation(effector.transform);

        // distance to object
        sensor.AddObservation(Vector3.Distance(effector.transform.position, target.transform.position));
    }




    public override void OnActionReceived(ActionBuffers actions)
    {

        float max_range = Mathf.Deg2Rad * 0.1f;
        float change_magnitude = Mathf.Deg2Rad * 0.05f;

        int actionCount = actions.ContinuousActions.Length;
        for (int i = 0; i < dofs; ++i)
        {
            float delta = (i < actionCount) ? actions.ContinuousActions[i] : 0f;
            changes[i] = Mathf.Clamp(
                changes[i] + delta * change_magnitude,
                -max_range, max_range
            );
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

        //// effort penalty
        //float effortPenalty = 0f;
        //foreach (var body in articulationBodies)
        //    effortPenalty += Mathf.Abs(body.jointForce[0]);

        // original distance‐based rewards
        float new_dist = Vector3.Distance(
            target.transform.position,
            effector.transform.position
        );

        float movementReward = 0f;
        for (int i = 0; i < dofs; ++i)
            movementReward += -0.5f * Mathf.Abs(changes[i]);

        float gotCloser = (lastDistanceToTarget - new_dist) * 0.1f;
        if (gotCloser <= 0) gotCloser = -3f;
        lastDistanceToTarget = new_dist;

        float proximity = -1f * new_dist;

        // flat time penalty (constant each FixedUpdate)
        float timePenalty = timeWeight * Time.fixedDeltaTime;

        // extra penalty when distance grows (agent "goes off" target)
        float offTargetPenalty = 0f;
        if (new_dist > lastDistanceToTarget)
            offTargetPenalty = offTargetWeight * (new_dist - lastDistanceToTarget);

        //nico_new_agent_1,  nico_new_agent_2 
        //---------------------------------------------------------------------------\\

        // pointing to target
        Vector3 toTarget = (target.transform.position - effector.transform.position).normalized;
        Vector3 forwardDir = effector.transform.up; // or the appropriate "pointing" direction of your effector
        float pointingAngle = Vector3.Angle(forwardDir, toTarget);

        float pointingReward = 0f;
        float maxPointingAngle = 10f; // degrees threshold
        if (pointingAngle < maxPointingAngle)
        {
            pointingReward = -1f; // reward for pointing correctly
        }

        // stayng inside target
        float insideReward = 0f;
        float insideDistanceThreshold = 0.5f; // adjust based on your target size
        if (Vector3.Distance(effector.transform.position, target.transform.position) < insideDistanceThreshold)
        {
            if (pointingAngle < maxPointingAngle) // only reward if already pointing correctly
                insideReward = -2f; // additional reward for staying inside
        }

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
