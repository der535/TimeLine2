using TimeLine.LevelEditor.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SceneToRawImageConverter : MonoBehaviour
{
    [Header("Ссылки")] public Camera mapCamera; // Камера, которая рендерит 3D сцену
    public RawImage rawImageDisplay; // UI элемент, где отображается эта сцена
    public Canvas mainUiCanvas; // Основной Canvas, где лежит RawImage (для проверки RenderMode)
    private CameraReferences _cameraReferences;

    [Inject]
    private void Construct(CameraReferences references)
    {
        _cameraReferences = references;
    }

    /// <summary>
    /// Переводит точку из 3D мира сцены в позицию UI инструмента (World Space UI)
    /// </summary>
    public Vector3 WorldToUIPosition(Vector3 worldScenePosition)
    {
        if (!mapCamera || !rawImageDisplay) return Vector3.zero;

        // 1. 3D World -> Viewport (0..1)
        Vector3 viewportPoint = mapCamera.WorldToViewportPoint(worldScenePosition);

        // 2. Viewport -> Локальные координаты RawImage
        RectTransform rt = rawImageDisplay.rectTransform;
        float localX = (viewportPoint.x - rt.pivot.x) * rt.rect.width;
        float localY = (viewportPoint.y - rt.pivot.y) * rt.rect.height;

        // 3. Локальные RawImage -> Мировые координаты UI (учитывает позицию и наклон RawImage)
        return rt.TransformPoint(new Vector2(localX, localY));
    }

    public Vector2 WorldToUIAnchoredPosition(Vector3 worldScenePosition, RectTransform parentRect)
    {
        if (!mapCamera || !rawImageDisplay) return Vector2.zero;

        // 1. Из 3D мира в Viewport (0..1)
        Vector3 viewportPoint = mapCamera.WorldToViewportPoint(worldScenePosition);

        // 2. Из Viewport в мировые координаты UI пространства
        RectTransform rt = rawImageDisplay.rectTransform;
        float localX = (viewportPoint.x - rt.pivot.x) * rt.rect.width;
        float localY = (viewportPoint.y - rt.pivot.y) * rt.rect.height;
        Vector3 worldUIPos = rt.TransformPoint(new Vector2(localX, localY));

        // 3. Из мировых координат UI в локальные координаты ПРЕДКА (anchoredPosition)
        // InverseTransformPoint переводит мировую позицию в локальную систему координат родителя
        return parentRect.InverseTransformPoint(worldUIPos);
    }

    /// <summary>
    /// Конвертирует позицию курсора (Screen Point) в координаты 3D мира сцены.
    /// </summary>
    /// <param name="screenPoint">Обычно Input.mousePosition</param>
    /// <param name="distanceFromCamera">Расстояние от камеры вглубь сцены</param>
    public Vector3 ScreenPointToWorldScene(Vector2 screenPoint, float distanceFromCamera = 10f)
    {
        if (!mapCamera || !rawImageDisplay) return Vector3.zero;

        RectTransform rawRect = rawImageDisplay.rectTransform;

        // 1. Переводим экранную позицию мыши в локальные координаты внутри RawImage
        // Мы передаем null в качестве камеры, если Canvas находится в режиме ScreenSpaceOverlay
        // Если Canvas в WorldSpace или ScreenSpaceCamera, нужно передать камеру этого Canvas'а.
        Canvas canvas = rawImageDisplay.canvas;
        Camera canvasCam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawRect, screenPoint, canvasCam,
                out Vector2 localPoint))
        {
            // 2. Преобразуем локальные координаты в нормализованные (Viewport: 0..1)
            // Учитываем Pivot (центр вращения/привязки)
            float viewportX = (localPoint.x / rawRect.rect.width) + rawRect.pivot.x;
            float viewportY = (localPoint.y / rawRect.rect.height) + rawRect.pivot.y;

            Vector3 viewportPoint = new Vector3(viewportX, viewportY, distanceFromCamera);
            return mapCamera.ViewportToWorldPoint(viewportPoint);
        }

        return Vector3.zero;
    }

    public Vector3? UIToWorldPosition(Vector3 toolWorldPosition, RectTransform rectTransform = null)
    {
        // 1. ПЕРЕВОДИМ ПОЗИЦИЮ ИНСТРУМЕНТА В VIEWPORT (0...1) РЕНДЕР-ТЕКСТУРЫ
        // Сначала получаем локальную точку внутри RawImage
        RectTransform rawRect = rawImageDisplay.rectTransform;

        // // Используем позицию инструмента в мировом пространстве UI
        // Vector3 toolWorldPos = toolTransform.position;
        //     

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform == null ? rawRect : rectTransform,
                toolWorldPosition, null,
                out Vector2 localInRaw))
        {
            return null;
        }

        // Конвертируем локальные координаты в нормализованные Viewport координаты (от 0 до 1)
        // Учитываем размеры Rect и его Pivot
        float viewportX = (localInRaw.x / rawRect.rect.width) + rawRect.pivot.x;
        float viewportY = (localInRaw.y / rawRect.rect.height) + rawRect.pivot.y;

        // 2. КОНВЕРТИРУЕМ VIEWPORT В МИРОВЫЕ КООРДИНАТЫ СЦЕНЫ
        // Для ViewportToWorldPoint нужно указать расстояние (Z), на котором находится объект от камеры

        Vector3 viewportPoint = new Vector3(viewportX, viewportY, 0);

        Vector3 smoothWorldPosition3D = mapCamera.ViewportToWorldPoint(viewportPoint);
        return smoothWorldPosition3D;
    }


    public Vector3 GetWorldPositionFromMouseOnRawImage()
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rawImageDisplay.rectTransform,
                Input.mousePosition,
                _cameraReferences.editUICamera, // Используйте камеру UI, если Canvas в режиме Render Mode: Camera
                out Vector2 localPoint))
        {
            // 1. Получаем размеры Rect
            Rect r = rawImageDisplay.rectTransform.rect;

            // 2. Преобразуем локальную точку в нормализованные координаты Viewport (0.0 - 1.0)
            // Приводим координаты так, чтобы левый нижний угол RawImage был (0,0)
            float viewportX = (localPoint.x - r.x) / r.width;
            float viewportY = (localPoint.y - r.y) / r.height;


            Vector3 viewportPoint = new Vector3(viewportX, viewportY, 10);

            // 3. Преобразуем из Viewport в World Space
            return _cameraReferences.editSceneCamera.ViewportToWorldPoint(viewportPoint);
        }

        return Vector3.zero;
    }
}