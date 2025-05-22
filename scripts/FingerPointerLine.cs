using UnityEngine;

public class FingerPointerLine : MonoBehaviour
{
    public Transform fingerTip;         // Assign the fingertip Transform in the Inspector
    public float lineLength = 5f;       // Length of the pointing line
    private LineRenderer lineRenderer;

    void Start()
    {
        // Add a LineRenderer component if not already present
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.002f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        if (fingerTip == null) return;

        Vector3 startPoint = fingerTip.position;
        Vector3 direction = fingerTip.up; // assumes finger is pointing along its Z+ axis
        Vector3 endPoint = startPoint + direction * lineLength;

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }
}
