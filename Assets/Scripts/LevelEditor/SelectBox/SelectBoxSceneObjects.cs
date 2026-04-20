using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.SelectBox
{
    public class SelectBoxSceneObjects : MonoBehaviour
    {
        [SerializeField] private RectTransform selectBox; // Объект который будет показывать область выделения
        [SerializeField] private RectTransform timeLineArea; //Объект внутри которого будет работать выделение
        [SerializeField] private Camera timeLineCamera;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private float deadZoneDistance = 5f; // Пиксели или единицы канваса

        private GameEventBus _gameEventBus;
        private DeselectObject _deselectObject;
        private SelectObjectController _selectObjectController;
        private SceneToRawImageConverter _sceneToRawImageConverter;
        private TrackObjectStorage _trackObjectStorage;

        private M_SelectBoxState _state = new();
        private M_SelectBoxDelta _delta = new();

        private HashSet<TrackObjectPacket> _ignoredPacketsDuringCurrentDrag = new HashSet<TrackObjectPacket>();

        [Inject]
        private void Constructor(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage,
            DeselectObject deselectObject, SelectObjectController selectObjectController,
            SceneToRawImageConverter sceneToRawImageConverter)
        {
            _trackObjectStorage = trackObjectStorage;
            _gameEventBus = gameEventBus;
            _deselectObject = deselectObject;
            _selectObjectController = selectObjectController;
            _sceneToRawImageConverter = sceneToRawImageConverter;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectObjectEvent selectBox) => { _state.IsActive = false; });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent deselectBox) => { _state.IsActive = true; });
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent all) => { _state.IsActive = true; });
        }

        private void Update()
        {
            if (_state.IsActive == false && _state.IsDragging == false)
            {
                return;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                StartMove();
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                EndMove();
            }


            if (_state.IsDragging && _state.CursorIsInside)
            {
                Vector2 currentMousePos =
                    TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position;

                // Проверка мертвой зоны, если она еще не пройдена
                if (!_state.HasExceededDeadZone)
                {
                    if (Vector2.Distance(_state.StartPosition, currentMousePos) > deadZoneDistance)
                    {
                        _state.HasExceededDeadZone = true;
                        selectBox.gameObject.SetActive(true); // Включаем рамку только здесь
                        _deselectObject.Deselect();
                    }
                    else
                    {
                        return; // Выходим из Update, пока мы внутри мертвой зоны
                    }
                }

                SelectBoxCalculate();
                // Оптимизация: выборку объектов лучше делать только если рамка активна
                UpdateSelection();
            }
        }

        private Vector2 startPositionOnScene;

        private void StartMove()
        {
            _state.CursorIsInside = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).isInside;
            if (!_state.CursorIsInside) return; // Если клик вне рабочей зоны, ничего не делаем

            _state.IsDragging = true;
            _state.HasExceededDeadZone = false;
            _state.StartPosition = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position;
    
            // Позиция клика в мировых координатах сцены
            Vector2 worldClickPos = _sceneToRawImageConverter.ScreenPointToWorldScene(UnityEngine.Input.mousePosition);
            startPositionOnScene = worldClickPos;

            // Очищаем старые игнорируемые объекты
            _ignoredPacketsDuringCurrentDrag.Clear();

            // Находим все объекты, в которые мы "попали" при клике
            var allActiveObjects = _trackObjectStorage.GetAllActiveSceneObjects();
            FillIgnoreList(allActiveObjects, worldClickPos);
        }
        // Рекурсивно ищем, по кому мы кликнули в самом начале
        private void FillIgnoreList(List<TrackObjectPacket> list, Vector3 worldPos)
        {
            foreach (var packet in list)
            {
                if (packet is TrackObjectGroup group)
                {
                    // Если мы кликнули хотя бы в одного ребенка группы, 
                    // вся группа должна быть проигнорирована
                    bool clickedInsideGroup = false;
                    foreach (var child in group.TrackObjectDatas)
                    {
                        if (CheckIsPointInside(child.entity, worldPos))
                        {
                            clickedInsideGroup = true;
                            break;
                        }
                    }
            
                    if (clickedInsideGroup)
                        _ignoredPacketsDuringCurrentDrag.Add(group);
                    else 
                        FillIgnoreList(group.TrackObjectDatas, worldPos); // Идем глубже, если группа большая
                }
                else
                {
                    if (CheckIsPointInside(packet.entity, worldPos))
                    {
                        _ignoredPacketsDuringCurrentDrag.Add(packet);
                    }
                }
            }
        }
        
        private bool CheckIsPointInside(Entity entity, float3 worldPos)
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!em.HasComponent<LocalToWorld>(entity)) return false;

            var ltw = em.GetComponentData<LocalToWorld>(entity).Value;
            float4x4 worldToLocal = math.inverse(ltw);
            float3 localPos = math.transform(worldToLocal, worldPos);

            // Проверка границ (0.5f - стандартный размер)
            if (localPos.x < -0.5f || localPos.x > 0.5f || localPos.y < -0.5f || localPos.y > 0.5f)
                return false;

            // Проверка альфа-канала (опционально, но лучше оставить для точности)
            if (em.HasComponent<MaterialMeshInfo>(entity))
            {
                RenderMeshArray rma = em.GetSharedComponentManaged<RenderMeshArray>(entity);
                var meshInfo = em.GetComponentData<MaterialMeshInfo>(entity);
                Material mat = rma.GetMaterial(meshInfo);
                Texture2D tex = mat.mainTexture as Texture2D;

                if (tex != null)
                {
                    float u = localPos.x + 0.5f;
                    float v = localPos.y + 0.5f;
                    return tex.GetPixel((int)(u * tex.width), (int)(v * tex.height)).a > 0.1f;
                }
            }

            return true;
        }

        private void EndMove()
        {
            selectBox.gameObject.SetActive(false);
            _state.IsDragging = false;
            _state.HasExceededDeadZone = false;
    
            // Очищаем список после завершения выделения
            _ignoredPacketsDuringCurrentDrag.Clear();
            _selectObjectController.UpdateSelection();
        }

        private void SelectBoxCalculate()
        {
            Vector2 mouseStartPositionModified =
                _sceneToRawImageConverter.WorldToUIAnchoredPosition(startPositionOnScene, timeLineArea);

            Vector2 delta = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position -
                            mouseStartPositionModified;
            selectBox.sizeDelta = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            selectBox.anchoredPosition = (mouseStartPositionModified +
                                          TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera)
                                              .position) / 2;
        }
        
        private void UpdateSelection()
        {
            // Получаем все корневые объекты сцены (верхний уровень)
            var allObjects = _trackObjectStorage.GetAllActiveSceneObjects();

            foreach (var topLevelObject in allObjects)
            {
                // Спрашиваем: пересекается ли этот объект ИЛИ любой из его потомков с рамкой?
                bool isHit = IsPacketIntersecting(topLevelObject);

                if (isHit)
                {
                    // Если пересекается, и еще не выделен — выделяем
                    if (!_selectObjectController.SelectObjects.Contains(topLevelObject))
                    {
                        _selectObjectController.DeselectVihoutEvent(topLevelObject);
                    }
                }
                else
                {
                    // Если не пересекается НИ ОДИН элемент группы, и группа была выделена — снимаем выделение
                    if (_selectObjectController.SelectObjects.Contains(topLevelObject))
                    {
                        _selectObjectController.Deselect(topLevelObject);
                    }
                }
            }
        }

