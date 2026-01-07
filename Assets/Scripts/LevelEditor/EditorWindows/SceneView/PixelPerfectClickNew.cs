using EventBus;
using EventBus.Events;
using TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class PixelPerfectClickNew : MonoBehaviour, IPointerClickHandler
{
    public Camera mapCamera; // камера, что рендерит в RenderTexture
    public RawImage mapImage; // UI-элемент с RenderTexture


    private TrackObjectStorage _trackObjectStorage;
    private SelectObjectController _selectObjectController;
    private MainObjects _mainObjects;
    private ActionMap _actionMap;
    private GameEventBus _gameEventBus;

    private bool _emptyClick = true;

    [Inject]
    void Construct(TrackObjectStorage trackObjectStorage, SelectObjectController selectObjectController,
        MainObjects mainObjects, ActionMap actionMap, GameEventBus gameEventBus)
    {
        _trackObjectStorage = trackObjectStorage;
        _selectObjectController = selectObjectController;
        _mainObjects = mainObjects;
        _actionMap = actionMap;
        _gameEventBus = gameEventBus;
    }


    public void OnMapClick(PointerEventData eventData)
    {
        _emptyClick = true;
        // 1. Получить локальную точку на RawImage (в его RectTransform)
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapImage.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        // 2. Преобразовать в UV (0..1)
        RectTransform rt = mapImage.rectTransform;
        Vector2 uv = new Vector2(
            (localPoint.x + rt.rect.width / 2f) / rt.rect.width,
            (localPoint.y + rt.rect.height / 2f) / rt.rect.height
        );

        // 3. Преобразовать UV → Viewport → World
        Vector3 viewportPos = new Vector3(uv.x, uv.y, mapCamera.nearClipPlane);
        Vector3 worldPos = mapCamera.ViewportToWorldPoint(viewportPos);

        // Теперь worldPos — это позиция в мировом пространстве сцены
        Debug.Log($"World position: {worldPos}");


        foreach (var active in _trackObjectStorage.GetAllActiveTrackData())
        {
            OnMouseLeftStarted(worldPos, active.sceneObject.GetComponent<SpriteRenderer>(),
                active.sceneObject);
        }
        
        if (eventData.button != PointerEventData.InputButton.Left) 
            return;
        
        if (_emptyClick)
        {
            _emptyClick = false;
            _gameEventBus.Raise(new DeselectAllObjectEvent());
        }
    }

    private void OnMouseLeftStarted(Vector3 mousePosition, SpriteRenderer spriteRenderer, GameObject sceneObject)
    {
        if (IsPixelOpaque(mousePosition, spriteRenderer, sceneObject.transform))
        {
            _emptyClick = false;
            print("select");
            _gameEventBus.Raise(new ObjectUnderCursorEvent());
            TransformComponent transformComponent = FindTopmostParentWithComponent.Find<TransformComponent>(sceneObject.transform);
            TrackObjectData data;
            if (transformComponent != null)
            {
                if (sceneObject.gameObject.activeSelf == false) return;
                Debug.Log(sceneObject.gameObject, sceneObject.gameObject);
                data = _trackObjectStorage.GetTrackObjectData(transformComponent.gameObject);
            }
            else
            {
                if (sceneObject.gameObject.activeSelf == false) return;
                Debug.Log(sceneObject, sceneObject);
                data = _trackObjectStorage.GetTrackObjectData(sceneObject);
            }

            print(data);
            _selectObjectController.SelectMultiple(data);
        }
    }


    bool IsPixelOpaque(Vector2 worldPos, SpriteRenderer spriteRenderer, Transform objectTransform)
    {
        if (spriteRenderer == null) return false;

        // 1. Проверка попадания в bounds
        if (!spriteRenderer.bounds.Contains(worldPos))
            return false;

        if (spriteRenderer.sprite == null) return true;

        Sprite sprite = spriteRenderer.sprite;
        Texture2D tex = sprite.texture;
        if (tex == null) return true;

        // 2. Преобразуем мировую позицию → локальные координаты спрайта
        Vector2 localPos = objectTransform.InverseTransformPoint(worldPos);

        // 3. Нормализуем в UV (0..1) в пространстве спрайта
        Vector2 extents = sprite.bounds.extents;
        if (extents.x == 0 || extents.y == 0) return false;

        Vector2 uv = new Vector2(
            (localPos.x + extents.x) / (2f * extents.x),
            (localPos.y + extents.y) / (2f * extents.y)
        );

        // 4. Преобразуем UV → пиксель в текстуре
        Rect rect = sprite.textureRect;
        int x = Mathf.FloorToInt(uv.x * rect.width) + (int)rect.x;
        int y = Mathf.FloorToInt(uv.y * rect.height) + (int)rect.y;

        // 5. Проверка границ текстуры
        if (x < 0 || x >= tex.width || y < 0 || y >= tex.height)
            return false;

        // 6. Проверка альфы
        return tex.GetPixel(x, y).a > 0.1f;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        OnMapClick(eventData);
    }
}