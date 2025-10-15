using UnityEngine;

public class FingerPointerLine : MonoBehaviour
{
    [Header("Pointing Setup")]
    [Tooltip("Assign the fingertip Transform in the Inspector")]
    public Transform fingerTip;
    [Tooltip("Adjust Line Length")]
    public float lineLength = 0.05f;
    private LineRenderer lineRenderer;
    private Vector3 dirCorrectionLocal = new Vector3(1.6f, 0, 0);
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
        Vector3 direction = fingerTip.up; // assumes finger is pointing along its local Y axis
        Vector3 endPoint = startPoint + direction * lineLength + fingerTip.TransformDirection(dirCorrectionLocal);

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }
}