// Рекурсивный метод: возвращает true, если сам объект или хотя бы один его "ребенок" попал в рамку
        private bool IsPacketIntersecting(TrackObjectPacket packet)
        {
            // ГЛАВНОЕ ИЗМЕНЕНИЕ: Если этот пакет в списке игнора, мы его "не видим"
            if (_ignoredPacketsDuringCurrentDrag.Contains(packet))
            {
                return false;
            }

            if (packet is TrackObjectGroup group)
            {
                foreach (var child in group.TrackObjectDatas)
                {
                    if (IsPacketIntersecting(child)) return true;
                }
                return false;
            }
            else
            {
                return CheckIsSelected(packet.entity, selectBox);
            }
        }

        private bool CheckIsSelected(Entity entity, RectTransform selectionBox)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Проверяем наличие матрицы трансформации (позиция, поворот, масштаб)
            if (!entityManager.HasComponent<LocalToWorld>(entity)) return false;

            var ltw = entityManager.GetComponentData<LocalToWorld>(entity).Value;

            // 1. Получаем 4 угла Entity в его локальных координатах (как в 1-м скрипте, считаем размер 0.5f)
            float2 halfSize = new float2(0.5f, 0.5f);
            float3[] localCorners =
            {
                new float3(-halfSize.x, -halfSize.y, 0),
                new float3(halfSize.x, -halfSize.y, 0),
                new float3(halfSize.x, halfSize.y, 0),
                new float3(-halfSize.x, halfSize.y, 0)
            };

            // 2. Переводим углы Entity в UI координаты пространства timeLineArea
            Vector2[] entityUICorners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                float3 worldPos = math.transform(ltw, localCorners[i]);
                entityUICorners[i] = _sceneToRawImageConverter.WorldToUIAnchoredPosition(worldPos, timeLineArea);
            }

            // 3. Получаем Rect рамки выделения
            Rect selectionRect = selectionBox.rect;
            selectionRect.x += selectionBox.anchoredPosition.x;
            selectionRect.y += selectionBox.anchoredPosition.y;

            // 4. Проверяем точное пересечение (с учетом вращения) с помощью SAT
            if (!IsPolygonIntersectingRect(entityUICorners, selectionRect))
                return false;

            // 5. Если геометрия пересекается, проверяем альфа-канал
            return CheckAlphaInSelectionBox(entity, entityManager, selectionRect, ltw);
        }

        // --- Вспомогательный метод 1: Проверка пересечения с учетом вращения (SAT) ---
        private bool IsPolygonIntersectingRect(Vector2[] poly, Rect rect)
        {
            // Шаг 1: Быстрая проверка по AABB рамки (Оси X и Y)
            float polyMinX = float.MaxValue, polyMaxX = float.MinValue;
            float polyMinY = float.MaxValue, polyMaxY = float.MinValue;

            foreach (var p in poly)
            {
                if (p.x < polyMinX) polyMinX = p.x;
                if (p.x > polyMaxX) polyMaxX = p.x;
                if (p.y < polyMinY) polyMinY = p.y;
                if (p.y > polyMaxY) polyMaxY = p.y;
            }

            // Если даже прямые границы не пересекаются, значит объекты точно далеко друг от друга
            if (polyMaxX < rect.xMin || polyMinX > rect.xMax) return false;
            if (polyMaxY < rect.yMin || polyMinY > rect.yMax) return false;

            // Углы нашей UI рамки
            Vector2[] rectCorners =
            {
                new Vector2(rect.xMin, rect.yMin),
                new Vector2(rect.xMax, rect.yMin),
                new Vector2(rect.xMax, rect.yMax),
                new Vector2(rect.xMin, rect.yMax)
            };

            // Шаг 2: Проверка по локальным осям повернутого объекта
            Vector2[] axes =
            {
                poly[1] - poly[0], // Ось X объекта
                poly[2] - poly[1] // Ось Y объекта
            };

            foreach (var axis in axes)
            {
                // Перпендикуляр к оси
                Vector2 normal = new Vector2(-axis.y, axis.x);

                float minPoly = float.MaxValue, maxPoly = float.MinValue;
                foreach (var p in poly)
                {
                    float proj = Vector2.Dot(normal, p);
                    if (proj < minPoly) minPoly = proj;
                    if (proj > maxPoly) maxPoly = proj;
                }

                float minRect = float.MaxValue, maxRect = float.MinValue;
                foreach (var p in rectCorners)
                {
                    float proj = Vector2.Dot(normal, p);
                    if (proj < minRect) minRect = proj;
                    if (proj > maxRect) maxRect = proj;
                }

                // Если проекции не наслаиваются, значит есть "зазор" и объекты не пересекаются
                if (maxPoly < minRect || minPoly > maxRect) return false;
            }

            return true;
        }

