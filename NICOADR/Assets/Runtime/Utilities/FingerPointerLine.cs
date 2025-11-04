using UnityEngine;

public class FingerPointerLine : MonoBehaviour
{
    public enum LocalAxis { X, Y, Z }

    [Header("Pointing Setup")]
    [Tooltip("Assign the fingertip Transform in the Inspector")]
    public Transform fingerTip;

    [Tooltip("Which local axis of the fingerTip should be considered 'forward'?")]
    public LocalAxis pointingAxis = LocalAxis.Y;

    [Tooltip("Adjust Line Length")]
    public float lineLength = 0.05f;

    private LineRenderer lineRenderer;
    private static Material sLineMat;

    private Vector3 GetLocalAxisVector()
    {
        switch (pointingAxis)
        {
            case LocalAxis.X: return Vector3.right;
            case LocalAxis.Y: return Vector3.up;
            case LocalAxis.Z: return Vector3.forward;
            default: return Vector3.up;
        }
    }

    void Start()
    {
        // Ensure single LineRenderer
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (!lineRenderer) lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.002f;
        lineRenderer.endWidth = 0.01f;

        if (!sLineMat) sLineMat = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = sLineMat;

        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        if (!fingerTip) return;

        Vector3 startPoint = fingerTip.position;
        Vector3 worldDir = fingerTip.TransformDirection(GetLocalAxisVector());
        Vector3 endPoint = startPoint + worldDir * lineLength;

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }
}
