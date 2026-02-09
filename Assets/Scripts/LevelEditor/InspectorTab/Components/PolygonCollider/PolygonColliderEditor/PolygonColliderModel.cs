using System.Collections.Generic;
using UnityEngine;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.General;

namespace TimeLine.EdgeColliderEditor
{
    public class PolygonColliderModel : IPolygonColliderModel
    {
        private readonly ListVector2Parameter _parameter;
        private readonly PolygonCollider2DComponent _component;
        private readonly PolygonCollider2D _collider;

        public PolygonColliderModel(
            ListVector2Parameter parameter, 
            PolygonCollider2DComponent component, 
            PolygonCollider2D collider)
        {
            _parameter = parameter;
            _component = component;
            _collider = collider;
        }

        public List<Vector2> Points
        {
            get => new List<Vector2>(_collider.points);
            set
            {
                if (value.Count < 3) 
                {
                    // Генерируем треугольник, а не точки на одной линии!
                    value = new List<Vector2> { 
                        Vector2.zero, 
                        Vector2.right * 0.5f, 
                        Vector2.up * 0.5f 
                    };
                }
                // Важно: PolygonCollider2D автоматически замыкает контур!
                _collider.points = value.ToArray();
                _parameter?.SetValue(value);
            }
        }

        public void BeginUpdate() => _component?.BeginParameterUpdate();
        public void EndUpdate() => _component?.EndParameterUpdate();
    }
}