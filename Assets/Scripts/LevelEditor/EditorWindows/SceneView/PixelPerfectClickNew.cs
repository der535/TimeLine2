using System.Collections.Generic;
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
    private GameEventBus _gameEventBus;
    private EntityManager _entityManager;
    private EntityComponentController _entityComponentController;
    private TransformationSquareController _transformationSquareController;


    // Поля для логики циклического выделения
    private List<Entity> _hitsAtLastPosition = new List<Entity>();
    private int _lastSelectedIndex = -1;

    [Inject]
    void Construct(TrackObjectStorage trackObjectStorage, SelectObjectController selectObjectController,
        GameEventBus gameEventBus, C_EditColliderState cEditColliderState,
        EntityComponentController entityComponentController,
        TransformationSquareController transformationSquareController)
    {
        _trackObjectStorage = trackObjectStorage;
        _selectObjectController = selectObjectController;
        _gameEventBus = gameEventBus;
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
        if (_transformationSquareController.activeToll)
        {
            if(_transformationSquareController.CheckIsEditing()) return;
        }
        if (selectBoxScene.gameObject.activeSelf || _cEditColliderState.GetState()) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

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
        // ProcessClickSelection(worldPos);
    }

    void Check(float3 mouseWorldPos)
    {
        // 2. Получаем менеджер сущностей
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // 3. Переводим координаты мыши в мировые
        mouseWorldPos.z = 0;

        List<Entity> selectedEntity = new List<Entity>();

        // 4. Получаем доступ к запросу всех объектов с LocalToWorld
        // В MonoBehaviour мы используем EntityManager.GetAllEntities или EntityQuery
        var query = em.CreateEntityQuery(typeof(LocalToWorld));
        var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
        var transforms = query.ToComponentDataArray<LocalToWorld>(Unity.Collections.Allocator.Temp);

        for (int i = 0; i < entities.Length; i++)
        {
            float4x4 ltw = transforms[i].Value;

            // Получаем инвертированную матрицу для локального пространства объекта
            float4x4 worldToLocal = math.inverse(ltw);
            float3 localMousePos = math.transform(worldToLocal, mouseWorldPos);

            // Здесь 0.5f - это дефолтный размер. Если у тебя есть компонент с размером, 
            // получи его через em.GetComponentData<MySizeComponent>(entities[i])
            float2 halfSize = new float2(0.5f, 0.5f);

            if (localMousePos.x >= -halfSize.x && localMousePos.x <= halfSize.x &&
                localMousePos.y >= -halfSize.y && localMousePos.y <= halfSize.y)
            {
                selectedEntity.Add(entities[i]);
            }
        }

        // Очистка памяти
        entities.Dispose();
        transforms.Dispose();

        SortList(selectedEntity);

        if (selectedEntity.Count <= 0)
        {
            _selectObjectController.DeselectAll(); //Снимает все выделения
            _hitsAtLastPosition.Clear();
            return;
        }

        bool isOneEntitySelected = false;

        Debug.Log(selectedEntity.Count);

        foreach (var entity in selectedEntity)
        {
            Entity parent = GetAllParents(em, entity);
            
            if (_hitsAtLastPosition.Contains(parent)) continue; // Проверяем начилие MaterialMeshInfo
            if (!em.HasComponent(parent, typeof(EntityActiveTag))) continue; // Проверяем начилие EntityActiveTag
            if (em.GetComponentData<EntityActiveTag>(parent).IsActive == false) continue; // Проверяем активность существа
            if (!_entityComponentController.CheckComponentAvailability(entity, ComponentNames.SpriteRenderer)) continue; // Проверяем начилие SpriteRenderer
            if (!_entityManager.HasComponent<MaterialMeshInfo>(entity)) continue; // Проверяем начилие MaterialMeshInfo

            Material currentMat = null;
            RenderMeshArray rma = _entityManager.GetSharedComponentManaged<RenderMeshArray>(entity);
            var meshInfo = _entityManager.GetComponentData<MaterialMeshInfo>(entity);
            currentMat = rma.GetMaterial(meshInfo);

            if (!IsPixelOpaque(entity, mouseWorldPos, (Texture2D)currentMat.mainTexture)) continue; // Проверяем попадаем ли мы в непрозрачный пиксель

            Debug.Log(_trackObjectStorage.GetTrackObjectData(entity).branch.Name);

            

            _selectObjectController.SelectMultiple(_trackObjectStorage.GetTrackObjectData(parent));
            _hitsAtLastPosition.Add(parent);
            isOneEntitySelected = true;
            break;
        }

        if (isOneEntitySelected == false)
        {
            _selectObjectController.DeselectAll(); //Снимает все выделения
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
        foreach (var res in results)
        {
            // Debug.Log($"Под курсором: {res.gameObject.name} (Layer: {res.gameObject.layer})");
        }

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