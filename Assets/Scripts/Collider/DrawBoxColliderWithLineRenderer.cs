using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DrawBoxColliderWithLineRenderer : MonoBehaviour
{
    [SerializeField] private Color lineColor = Color.green;
    [SerializeField] private float lineWidth = 0.02f;

    private BoxCollider boxCollider;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 24; // 12 ребер * 2 точки на ребро
    }

    private void Update()
    {
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        Vector3 center = transform.TransformPoint(boxCollider.center);
        Vector3 extents = boxCollider.size * 0.5f;

        Vector3[] corners = new Vector3[8]
        {
            center + transform.TransformVector(new Vector3(-extents.x, -extents.y, -extents.z)),
            center + transform.TransformVector(new Vector3( extents.x, -extents.y, -extents.z)),
            center + transform.TransformVector(new Vector3( extents.x,  extents.y, -extents.z)),
            center + transform.TransformVector(new Vector3(-extents.x,  extents.y, -extents.z)),

            center + transform.TransformVector(new Vector3(-extents.x, -extents.y,  extents.z)),
            center + transform.TransformVector(new Vector3( extents.x, -extents.y,  extents.z)),
            center + transform.TransformVector(new Vector3( extents.x,  extents.y,  extents.z)),
            center + transform.TransformVector(new Vector3(-extents.x,  extents.y,  extents.z))
        };

        // 12 ребер
        Vector3[] linePoints = new Vector3[24];
        int index = 0;

        int[,] edges = new int[,]
        {
            {0,1}, {1,2}, {2,3}, {3,0}, // нижняя грань
            {4,5}, {5,6}, {6,7}, {7,4}, // верхняя грань
            {0,4}, {1,5}, {2,6}, {3,7}  // вертикальные ребра
        };

        for (int i = 0; i < edges.GetLength(0); i++)
        {
            linePoints[index++] = corners[edges[i, 0]];
            linePoints[index++] = corners[edges[i, 1]];
        }

        lineRenderer.SetPositions(linePoints);
    }
}