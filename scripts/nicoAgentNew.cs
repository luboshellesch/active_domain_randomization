using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using Unity.MLAgents.Policies;

public class NicoAgentNew : Agent
{
    // initialize used variables
    private ArticulationBody nico;

    private List<float> initial_targets = new List<float>();
    private List<float> initial_positions = new List<float>();
    private List<float> initial_velocities = new List<float>();
    private List<float> targets = new List<float>();
    private List<float> initial_changes = new List<float>();
    private List<float> changes = new List<float>();

    private List<int> dof_ind = new List<int>();

    private List<float> low_limits;
    private List<float> high_limits;

    private int dofs;
    private int abs;

    [Tooltip("The target object")]
    public GameObject target;

    [Tooltip("End effector")]
    public GameObject effector;

    private float last_dist;

    //  weights for smoothness & effort
    [Header("Smoothness & Effort Penalties")]
    [Tooltip("Weight for jerk (3rd‐derivative) penalty.")]
    [SerializeField] private float jerkWeight = 1f;
    [Tooltip("Weight for torque/effort penalty.")]
    [SerializeField] private float effortWeight = 0.001f;

    //  history buffer and articulation cache
    private float[] previousChanges;
    private ArticulationBody[] articulationBodies;
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
        nico.GetDofStartIndices(dof_ind);
        abs = nico.GetDriveTargets(initial_targets);
        dofs = abs;
        nico.GetJointPositions(initial_positions);
        nico.GetJointVelocities(initial_velocities);

        low_limits = new List<float>(new float[abs]);
        high_limits = new List<float>(new float[abs]);

        GetLimits(nico, low_limits, high_limits);
        for (int i = 0; i < dofs; ++i)
            initial_changes.Add(0f);

        changes = new List<float>(initial_changes);
        targets = new List<float>(initial_targets);

        last_dist = (target.transform.position - effector.transform.position).magnitude;

        //  cache articulation bodies and init jerk buffer
        articulationBodies = GetComponentsInChildren<ArticulationBody>();
        previousChanges = new float[dofs];

        Debug.Log($"DOFs detected = {dofs},  Action size in Inspector = {GetComponent<BehaviorParameters>().BrainParameters.ActionSpec.NumContinuousActions}");

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

        nico.SetDriveTargets(initial_targets);
        nico.SetJointPositions(initial_positions);
        nico.SetJointVelocities(initial_velocities);

        changes = new List<float>(initial_changes);
        targets = new List<float>(initial_targets);

        //  reset jerk history
        for (int i = 0; i < previousChanges.Length; i++)
            previousChanges[i] = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        List<float> observation = new List<float>();
        nico.GetDriveTargets(observation);

        sensor.AddObservation(observation);
       

        // get vector from end effector to target
        sensor.AddObservation(target.transform.position - effector.transform.position);

        //get end efector orientation; 
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

        // compute jerk penalty
        float jerkPenalty = 0f;
        for (int i = 0; i < dofs; i++)
        {
            float delta = (changes[i] - previousChanges[i]) / Time.fixedDeltaTime;
            jerkPenalty += Mathf.Abs(delta);
            previousChanges[i] = changes[i];
        }

        //  compute effort penalty
        float effortPenalty = 0f;
        foreach (var body in articulationBodies)
        {
            var jf = body.jointForce;
            for (int k = 0; k < jf.dofCount; k++)    // jf.dofCount is cheap
                effortPenalty += Mathf.Abs(jf[k]);
        }

        // original distance‐based rewards
        float new_dist = Vector3.Distance(
            target.transform.position,
            effector.transform.position
        );

        float movementReward = 0f;
        for (int i = 0; i < dofs; ++i)
            movementReward += -0.5f * Mathf.Abs(changes[i]);

        float gotCloser = (last_dist - new_dist) * 0.1f;
        if (gotCloser <= 0) gotCloser = -3f;
        last_dist = new_dist;

        float proximity = -1f * new_dist;

        // combine all
        float totalReward =
              gotCloser
            + proximity
            + movementReward
            - jerkWeight * jerkPenalty
            - effortWeight * effortPenalty;

        AddReward(totalReward);
    }
}
