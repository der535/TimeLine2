using System.Collections.Generic;
using System.Linq; // Добавлено для сортировки
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using EventBus;
using EventBus.Events;
using TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.General;

public class PixelPerfectClickNew : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform selectBoxScene;
    public Camera mapCamera;
    public RawImage mapImage;

    private TrackObjectStorage _trackObjectStorage;
    private SelectObjectController _selectObjectController;
    private C_EditColliderState _cEditColliderState;
    private GameEventBus _gameEventBus;

    private bool _emptyClick = true;
    
    // Поля для логики циклического выделения
    private List<TrackObjectData> _hitsAtLastPosition = new List<TrackObjectData>();
    private int _lastSelectedIndex = -1;

    [Inject]
    void Construct(TrackObjectStorage trackObjectStorage, SelectObjectController selectObjectController,
        GameEventBus gameEventBus, C_EditColliderState cEditColliderState)
    {
        _trackObjectStorage = trackObjectStorage;
        _selectObjectController = selectObjectController;
        _gameEventBus = gameEventBus;
        _cEditColliderState = cEditColliderState;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectBoxScene.gameObject.activeSelf || _cEditColliderState.GetState()) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapImage.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        RectTransform rt = mapImage.rectTransform;
        Vector2 uv = new Vector2(
            (localPoint.x + rt.rect.width / 2f) / rt.rect.width,
            (localPoint.y + rt.rect.height / 2f) / rt.rect.height
        );

        Vector3 viewportPos = new Vector3(uv.x, uv.y, mapCamera.nearClipPlane);
        Vector3 worldPos = mapCamera.ViewportToWorldPoint(viewportPos);

        ProcessClickSelection(worldPos);
    }

    private void ProcessClickSelection(Vector3 worldPos)
    {
        // 1. Находим все объекты под курсором
        List<TrackObjectData> currentHits = new List<TrackObjectData>();
        FindAllObjectsUnderCursor(worldPos, _trackObjectStorage.GetAllActiveTrackData(), currentHits);

        // 2. Сортируем: сначала те, у кого SortingOrder выше
        currentHits = currentHits
            .Select(h => new { 
                Data = h, 
                SR = h.sceneObject.GetComponentInChildren<SpriteRenderer>() 
            })
            .OrderByDescending(x => x.SR != null ? x.SR.sortingLayerID : -1)
            .ThenByDescending(x => x.SR != null ? x.SR.sortingOrder : -1)
            .Select(x => x.Data)
            .ToList();

        if (currentHits.Count == 0)
        {
            _gameEventBus.Raise(new DeselectAllObjectEvent());
            _hitsAtLastPosition.Clear();
            _lastSelectedIndex = -1;
            return;
        }

        // 3. Проверяем, совпадает ли набор объектов с прошлым разом (клик в то же место)
        bool isSameSet = currentHits.SequenceEqual(_hitsAtLastPosition);

        if (isSameSet)
        {
            _lastSelectedIndex = (_lastSelectedIndex + 1) % currentHits.Count;
        }
        else
        {
            _hitsAtLastPosition = currentHits;
            _lastSelectedIndex = 0;
        }

        // 4. Выделяем нужный объект
        var objectToSelect = _hitsAtLastPosition[_lastSelectedIndex];
        
        _gameEventBus.Raise(new ObjectUnderCursorEvent());
        _selectObjectController.SelectMultiple(objectToSelect);
    }

    private void FindAllObjectsUnderCursor(Vector3 mousePosition, List<TrackObjectData> list, List<TrackObjectData> results, TrackObjectData parentGroup = null)
    {
        foreach (var trackObjectData in list)
        {
            if (trackObjectData is TrackObjectGroup group)
            {
                // Если это группа, рекурсивно ищем внутри, но передаем группу как потенциальную цель выделения
                FindAllObjectsUnderCursor(mousePosition, group.TrackObjectDatas, results, parentGroup ?? group);
            }
            else
            {
                if (trackObjectData.sceneObject == null || !trackObjectData.sceneObject.activeSelf) continue;

                var sr = trackObjectData.sceneObject.GetComponent<SpriteRenderer>();
                if (IsPixelOpaque(mousePosition, sr, trackObjectData.sceneObject.transform))
                {
                    var target = parentGroup ?? trackObjectData;
                    // Добавляем в результаты только если этого объекта (или его группы) еще нет в списке
                    if (!results.Contains(target))
                    {
                        results.Add(target);
                    }
                }
            }
        }
    }

    // Метод IsPixelOpaque остается без изменений...
    bool IsPixelOpaque(Vector2 worldPos, SpriteRenderer spriteRenderer, Transform objectTransform)
    {
        if (spriteRenderer == null || !spriteRenderer.enabled) return false;
        if (!spriteRenderer.bounds.Contains(worldPos)) return false;
        if (spriteRenderer.sprite == null) return true;

        Sprite sprite = spriteRenderer.sprite;
        Texture2D tex = sprite.texture;
        
        // Важно: для GetPixel текстура должна быть Readable в настройках импорта
        Vector2 localPos = objectTransform.InverseTransformPoint(worldPos);
        Vector2 extents = sprite.bounds.extents;
        if (extents.x == 0 || extents.y == 0) return false;

        Vector2 uv = new Vector2(
            (localPos.x + extents.x) / (2f * extents.x),
            (localPos.y + extents.y) / (2f * extents.y)
        );

        Rect rect = sprite.textureRect;
        int x = Mathf.FloorToInt(uv.x * rect.width) + (int)rect.x;
        int y = Mathf.FloorToInt(uv.y * rect.height) + (int)rect.y;

        if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) return false;

        return tex.GetPixel(x, y).a > 0.1f;
    }
}