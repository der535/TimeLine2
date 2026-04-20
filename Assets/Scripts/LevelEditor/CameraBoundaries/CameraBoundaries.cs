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
            
            // 1. Вычисляем ширину линии в 1 пиксель
            // Используем pixelHeight камеры. Если камера рендерит в RenderTexture, 
            // cam.pixelHeight вернет высоту этой текстуры.
            float unitPerPixel = (cam.orthographicSize * 2f) / cam.pixelHeight;

            lineRenderer.transform.position = _references.playCamera.transform.position;
            
            // Устанавливаем толщину линии
            lineRenderer.startWidth = unitPerPixel;
            lineRenderer.endWidth = unitPerPixel;

            float height = _references.playCamera.orthographicSize;
            float width = height * _references.playCamera.aspect;
            Vector3 center = _references.playCamera.transform.position;

            // Смещение на пол-пикселя (0.5f * unitPerPixel), чтобы рамка шла 
            // строго по краю или чуть снаружи/внутри
            float halfPixel = unitPerPixel * 0.5f;

            // Вычисляем углы с учетом рассчитанной толщины
            Vector3 topLeft     = center + new Vector3(-width - halfPixel,  height + halfPixel, -center.z);
            Vector3 topRight    = center + new Vector3( width + halfPixel,  height + halfPixel, -center.z);
            Vector3 bottomRight = center + new Vector3( width + halfPixel, -height - halfPixel, -center.z);
            Vector3 bottomLeft  = center + new Vector3(-width - halfPixel, -height - halfPixel, -center.z);

            lineRenderer.SetPosition(0, topLeft);
            lineRenderer.SetPosition(1, topRight);
            lineRenderer.SetPosition(2, bottomRight);
            lineRenderer.SetPosition(3, bottomLeft);
        }
    }
}