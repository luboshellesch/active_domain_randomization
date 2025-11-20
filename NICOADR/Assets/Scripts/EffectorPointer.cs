using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EffectorPointer : MonoBehaviour
{
    [Tooltip("Transform representing the effector/fingertip.")]
    public Transform EffectorTransform;

    [Tooltip("Length of the pointer line.")]
    public float PointerLength = 3f;

    [Tooltip("Color of the pointer line.")]
    public Color PointerColor = Color.red;

    private LineRenderer _lineRenderer;
    private static Material _defaultMaterial;

    private const float StartWidth = 0.002f;
    private const float EndWidth = 0.002f;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.startWidth = StartWidth;
        _lineRenderer.endWidth = EndWidth;

        if (_defaultMaterial == null)
            _defaultMaterial = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.material = _defaultMaterial;

        _lineRenderer.startColor = PointerColor;
        _lineRenderer.endColor = PointerColor;
        _lineRenderer.positionCount = 2;
    }

    private void Update()
    {
        if (EffectorTransform == null)
            return;

        Vector3 startPoint = EffectorTransform.position;
        Vector3 direction = EffectorTargeting.GetPointingDirection(EffectorTransform);
        Vector3 endPoint = startPoint + direction * PointerLength;

        _lineRenderer.SetPosition(0, startPoint);
        _lineRenderer.SetPosition(1, endPoint);
    }
}
