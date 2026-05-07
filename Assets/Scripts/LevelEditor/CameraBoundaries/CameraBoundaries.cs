using TimeLine.LevelEditor.Core;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.CameraBoundaries
{
    public class CameraBoundaries : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineWidth;
        
        private CameraReferences _references;
        
        [Inject]
        private void Construct(CameraReferences cameraReferences)
        {
            _references = cameraReferences;
        }

        void Start()
        {
            // Настройка LineRenderer для замыкания рамки
            lineRenderer.positionCount = 4; // 4 угла + возврат в начало
            lineRenderer.loop = true;
            UpdateBounds();
        }

        private void Update()
        {
            UpdateBounds();
        }


        private void UpdateBounds()
        {
            if (_references?.playCamera == null) return;

            Camera cam = _references.editSceneCamera;
            Camera playCam = _references.playCamera;

            // 1. Вычисляем размер пикселя для корректной толщины линии
            float unitPerPixel = (cam.orthographicSize * 2f) / cam.pixelHeight;
    
            lineRenderer.transform.position = playCam.transform.position;
            lineRenderer.startWidth = unitPerPixel;
            lineRenderer.endWidth = unitPerPixel;

            // 2. ФИКСИРУЕМ АСПЕКТ
            // Вместо playCam.aspect используем жестко заданное соотношение
            float targetAspect = 16f / 9f;
    
            float height = playCam.orthographicSize;
            float width = height * targetAspect; // Теперь ширина всегда 16/9 от высоты
    
            Vector3 center = playCam.transform.position;
            float halfPixel = unitPerPixel * 0.5f;

            // Вычисляем углы (Z обнуляем относительно камеры, если нужно в 2D)
            Vector3 topLeft     = new Vector3(-width - halfPixel,  height + halfPixel, 0);
            Vector3 topRight    = new Vector3( width + halfPixel,  height + halfPixel, 0);
            Vector3 bottomRight = new Vector3( width + halfPixel, -height - halfPixel, 0);
            Vector3 bottomLeft  = new Vector3(-width - halfPixel, -height - halfPixel, 0);

            // Устанавливаем позиции локально, так как мы привязали LineRenderer к позиции камеры
            lineRenderer.SetPosition(0, topLeft);
            lineRenderer.SetPosition(1, topRight);
            lineRenderer.SetPosition(2, bottomRight);
            lineRenderer.SetPosition(3, bottomLeft);
        }
    }
}