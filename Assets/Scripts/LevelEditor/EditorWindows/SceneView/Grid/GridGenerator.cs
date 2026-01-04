using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public class GridGenerator : MonoBehaviour
{
    [Header("Grid Settings")] 
    public float gridWidth = 10f;
    public float gridHeight = 10f;
    public float horizontalSpacing = 1f;
    public float verticalSpacing = 1f;
    public float lineThickness = 0.05f;
    public Color gridColor = Color.white;
    public int sortingOrder = -1;

    private Mesh gridMesh;
    private Material gridMaterial;

    void Start()
    {
        GenerateGrid();
    }

    void OnValidate()
    {
        // FIX: Delay the generation until the Editor is done with current checks
#if UNITY_EDITOR
        EditorApplication.delayCall += () =>
        {
            if (this != null) GenerateGrid();
        };
#endif
    }

    public void GenerateGrid()
    {
        if (gridWidth <= 0 || gridHeight <= 0 ||
            horizontalSpacing <= 0 || verticalSpacing <= 0 ||
            lineThickness <= 0)
        {
            return;
        }

        InitializeComponents();
        CreateGridMesh();
        UpdateMaterial();
    }

    private void InitializeComponents()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        
        
        // FIX: Reuse existing mesh if possible to prevent memory garbage
        if (mf.sharedMesh != null)
        {
            gridMesh = mf.sharedMesh;
        }
        else
        {
            gridMesh = new Mesh();
            gridMesh.name = "GridMesh";
            mf.mesh = gridMesh;
        }
    }

    private void CreateGridMesh()
    {
        // Safety check if mesh was deleted manually
        if (gridMesh == null) InitializeComponents();

        gridMesh.Clear();

        int horizontalLines = Mathf.FloorToInt(gridHeight / verticalSpacing) + 1;
        int verticalLines = Mathf.FloorToInt(gridWidth / horizontalSpacing) + 1;

        int quadsCount = horizontalLines + verticalLines;
        int verticesCount = quadsCount * 4;
        int trianglesCount = quadsCount * 6;

        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[trianglesCount];
        
        // Note: UVs are not strictly necessary for unlit color, but good practice
        // Vector2[] uv = new Vector2[verticesCount]; 

        int vertexIndex = 0;
        int triangleIndex = 0;

        // Horizontal Lines
        for (int i = 0; i < horizontalLines; i++)
        {
            float y = i * verticalSpacing;
            CreateLineQuad(
                new Vector3(0, y, 0),
                new Vector3(gridWidth, y, 0),
                lineThickness,
                ref vertexIndex,
                ref triangleIndex,
                vertices,
                triangles
            );
        }

        // Vertical Lines
        for (int j = 0; j < verticalLines; j++)
        {
            float x = j * horizontalSpacing;
            CreateLineQuad(
                new Vector3(x, 0, 0),
                new Vector3(x, gridHeight, 0),
                lineThickness,
                ref vertexIndex,
                ref triangleIndex,
                vertices,
                triangles
            );
        }

        gridMesh.vertices = vertices;
        gridMesh.triangles = triangles;
        
        // Optimizations
        gridMesh.RecalculateBounds();
        // RecalculateNormals is usually not needed for a flat 2D grid unless lit
        // gridMesh.RecalculateNormals(); 
    }

    private void CreateLineQuad(
        Vector3 start,
        Vector3 end,
        float thickness,
        ref int vertexIndex,
        ref int triangleIndex,
        Vector3[] vertices,
        int[] triangles)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0) * thickness / 2;

        vertices[vertexIndex] = start - perpendicular; 
        vertices[vertexIndex + 1] = start + perpendicular; 
        vertices[vertexIndex + 2] = end + perpendicular; 
        vertices[vertexIndex + 3] = end - perpendicular; 

        triangles[triangleIndex] = vertexIndex;
        triangles[triangleIndex + 1] = vertexIndex + 1;
        triangles[triangleIndex + 2] = vertexIndex + 2;
        triangles[triangleIndex + 3] = vertexIndex;
        triangles[triangleIndex + 4] = vertexIndex + 2;
        triangles[triangleIndex + 5] = vertexIndex + 3;

        vertexIndex += 4;
        triangleIndex += 6;
    }

    private void UpdateMaterial()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        
        mr.sortingOrder = sortingOrder;
        
        // FIX: Prevent creating infinite new Materials in Editor
        if (mr.sharedMaterial == null)
        {
            gridMaterial = new Material(Shader.Find("Unlit/Color"));
            mr.material = gridMaterial;
        }
        else
        {
            // Use sharedMaterial to avoid "(Instance)" creation spam in Editor
            gridMaterial = mr.sharedMaterial; 
        }

        if (gridMaterial != null)
        {
            gridMaterial.color = gridColor;
        }
    }
}