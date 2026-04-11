using System;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ECS.Services;
using TimeLine.LevelEditor.InspectorTab.Components.EdgeCollider;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.InspectorTab.Components.PolygonCollider
{
    public class PolygonColliderEditorNew : MonoBehaviour
    {
        [SerializeField] private GameObject selectCube;
        [SerializeField] private Camera _camera; // Переименовано во избежание конфликта (hiding base member)

        [Space] [SerializeField] private float startScale;
        [SerializeField] private float cornerScale;
        [SerializeField] private float distanceToEdge;

        private SceneToRawImageConverter _sceneToRawImageConverter;
        private GameEventBus _eventBus;
        private EntityManager _entityManager;
        private C_EditColliderState _editColliderState;

        private float2[] _points;
        private Entity _selectedEntity;
        private int _selectedPointIndex = -1;

        [Inject]
        private void Construct(GameEventBus eventBus, SceneToRawImageConverter sceneToRawImageConverter,
            C_EditColliderState editColliderState)
        {
            _eventBus = eventBus;
            _sceneToRawImageConverter = sceneToRawImageConverter;
            _editColliderState = editColliderState;
        }

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            _eventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                var entity = data.Tracks[^1].entity;

                if (_entityManager.HasComponent<PolygonColliderData>(entity))
                {
                    _points = _entityManager.GetComponentData<PolygonColliderData>(entity)
                        .PointsReference.Value.Points.ToArray();
                    _selectedEntity = entity;
                }
                else
                {
                    _points = null;
                }
            });
        }

        // Изменено: теперь принимает обычный массив. NativeArray больше не "утекает" в Update.
        public BlobAssetReference<PolygonPointsBlob> CreateBlobFromPoints(float2[] points)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<PolygonPointsBlob>();

            var arrayBuilder = builder.Allocate(ref root.Points, points.Length);
            for (int i = 0; i < points.Length; i++)
            {
                arrayBuilder[i] = points[i];
            }

            var blobReference = builder.CreateBlobAssetReference<PolygonPointsBlob>(Allocator.Persistent);
            builder.Dispose();

            return blobReference;
        }

        private void Update()
        {
            if (_points == null || _points.Length == 0 || _editColliderState.GetState() == false) return;

            var entityPosition = _entityManager.GetComponentData<LocalToWorld>(_selectedEntity).Position;
            var entityScale =
                GetScaleFromMatrix.Get(_entityManager.GetComponentData<LocalToWorld>(_selectedEntity).Value);
            var entityRotation = _entityManager.GetComponentData<LocalToWorld>(_selectedEntity).Rotation;

            var mousePosition = _sceneToRawImageConverter.ScreenPointToWorldScene(UnityEngine.Input.mousePosition);
            mousePosition.z = 0; // На всякий случай фиксируем Z в 0

            // Сброс индекса при отпускании кнопки
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                _selectedPointIndex = -1;
            }

            // Логика перемещения (если точка выбрана)
            if (_selectedPointIndex >= 0)
            {
                _points[_selectedPointIndex] =
                    new float2(mousePosition.x - entityPosition.x, mousePosition.y - entityPosition.y);

                var zeroRotate = math.rotate(math.inverse(entityRotation),
                    new Vector3(_points[_selectedPointIndex].x, _points[_selectedPointIndex].y, 0));


                _points[_selectedPointIndex] = new float2(zeroRotate.x, zeroRotate.y);
                _points[_selectedPointIndex] = new float2(_points[_selectedPointIndex].x / entityScale.x,
                    _points[_selectedPointIndex].y / entityScale.y);

                _entityManager.SetComponentData(_selectedEntity, new PolygonColliderData
                {
                    PointsReference = CreateBlobFromPoints(_points)
                });

                // Визуальное обновление кубика при перетаскивании
                selectCube.transform.position = mousePosition;
                selectCube.transform.localScale = new Vector3(_camera.orthographicSize * startScale,
                    _camera.orthographicSize * startScale, 1);

                return; // Прерываем Update, так как вычислять ближайшие грани сейчас не нужно
            }

            // --- Логика наведения (поиск ближайшей точки/грани) ---

            float minDistance = float.MaxValue;
            Vector3 closedPosition = Vector3.zero;
            int localSelectedPointIndex = -1;
            int firstPointOfTheSegment = -1;

            // Проверка всех соединений (включая замыкание: последняя точка с первой)
            for (int i = 0; i < _points.Length; i++)
            {
                int nextIndex = (i + 1) % _points.Length; // Закольцовываем индекс для последнего отрезка

                Vector3 p1 = new Vector3(_points[i].x, _points[i].y, 0);
                Vector3 p2 = new Vector3(_points[nextIndex].x, _points[nextIndex].y, 0);

                p1 = math.rotate(entityRotation, Vector3.Scale(p1, entityScale));
                p2 = math.rotate(entityRotation, Vector3.Scale(p2, entityScale));

                p1 += (Vector3)entityPosition;
                p2 += (Vector3)entityPosition;

                var positionOnLine = GetClosestPointOnSegment(p1, p2, mousePosition);
                float distToLine = Vector3.Distance(mousePosition, positionOnLine);

                if (distToLine < distanceToEdge * _camera.orthographicSize)
                {
                    minDistance = distToLine;
                    closedPosition = positionOnLine;
                    firstPointOfTheSegment = i;

                    bool closeToP1 = Vector3.Distance(closedPosition, p1) < distanceToEdge * _camera.orthographicSize;
                    bool closeToP2 = Vector3.Distance(closedPosition, p2) < distanceToEdge * _camera.orthographicSize;


                    if (closeToP1)
                    {
                        localSelectedPointIndex = i;
                        firstPointOfTheSegment = -1;
                    }
                    else if (closeToP2)
                    {
                        localSelectedPointIndex = nextIndex;
                        firstPointOfTheSegment = -1;
                    }
                }
            }

            // Обновление позиции кубика выделения
            if (localSelectedPointIndex >= 0)
            {
                selectCube.transform.position = math.rotate(entityRotation, Vector3.Scale(
                    new Vector3(_points[localSelectedPointIndex].x,
                        _points[localSelectedPointIndex].y, 0), entityScale));
                selectCube.transform.position += (Vector3)entityPosition;
            }
            else
            {
                selectCube.transform.position = closedPosition;
            }

            // Обновление масштаба кубика
            if (minDistance < distanceToEdge)
            {
                float targetScale = localSelectedPointIndex != -1 ? cornerScale : startScale;
                targetScale *= _camera.orthographicSize;
                selectCube.transform.localScale = new Vector3(targetScale, targetScale, 1);
            }
            else
            {
                selectCube.transform.localScale = Vector3.zero;
            }

            if (UnityEngine.Input.GetMouseButtonDown(1) && localSelectedPointIndex != -1 && _points.Length > 3)
            {
                _points = RemoveAt(_points, localSelectedPointIndex);
                _entityManager.SetComponentData(_selectedEntity, new PolygonColliderData
                {
                    PointsReference = CreateBlobFromPoints(_points)
                });
            }

            // Захват точки по клику
            if (UnityEngine.Input.GetMouseButtonDown(0) && localSelectedPointIndex != -1)
            {
                _selectedPointIndex = localSelectedPointIndex;
            }
            else if (UnityEngine.Input.GetMouseButtonDown(0) && firstPointOfTheSegment != -1)
            {
                var result = InsertPoint(_points, firstPointOfTheSegment, new float2(mousePosition.x, mousePosition.y));
                _points = result;

                _entityManager.SetComponentData(_selectedEntity, new PolygonColliderData
                {
                    PointsReference = CreateBlobFromPoints(_points)
                });
                _selectedPointIndex = firstPointOfTheSegment + 1;
            }
        }

        public float2[] InsertPoint(float2[] source, int indexAfter, float2 newPoint)
        {
            float2[] result = new float2[source.Length + 1];

            // Копируем первую часть
            Array.Copy(source, 0, result, 0, indexAfter + 1);

            // Ставим новую точку
            result[indexAfter + 1] = newPoint;

            // Копируем остаток
            Array.Copy(source, indexAfter + 1, result, indexAfter + 2, source.Length - (indexAfter + 1));
            return result;
        }

        public float2[] RemoveAt(float2[] source, int index)
        {
            // Проверка на корректность индекса
            if (index < 0 || index >= source.Length)
            {
                Debug.LogError("Индекс вне диапазона массива!");
                return source;
            }

            // Если в массиве был всего один элемент, возвращаем пустой массив
            if (source.Length == 1) return new float2[0];

            float2[] result = new float2[source.Length - 1];

            // Копируем всё ДО индекса
            if (index > 0)
            {
                Array.Copy(source, 0, result, 0, index);
            }

            // Копируем всё ПОСЛЕ индекса
            if (index < source.Length - 1)
            {
                Array.Copy(source, index + 1, result, index, source.Length - index - 1);
            }

            return result;
        }

        /// <summary>
        /// Возвращает координаты на отрезке между двумя точками наиближайшее к целевой точке.
        /// </summary>
        public static Vector3 GetClosestPointOnSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 segment = end - start;
            Vector3 toPoint = point - start;
            float lengthSquared = segment.sqrMagnitude;

            if (lengthSquared == 0f) return start;

            float t = Mathf.Clamp01(Vector3.Dot(toPoint, segment) / lengthSquared);
            return start + segment * t;
        }
    }
}