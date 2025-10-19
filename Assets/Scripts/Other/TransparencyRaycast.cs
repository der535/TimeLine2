using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

public class PixelPerfectClick : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Texture2D _spriteTexture;
    private Sprite _sprite;
    
    private TrackObjectStorage _trackObjectStorage;
    private SelectObjectController _selectObjectController;

    [Inject]
    void Construct(TrackObjectStorage trackObjectStorage, SelectObjectController selectObjectController)
    {
        _trackObjectStorage = trackObjectStorage;
        _selectObjectController = selectObjectController;
    }

    internal void Setup(SpriteRenderer spriteRenderer)
    {
        _spriteRenderer = spriteRenderer;
        _sprite = _spriteRenderer.sprite;
        
        // Проверяем, есть ли текстура у спрайта
        if (_sprite != null && _sprite.texture != null)
        {
            _spriteTexture = _sprite.texture;
        }
        // Если текстуры нет, то считаем объект полностью непрозрачным (белый квадрат)
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (IsPixelOpaque(mousePos))
            {
                _selectObjectController.Select(_trackObjectStorage.GetTrackObjectDataOrParentGroupBySceneObject(gameObject), UnityEngine.Input.GetKey(KeyCode.LeftShift));
                Debug.Log("Клик по объекту!");
                // Здесь можно вызвать нужное действие
            }
        }
    }

    bool IsPixelOpaque(Vector2 worldPos)
    {
        // Переводим мировые координаты в локальные
        Vector2 localPos = transform.InverseTransformPoint(worldPos);

        // Проверяем, находится ли точка внутри bounds спрайта
        Bounds bounds = _spriteRenderer.bounds;
        if (!bounds.Contains(worldPos))
            return false;

        // Если у спрайта нет текстуры, считаем его полностью непрозрачным
        if (_sprite == null || _spriteTexture == null)
        {
            return true; // Белый квадрат - полностью непрозрачный
        }

        // Получаем UV координаты
        Rect textureRect = _sprite.textureRect;
        Vector2 uv = new Vector2(
            (localPos.x + _sprite.bounds.extents.x) / (_sprite.bounds.extents.x * 2),
            (localPos.y + _sprite.bounds.extents.y) / (_sprite.bounds.extents.y * 2)
        );

        // Переводим UV в пиксельные координаты на текстуре
        int x = Mathf.FloorToInt(uv.x * textureRect.width) + (int)textureRect.x;
        int y = Mathf.FloorToInt(uv.y * textureRect.height) + (int)textureRect.y;

        // Проверяем, не вышли ли за границы текстуры
        if (x < 0 || x >= _spriteTexture.width || y < 0 || y >= _spriteTexture.height)
            return false;

        // Получаем цвет пикселя
        Color pixelColor = _spriteTexture.GetPixel(x, y);

        // Если альфа > 0.1 — пиксель не прозрачный
        return pixelColor.a > 0.1f;
    }
}