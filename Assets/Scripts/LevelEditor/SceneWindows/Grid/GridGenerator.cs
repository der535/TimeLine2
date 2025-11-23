using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public class GridGenerator : MonoBehaviour
{
    [Header("Grid Settings")] public float gridWidth = 10f;
    public float gridHeight = 10f;
    public float horizontalSpacing = 1f;
    public float verticalSpacing = 1f;
    public float lineThickness = 0.05f;
    public Color gridColor = Color.white;

    private Mesh gridMesh;
    private Material gridMaterial;

    void Start()
    {
        GenerateGrid();
    }

    void OnValidate()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        // Проверка корректности параметров
        if (gridWidth <= 0 || gridHeight <= 0 ||
            horizontalSpacing <= 0 || verticalSpacing <= 0 ||
            lineThickness <= 0)
        {
            Debug.LogError("Invalid grid parameters! All values must be positive.");
            return;
        }

        InitializeComponents();
        CreateGridMesh();
        UpdateMaterial();
    }

    private void InitializeComponents()
    {
        if (gridMesh == null)
        {
            gridMesh = new Mesh();
            gridMesh.name = "GridMesh";
            GetComponent<MeshFilter>().mesh = gridMesh;
        }
    }

    private void CreateGridMesh()
    {
        // Рассчитываем количество линий
        int horizontalLines = Mathf.FloorToInt(gridHeight / verticalSpacing) + 1;
        int verticalLines = Mathf.FloorToInt(gridWidth / horizontalSpacing) + 1;

        // Рассчитываем вершины и треугольники
        int quadsCount = horizontalLines + verticalLines;
        int verticesCount = quadsCount * 4;
        int trianglesCount = quadsCount * 6;

        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[trianglesCount];
        Vector2[] uv = new Vector2[verticesCount];

        int vertexIndex = 0;
        int triangleIndex = 0;

        // Создаем горизонтальные линии
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

        // Создаем вертикальные линии
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

        // Применяем данные к мешу
        gridMesh.Clear();
        gridMesh.vertices = vertices;
        gridMesh.triangles = triangles;
        gridMesh.uv = uv;
        gridMesh.RecalculateBounds();
        gridMesh.RecalculateNormals();
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

        // Рассчитываем 4 вершины для линии-прямоугольника
        vertices[vertexIndex] = start - perpendicular; // Левый нижний
        vertices[vertexIndex + 1] = start + perpendicular; // Правый нижний
        vertices[vertexIndex + 2] = end + perpendicular; // Правый верхний
        vertices[vertexIndex + 3] = end - perpendicular; // Левый верхний

        // Создаем два треугольника
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
        if (gridMaterial == null)
        {
            gridMaterial = new Material(Shader.Find("Unlit/Color"));
            gridMaterial.color = gridColor;
            GetComponent<MeshRenderer>().material = gridMaterial;
        }
        else
        {
            gridMaterial.color = gridColor;
        }
    }

    // Метод для ручного обновления сетки
    public void UpdateGrid()
    {
        GenerateGrid();
    }
}