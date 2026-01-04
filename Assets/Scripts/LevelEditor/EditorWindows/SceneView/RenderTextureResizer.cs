using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class RenderTextureResizer : MonoBehaviour
{
    public Camera renderCamera; // Камера, которая рендерит в текстуру
    private RawImage _rawImage;
    private RectTransform _rectTransform;
    private Vector2 _lastSize;

    void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Проверяем, изменился ли размер RectTransform
        Vector2 currentSize = _rectTransform.rect.size;

        if (currentSize.x != _lastSize.x || currentSize.y != _lastSize.y)
        {
            ResizeTexture((int)currentSize.x, (int)currentSize.y);
            _lastSize = currentSize;
        }
    }

    void ResizeTexture(int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        // Если текстура уже есть, освобождаем её из памяти
        if (_rawImage.texture != null)
        {
            RenderTexture oldRt = _rawImage.texture as RenderTexture;
            if (oldRt != null)
            {
                oldRt.Release();
            }
        }

        // Создаем новую Render Texture с актуальными размерами
        RenderTexture rt = new RenderTexture(width, height, 24);
        rt.antiAliasing = 1; // Можно настроить (1, 2, 4, 8)
        
        // Назначаем новую текстуру в Raw Image и в камеру
        _rawImage.texture = rt;
        if (renderCamera != null)
        {
            renderCamera.targetTexture = rt;
        }
    }
}