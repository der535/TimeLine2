using EventBus;
using UnityEngine;
using UnityEngine.Events;
using Radishmouse;
using TimeLine.EventBus.Events.ValueEditor;
using TimeLine.LevelEditor.Core;
using UnityEngine.Serialization;
using Zenject; // Если используешь этот LineRenderer

namespace TimeLine.LevelEditor.ValueEditor
{
    public class NodeConnectionDetector : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private float detectionThreshold = 15f; // Чувствительность (в пикселях)

        [SerializeField] private NodeConnection _nodeConnection;
        [SerializeField] private UILineRenderer _lineRenderer;
         private RectTransform parentObject;
        private bool _isHovered;

        private GameEventBus _gameEventBus;
        private CameraReferences _cameraReferences;
        private Node _node;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, CameraReferences references)
        {
            _gameEventBus = gameEventBus;
            _cameraReferences = references;
        }



        private void Update()
        {
            if(parentObject == null) return;
            
            Vector2 localMousePos = GetLocalMousePosition();
            bool isNear = IsMouseNearLine(localMousePos);

            // Логика наведения (Hover)
            if (isNear && !_isHovered)
            {
                _isHovered = true;
                OnMouseHoverEnter();
            }
            else if (!isNear && _isHovered)
            {
                _isHovered = false;
                OnMouseHoverExit();
            }

            // Логика клика
            if (_isHovered && UnityEngine.Input.GetMouseButtonDown(0))
            {
                OnConnectionClick();
            }
        }

        private void OnMouseHoverEnter()
        {
            // print("Enter");
        }

        private void OnMouseHoverExit()
        {
            // print("Exit");
        }

        private void OnConnectionClick()
        {
            _gameEventBus.Raise(new SelectNodeConnectionEvent(_nodeConnection));
        }

        private bool IsMouseNearLine(Vector2 mousePos)
        {
            Vector2[] points = _lineRenderer.GetPoints();
            if (points == null || points.Length < 2) return false;

            // Проходим по всем сегментам линии
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (DistanceToSegment(mousePos, points[i], points[i + 1]) < detectionThreshold)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Математика: находит кратчайшее расстояние от точки P до отрезка AB
        /// </summary>
        private float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a;
            Vector2 ba = b - a;

            // Проекция точки на вектор отрезка с ограничением [0, 1]
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));

            // Расстояние от точки до ближайшей точки на отрезке
            return (pa - ba * h).magnitude;
        }

        private Vector2 GetLocalMousePosition()
        {
            // Получаем позицию мыши относительно родителя (linesRoot)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentObject,
                UnityEngine.Input.mousePosition,
                _cameraReferences.editUICamera,
                out Vector2 localPos
            );
            return localPos;
        }

        public void Setup(RectTransform rootLineRenderer)
        {
            parentObject = rootLineRenderer;
        }
    }
}