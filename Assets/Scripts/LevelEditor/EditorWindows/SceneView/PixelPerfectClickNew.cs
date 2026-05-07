using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ECS.Components;
using TimeLine.LevelEditor.InspectorTab.Components.EdgeCollider;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TransformationSquare;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class PixelPerfectClickNew : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform selectBoxScene;
    public Camera mapCamera;
    public RawImage mapImage;

    private TrackObjectStorage _trackObjectStorage;
    private SelectObjectController _selectObjectController;
    private C_EditColliderState _cEditColliderState;
    private EntityManager _entityManager;
    private EntityComponentController _entityComponentController;
    private TransformationSquareController _transformationSquareController;


    // Поля для логики циклического выделения
    private readonly List<Entity> _hitsAtLastPosition = new();
    private int _lastSelectedIndex = -1;

    [Inject]
    void Construct(TrackObjectStorage trackObjectStorage, SelectObjectController selectObjectController,
        C_EditColliderState cEditColliderState,
        EntityComponentController entityComponentController, TransformationSquareController transformationSquareController)
    {
        _trackObjectStorage = trackObjectStorage;
        _selectObjectController = selectObjectController;
        _cEditColliderState = cEditColliderState;
        _entityComponentController = entityComponentController;
        _transformationSquareController = transformationSquareController;
    }

    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectBoxScene.gameObject.activeSelf || _cEditColliderState.GetState()) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if(_transformationSquareController.isEditing) return;

        if (IsOverlaidByOtherUI(eventData)) return;

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

        Check(worldPos);
    }

    void Check(float3 mouseWorldPos)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        mouseWorldPos.z = 0;

        List<Entity> selectedEntity = new List<Entity>();

        var query = em.CreateEntityQuery(typeof(LocalToWorld));
        var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
        var transforms = query.ToComponentDataArray<LocalToWorld>(Unity.Collections.Allocator.Temp);

        for (int i = 0; i < entities.Length; i++)
        {
            float4x4 ltw = transforms[i].Value;
            float4x4 worldToLocal = math.inverse(ltw);
            float3 localMousePos = math.transform(worldToLocal, mouseWorldPos);

            float2 halfSize = new float2(0.5f, 0.5f);

            if (localMousePos.x >= -halfSize.x && localMousePos.x <= halfSize.x &&
                localMousePos.y >= -halfSize.y && localMousePos.y <= halfSize.y)
            {
                selectedEntity.Add(entities[i]);
            }
        }

        entities.Dispose();
        transforms.Dispose();
        
        if (_hitsAtLastPosition.Count > 0)
        {
            // Используем selectedEntity вместо entitiesInBoundingBox
            var currentParentsInBox = selectedEntity.Select(e => GetAllParents(em, e)).Distinct().ToList();

            // Удаляем из истории те сущности, которых нет под мышкой в данный момент
            int removedCount = _hitsAtLastPosition.RemoveAll(hit => !currentParentsInBox.Contains(hit));

            if (removedCount > 0)
                Debug.Log($"[Selection] Из истории перебора удалено {removedCount} сущностей (вне зоны клика).");
        }

        SortList(selectedEntity);

        if (selectedEntity.Count <= 0)
        {
            Debug.Log("[Selection] Не найдено сущностей в границах клика (Bounding Box).");
            _selectObjectController.DeselectAll();
            _hitsAtLastPosition.Clear();
            return;
        }

        bool isOneEntitySelected = false;
        Debug.Log($"[Selection] Найдено потенциальных сущностей: {selectedEntity.Count}");

        foreach (var entity in selectedEntity)
        {
            Entity parent = GetAllParents(em, entity);
            string entityName = em.GetName(entity);

            // 1. Проверка на повторное выделение
            if (_hitsAtLastPosition.Contains(parent))
            {
                Debug.Log($"[Selection] Пропуск {entityName}: Родитель уже находится в списке последних хитов (циклическое выделение).");
                continue;
            }

            // 2. Проверка EntityActiveTag
            if (!em.HasComponent(parent, typeof(EntityActiveTag)))
            {
                Debug.Log($"[Selection] Пропуск {entityName}: У родителя отсутствует компонент EntityActiveTag.");
                continue;
            }

            if (em.GetComponentData<EntityActiveTag>(parent).IsActive == false)
            {
                Debug.Log($"[Selection] Пропуск {entityName}: EntityActiveTag.IsActive == false.");
                continue;
            }

            // 3. Проверка SpriteRenderer (через ваш контроллер)
            if (!_entityComponentController.CheckComponentAvailability(entity, ComponentNames.SpriteRenderer))
            {
                Debug.Log($"[Selection] Пропуск {entityName}: Не пройдена проверка CheckComponentAvailability для SpriteRenderer.");
                continue;
            }

            // 4. Проверка MaterialMeshInfo
            if (!_entityManager.HasComponent<MaterialMeshInfo>(entity))
            {
                Debug.Log($"[Selection] Пропуск {entityName}: Отсутствует компонент MaterialMeshInfo.");
                continue;
            }

            // 5. Получение материала и текстуры
            RenderMeshArray rma = _entityManager.GetSharedComponentManaged<RenderMeshArray>(entity);
            var meshInfo = _entityManager.GetComponentData<MaterialMeshInfo>(entity);
            Material currentMat = rma.GetMaterial(meshInfo);

            if (currentMat == null || currentMat.mainTexture == null)
            {
                Debug.Log($"[Selection] Пропуск {entityName}: Материал или текстура не найдены.");
                continue;
            }

            // 6. Проверка прозрачности пикселя
            if (!IsPixelOpaque(entity, mouseWorldPos, (Texture2D)currentMat.mainTexture))
            {
                Debug.Log($"[Selection] Пропуск {entityName}: Клик попал в прозрачную область текстуры.");
                continue;
            }

            // Если дошли сюда — объект успешно выделен
            Debug.Log($"[Selection] УСПЕХ: Объект {entityName} (Parent: {parent.Index}) выделен.");
            _selectObjectController.SelectMultiple(_trackObjectStorage.GetTrackObjectData(parent));
            _hitsAtLastPosition.Add(parent);
            isOneEntitySelected = true;
            break;
        }

        if (!isOneEntitySelected)
        {
            Debug.Log("[Selection] Ни одна сущность не прошла фильтры. Сброс выделения.");
            _selectObjectController.DeselectAll();
            _hitsAtLastPosition.Clear();
        }
    }

    public Entity GetAllParents(EntityManager entityManager, Entity entity)
    {
        Entity parent = entity;

        // Проверяем, есть ли у сущности компонент Parent
        while (entityManager.HasComponent<Parent>(entity))
        {
            // Получаем родителя
            entity = entityManager.GetComponentData<Parent>(entity).Value;

            parent = entity;
        }


        return parent;
    }

    public void SortList(List<Entity> entities)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Сортируем по возрастанию Z
        entities.Sort((a, b) =>
        {
            float zA = entityManager.GetComponentData<LocalToWorld>(a).Position.z;
            float zB = entityManager.GetComponentData<LocalToWorld>(b).Position.z;
            return zA.CompareTo(zB);
        });
    }

    public bool IsPixelOpaque(Entity entity, float3 worldPos, Texture2D tex)
    {
        // 1. Получаем трансформацию
        var ltw = _entityManager.GetComponentData<LocalToWorld>(entity).Value;
        float4x4 worldToLocal = math.inverse(ltw);
        float3 localPos = math.transform(worldToLocal, worldPos);

        // 2. Превращаем localPos (-0.5 .. 0.5) в UV (0 .. 1)
        // Предполагаем, что объект в локальном пространстве имеет размер 1x1
        float2 uv = new float2(localPos.x + 0.5f, localPos.y + 0.5f);

        // Если клик вне границ спрайта (например, в прозрачном углу прямоугольника)
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return false;

        // 3. Вычисляем координаты пикселя
        int pixelX = (int)(uv.x * tex.width);
        int pixelY = (int)(uv.y * tex.height);

        // 4. Получаем альфа-канал
        Color color = tex.GetPixel(pixelX, pixelY);

        return color.a > 0.1f; // Порог непрозрачности
    }

    private bool IsOverlaidByOtherUI(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // ВЫВОДИМ СПИСОК ВСЕХ, КТО ПОД МЫШКОЙ
        // foreach (var res in results)
        // {
        //     // Debug.Log($"Под курсором: {res.gameObject.name} (Layer: {res.gameObject.layer})");
        // }

        if (results.Count > 0)
        {
            // Ищем индекс нашего mapImage в списке попаданий
            int myIndex = results.FindIndex(r => r.gameObject == mapImage.gameObject);

            // Если индекс 0 — мы сверху. 
            // Если индекс > 0 — значит перед нами есть кто-то еще (results[0])
            if (myIndex > 0)
            {
                Debug.LogWarning($"Клик заблокирован объектом: {results[0].gameObject.name}");
                return true;
            }

            // Если вообще не нашли mapImage в списке (индекс -1)
            if (myIndex == -1) return true;
        }

        return false;
    }
}