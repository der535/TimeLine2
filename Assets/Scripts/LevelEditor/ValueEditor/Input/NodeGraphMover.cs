using UnityEngine;

namespace TimeLine.LevelEditor.ValueEditor
{
    public class NodeGraphController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform contentRoot; // Контейнер с нодами
        [SerializeField] private Canvas parentCanvas;

        [Header("Movement Settings")]
        [SerializeField] private float zoomSpeed = 0.5f;
        [SerializeField] private float minZoom = 0.3f;
        [SerializeField] private float maxZoom = 3.0f;

        private Vector3 _lastMousePosition;

        void Update()
        {
            HandlePan();
            HandleZoom();
        }

        private void HandlePan()
        {
            // 2 — это индекс средней кнопки мыши (колесико)
            if (UnityEngine.Input.GetMouseButtonDown(2))
            {
                _lastMousePosition = UnityEngine.Input.mousePosition;
            }

            if (UnityEngine.Input.GetMouseButton(2))
            {
                // Считаем разницу в движении мыши
                Vector3 delta = UnityEngine.Input.mousePosition - _lastMousePosition;
                
                // Делим на scaleFactor канваса, чтобы скорость не зависела от разрешения
                contentRoot.anchoredPosition += (Vector2)delta / parentCanvas.scaleFactor;
                
                _lastMousePosition = UnityEngine.Input.mousePosition;
            }
        }

        private void HandleZoom()
        {
            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 newScale = contentRoot.localScale + Vector3.one * scroll * zoomSpeed;
                
                // Ограничиваем масштаб
                float clampedScale = Mathf.Clamp(newScale.x, minZoom, maxZoom);
                contentRoot.localScale = new Vector3(clampedScale, clampedScale, 1f);
            }
        }
    }
}