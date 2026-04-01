using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ECS.Services;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace TimeLine.LevelEditor.InspectorTab.Components.BoxCollider
{
    public enum ColliderType
    {
        BoxCollider,
        CircleCollider,
        PolygonCollider,
    }

    public class ColliderDrawer : MonoBehaviour
    {
        public GameObject linePrefab;

        private Dictionary<Entity, ColliderType> _dictionary = new();
        private List<LineRenderer> _lineRenderer = new ();

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                _dictionary.Remove(data.DeselectedObject.entity);
            });
            _gameEventBus.SubscribeTo((ref StartCompositionEdit data) =>
            {
                Clear();
            });
            _gameEventBus.SubscribeTo((ref EndCompositionEdit data) =>
            {
                Clear();
            });
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => { _dictionary.Clear(); });
        }

        public void AddCollider(ColliderType colliderType, Entity entity)
        {
            foreach (var variable in _dictionary)
            {
                if (variable.Key == entity)
                {
                    _dictionary.Remove(variable.Key);
                    break;
                }
            }

            _dictionary.Add(entity, colliderType);
        }

        private void Clear()
        {
            foreach (var VARIABLE in _lineRenderer)
            {
                Destroy(VARIABLE.gameObject);
            }

            _lineRenderer.Clear();
            _dictionary.Clear();
        }

        public void Update()
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (var VARIABLE in _lineRenderer)
            {
                Destroy(VARIABLE.gameObject);
            }

            _lineRenderer.Clear();

            foreach (var pair in _dictionary)
            {
                float4x4 ltw = em.GetComponentData<LocalToWorld>(pair.Key).Value;
                float3 scale = GetScaleFromMatrix.Get(ltw);

                if(scale.x <= 0 || scale.y <= 0) break;
                
                LineRenderer lineRenderer = Instantiate(linePrefab).GetComponent<LineRenderer>();
                _lineRenderer.Add(lineRenderer);

                Entity entity = pair.Key;
                if (!em.Exists(entity)) continue;
                if (!em.HasComponent<LocalToWorld>(entity)) continue;

                // 1. Берем сырую матрицу
                


                // 2. ОЧИЩАЕМ МАТРИЦУ ОТ СКЕЙЛА (Вставлять СЮДА)
                float3 position = em.GetComponentData<LocalToWorld>(entity).Position;
                // Берем базисные векторы (c0, c1, c2), нормализуем их, чтобы длина стала 1
                float3x3 rotationMatrix = new float3x3(
                    math.normalize(ltw.c0.xyz),
                    math.normalize(ltw.c1.xyz),
                    math.normalize(ltw.c2.xyz)
                );
                quaternion rotation = math.quaternion(rotationMatrix);

                // Создаем матрицу, у которой Scale всегда 1,1,1
                float4x4 drawMatrix = float4x4.TRS(position, rotation, new float3(1f));

                if (em.HasComponent<PhysicsCollider>(entity))
                {
                    var physicsCollider = em.GetComponentData<PhysicsCollider>(entity);

                    switch (pair.Value)
                    {
                        case ColliderType.BoxCollider:
                            if (physicsCollider.TryGetBox(out BoxGeometry boxGeometry))
                            {
                                float3 halfSize = boxGeometry.Size * 0.5f;
                                float3[] vertices =
                                {
                                    new(-halfSize.x, -halfSize.y, 0),
                                    new(halfSize.x, -halfSize.y, 0),
                                    new(halfSize.x, halfSize.y, 0),
                                    new(-halfSize.x, halfSize.y, 0)
                                };

                                lineRenderer.positionCount = 4;
                                lineRenderer.loop = true;

                                for (int i = 0; i < 4; i++)
                                {
                                    // Учитываем внутренние параметры геометрии
                                    float3 localWithGeometry = math.rotate(boxGeometry.Orientation, vertices[i]) +
                                                               boxGeometry.Center;

                                    // 3. ИСПОЛЬЗУЕМ drawMatrix ВМЕСТО ltw
                                    float3 worldPoint = math.transform(drawMatrix, localWithGeometry);
                                    lineRenderer.SetPosition(i, (Vector3)worldPoint);
                                }
                            }

                            break;
                        case ColliderType.CircleCollider:
                            if (physicsCollider.TryGetSphere(out SphereGeometry sphereGeometry))
                            {
                                float radius = sphereGeometry.Radius;
                                float3 center = sphereGeometry.Center;

                                int segments = 32; // Количество сегментов для гладкости круга
                                lineRenderer.positionCount = segments;
                                lineRenderer.loop = true;

                                quaternion worldRotation = math.quaternion(ltw); // Извлекаем поворот из матрицы
                                float3 worldPosition = position;


                                for (int i = 0; i < segments; i++)
                                {
                                    // Вычисляем угол для текущей точки
                                    float angle = math.radians(i * 360f / segments);

                                    // Точка в локальных координатах коллайдера (плоскость XY)
                                    float3 localPoint = new float3(
                                        center.x + math.cos(angle) * radius,
                                        center.y + math.sin(angle) * radius,
                                        0
                                    );

                                    // Вращаем точку относительно локального центра (0,0,0)
                                    float3 rotatedPoint = math.rotate(worldRotation, localPoint);

                                    // Прибавляем мировую позицию
                                    float3 finalWorldPoint = worldPosition + rotatedPoint;

                                    lineRenderer.SetPosition(i, (Vector3)finalWorldPoint);
                                }
                            }

                            break;
                        case ColliderType.PolygonCollider:
                            if (em.HasComponent<PolygonColliderData>(entity))
                            {
                                var polygonColliderTag = em.GetComponentData<PolygonColliderData>(entity).PointsReference
                                    .Value.Points.ToArray();
                                lineRenderer.positionCount = polygonColliderTag.Length;
                                lineRenderer.loop = true;
                                for (var index = 0; index < polygonColliderTag.Length; index++)
                                {
                                    var point = polygonColliderTag[index];

                                    lineRenderer.SetPosition(index,
                                        math.transform(ltw, new Vector3(point.x, point.y, 0)));
                                }
                            }

                            break;
                    }
                }
            }
        }
    }
}