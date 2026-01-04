using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Circle With Outline")]
public class CircleWithOutline : Graphic
{
    [Header("Circle Settings")]
    [Range(3, 128)] public int segments = 32;
    public float thickness = 10f;
    public Color outlineColor = Color.black;
    public Color fillColor = Color.white;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        float radius = Mathf.Min(width, height) * 0.5f;
        
        // Корректировка толщины
        float innerRadius = Mathf.Max(0, radius - thickness);
        
        // Создание центральной точки (для заполнения)
        UIVertex centerVert = UIVertex.simpleVert;
        centerVert.color = fillColor;
        centerVert.position = Vector3.zero;
        vh.AddVert(centerVert);
        
        // Создание точек для круга
        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.PI * 2 * i / segments;
            Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
            
            // Внутренняя точка (заполнение)
            UIVertex innerVert = UIVertex.simpleVert;
            innerVert.color = fillColor;
            innerVert.position = dir * innerRadius;
            vh.AddVert(innerVert);
            
            // Внешняя точка (обводка)
            UIVertex outerVert = UIVertex.simpleVert;
            outerVert.color = outlineColor;
            outerVert.position = dir * radius;
            vh.AddVert(outerVert);
        }
        
        // Генерация треугольников для заполнения
        for (int i = 1; i <= segments; i++)
        {
            int current = i * 2;
            int next = (i % segments == 0) ? 1 : i * 2 + 1;
            vh.AddTriangle(0, current, next);
        }
        
        // Генерация треугольников для обводки
        for (int i = 0; i < segments; i++)
        {
            int innerCurrent = i * 2 + 1;
            int innerNext = ((i + 1) % segments) * 2 + 1;
            int outerCurrent = i * 2 + 2;
            int outerNext = ((i + 1) % segments) * 2 + 2;
            
            vh.AddTriangle(innerCurrent, outerCurrent, innerNext);
            vh.AddTriangle(outerCurrent, outerNext, innerNext);
        }
    }

    // Обновляем меш при изменении свойств в инспекторе
    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
    #endif
}