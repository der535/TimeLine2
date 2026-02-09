using System.Collections.Generic;
using UnityEngine;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.General;

namespace TimeLine.EdgeColliderEditor
{
    public class EdgeColliderModel : IEdgeColliderModel
    {
        private readonly ListVector2Parameter _parameter;
        private readonly EdgeCollider2DComponent _component;
        private readonly EdgeCollider2D _collider;
        private readonly FloatParameter _edgeRadius;

        public EdgeColliderModel(
            ListVector2Parameter parameter, 
            EdgeCollider2DComponent component, 
            EdgeCollider2D collider,
            FloatParameter edgeRadius)
        {
            _parameter = parameter;
            _component = component;
            _collider = collider;
            _edgeRadius = edgeRadius;
        }

        public List<Vector2> Points
        {
            get => new List<Vector2>(_collider.points);
            set
            {
                _collider.points = value.ToArray();
                _parameter?.SetValue(value);
            }
        }

        public float EdgeRadius => _edgeRadius.Value;

        public void BeginUpdate() => _component?.BeginParameterUpdate();
        public void EndUpdate() => _component?.EndParameterUpdate();
    }
}