// --- Вспомогательный метод 2: Оптимизированная проверка альфа-канала ---
        private bool CheckAlphaInSelectionBox(Entity entity, EntityManager em, Rect selectionRect, float4x4 ltw)
        {
            // Проверяем наличие нужных компонентов для текстуры
            if (!em.HasComponent<MaterialMeshInfo>(entity)) return true; // Если нет материала, считаем выделенным по геометрии

            RenderMeshArray rma = em.GetSharedComponentManaged<RenderMeshArray>(entity);
            var meshInfo = em.GetComponentData<MaterialMeshInfo>(entity);
            Material currentMat = rma.GetMaterial(meshInfo);

            Texture2D tex = currentMat.mainTexture as Texture2D;
            if (tex == null) return true;

            // Настройка качества сэмплирования (количество точек). 
            // 10 означает проверку 11x11 = 121 точки на спрайте. Этого достаточно для точного выделения.
            int steps = 10;

            for (int x = 0; x <= steps; x++)
            {
                for (int y = 0; y <= steps; y++)
                {
                    // UV координаты (от 0 до 1)
                    float u = x / (float)steps;
                    float v = y / (float)steps;

                    // Вычисляем позицию пикселя в текстуре
                    int pixelX = (int)(u * (tex.width - 1));
                    int pixelY = (int)(v * (tex.height - 1));

                    // Читаем альфу (ВАЖНО: текстура должна быть Read/Write Enabled в настройках импорта)
                    if (tex.GetPixel(pixelX, pixelY).a > 0.1f)
                    {
                        // Если пиксель непрозрачный, переводим его позицию в UI и смотрим, внутри ли он рамки
                        float localX = -0.5f + u; // от -0.5 до 0.5
                        float localY = -0.5f + v;

                        float3 worldPos = math.transform(ltw, new float3(localX, localY, 0));
                        Vector2 uiPos = _sceneToRawImageConverter.WorldToUIAnchoredPosition(worldPos, timeLineArea);

                        // Если непрозрачная точка попала в рамку — объект выделен!
                        if (selectionRect.Contains(uiPos))
                        {
                            return true;
                        }
                    }
                }
            }

            // Если рамка пересеклась только с прозрачной областью
            return false;
        }


// Вспомогательный метод для создания границ рамки в координатах Viewport
        private Bounds GetViewportBounds(RectTransform selectionBox, Camera cam)
        {
            Vector3[] corners = new Vector3[4];
            selectionBox.GetWorldCorners(corners);

            Bounds bounds = new Bounds();
            for (int i = 0; i < 4; i++)
            {
                // Переводим углы UI рамки в координаты Viewport камеры
                Vector3 viewportCorner = cam.ScreenToViewportPoint(corners[i]);
                if (i == 0) bounds.SetMinMax(viewportCorner, viewportCorner);
                else bounds.Encapsulate(viewportCorner);
            }

            // Обнуляем Z, так как нам важна только плоскость экрана
            Vector3 min = bounds.min;
            min.z = 0f;
            Vector3 max = bounds.max;
            max.z = 1f; // Даем небольшой запас по глубине
            bounds.SetMinMax(min, max);

            return bounds;
        }
    }
}