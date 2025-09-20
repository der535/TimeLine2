using System;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace TimeLine
{
    public class BezierPoint : MonoBehaviour
    {
        [SerializeField] private RectTransform point;         
        [SerializeField] private RectTransform tangentLeft;         
        [SerializeField] private RectTransform tangentRight;

        public Action onValueChanged;

        private bool _isDraging;
        private bool _isDragingTangleLeft;
        private bool _isDragingTangleRight;
        
        public Vector3 Point => point.anchoredPosition;
        public Vector3 TangentLeft => tangentLeft.anchoredPosition + point.anchoredPosition;
        public Vector3 TangentRight => tangentRight.anchoredPosition + point.anchoredPosition;

        private MainObjects _mainObjects;
        
        [Inject]
        private void Construct(MainObjects mainObject)
        {
            _mainObjects = mainObject;
        }
        
        public void DragTangentLeftPoint(bool drag)
        {
            _isDragingTangleLeft = drag;
        }
        
        public void DragTangentRightPoint(bool drag)
        {
            _isDragingTangleRight = drag;
        }

        public void DragPoint(bool drag)
        {
            _isDraging = drag;
        }

        private Vector2 GetMousePosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle
            (_mainObjects.CanvasRectTransform, Mouse.current.position.ReadValue(), _mainObjects.MainCamera,
                out var localPoint);
            return localPoint;
        }

        private void Update()
        {
            if (_isDraging)
            {
                point.anchoredPosition = GetMousePosition();
                onValueChanged?.Invoke();
            }

            if (_isDragingTangleLeft)
            {
                tangentLeft.anchoredPosition = GetMousePosition() - (Vector2)Point;
                onValueChanged?.Invoke();
                tangentRight.anchoredPosition = tangentLeft.anchoredPosition * -1;
            }
            
            if (_isDragingTangleRight)
            {
                tangentRight.anchoredPosition = GetMousePosition() - (Vector2)Point;
                onValueChanged?.Invoke();
                tangentLeft.anchoredPosition = tangentRight.anchoredPosition * -1;
            }
        }
    }
}