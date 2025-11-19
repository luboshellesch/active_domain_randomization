using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class NicoAgentNew : Agent
{
    [Tooltip("The target object")]
    public GameObject Target;

    [Tooltip("End effector")]
    public GameObject Effector;

    [Tooltip("Evaulation mode switch")]
    public bool evaluationMode;

    [Header("Debug Visualization")]
    [SerializeField] private Material _debugAreaMaterial;


    // --- Robot state ---
    private ArticulationBody _nico;
    private readonly List<float> _initialTargets = new();
    private readonly List<float> _initialPositions = new();
    private readonly List<float> _initialVelocities = new();
    private List<float> _targets = new();
    private readonly List<float> _initialChanges = new();
    private List<float> _changes = new();
    private readonly List<int> _dofIndices = new();
    private List<float> _lowLimits;
    private List<float> _highLimits;
    private int _dofs;

    // --- Reward ---
    private float _angleThreshold = 3f;   
    private float _distThreshold = 0.02f;
    private int _alignedSteps = 0;
    private int _stepsToSuccess = 10;


    private void GetLimits(ArticulationBody root, List<float> lowerLimits, List<float> upperLimits)
    {
        var currentObj = root.gameObject;
        int childCount = currentObj.transform.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            var child = currentObj.transform.GetChild(i).gameObject;
            var childAb = child.GetComponent<ArticulationBody>();
            if (childAb != null)
            {
                int index = childAb.index;
                lowerLimits[index - 1] = childAb.xDrive.lowerLimit;
                upperLimits[index - 1] = childAb.xDrive.upperLimit;
                GetLimits(childAb, lowerLimits, upperLimits);
            }
        }
    }

    [Header("Target Spawn Settings")]
    private Vector3 _spawnAreaMin = new(-0.3f, -0.1f, -0.6f);
    private Vector3 _spawnAreaMax = new(0.3f, 0.5f, -1.6f);
    private GameObject _debugSpawnArea;
    private void CreateDebugSpawnArea()
    {
        if (_debugSpawnArea != null) return;

        _debugSpawnArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _debugSpawnArea.name = "TargetSpawnArea";

        DestroyImmediate(_debugSpawnArea.GetComponent<Collider>());

        var renderer = _debugSpawnArea.GetComponent<Renderer>();
        renderer.material = _debugAreaMaterial;
    }

    public override void Initialize()
    {
        base.Initialize();

        _nico = GetComponent<ArticulationBody>();
        _nico.GetDofStartIndices(_dofIndices);
        _dofs = _nico.GetDriveTargets(_initialTargets);
        _nico.GetJointPositions(_initialPositions);
        _nico.GetJointVelocities(_initialVelocities);

        _lowLimits = new List<float>(new float[_dofs]);
        _highLimits = new List<float>(new float[_dofs]);
        GetLimits(_nico, _lowLimits, _highLimits);

        for (int i = 0; i < _dofs; ++i)
            _initialChanges.Add(0f);

        _changes = new List<float>(_initialChanges);
        _targets = new List<float>(_initialTargets);

        //CreateDebugSpawnArea();
        UpdateDebugSpawnArea();
    }

    private void UpdateDebugSpawnArea()
    {
        if (_debugSpawnArea == null) return;

        Vector3 center = _nico.transform.position + (_spawnAreaMin + _spawnAreaMax) / 2f;
        _debugSpawnArea.transform.position = center;

        Vector3 size = _spawnAreaMax - _spawnAreaMin;
        _debugSpawnArea.transform.localScale = size;
    }

    public override void OnEpisodeBegin()
    {
        Vector3 randomOffset = new(
            Random.Range(_spawnAreaMin.x, _spawnAreaMax.x),
            Random.Range(_spawnAreaMin.y, _spawnAreaMax.y),
            Random.Range(_spawnAreaMin.z, _spawnAreaMax.z)
        );

        Target.transform.position = _nico.transform.position + randomOffset;
        Target.transform.localRotation = Quaternion.identity;

        _nico.SetDriveTargets(_initialTargets);
        _nico.SetJointPositions(_initialPositions);
        _nico.SetJointVelocities(_initialVelocities);

        _changes = new List<float>(_initialChanges);
        _targets = new List<float>(_initialTargets);
    }
    private void OnDestroy()
    {
        if (_debugSpawnArea != null)
        {
            Destroy(_debugSpawnArea);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var observation = new List<float>();
        _nico.GetDriveTargets(observation);
        sensor.AddObservation(observation);
        sensor.AddObservation(EffectorTargeting.GetDirectionToTarget(Effector.transform, Target.transform.position));
        sensor.AddObservation(EffectorTargeting.GetPointingDirection(Effector.transform));
        sensor.AddObservation(Effector.transform.position);
        sensor.AddObservation(EffectorTargeting.GetDistanceToTarget(Effector.transform.position, Target.transform.position));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        const float MaxRange = Mathf.Deg2Rad * 0.1f;
        const float ChangeMagnitude = Mathf.Deg2Rad * 0.05f;

        int actionCount = actions.ContinuousActions.Length;
        for (int i = 0; i < _dofs; ++i)
        {
            float delta = (i < actionCount) ? actions.ContinuousActions[i] : 0f;
            _changes[i] = Mathf.Clamp(_changes[i] + delta * ChangeMagnitude, -MaxRange, MaxRange);
        }

        for (int i = 0; i < _dofs; ++i)
        {
            _targets[i] = Mathf.Clamp(
                _targets[i] + _changes[i],
                Mathf.Deg2Rad * _lowLimits[i],
                Mathf.Deg2Rad * _highLimits[i]
            );
        }

        _nico.SetDriveTargets(_targets);

        // --- Reward --- 
        float angleError = effectorDirectionCalculator.GetAngleErrorDegrees();
        float distError = effectorDirectionCalculator.GetDistanceToTarget();

    }

}
