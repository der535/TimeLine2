using System;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.ECS.Components;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.CameraBoundaries
{
   public class PlayerBoundaries : MonoBehaviour
    {
        private CameraReferences _references;
        private BoxColliderInstaller _boxColliderInstaller;
        private SpriteRendererInstaller _spriteRendererInstaller;

        private const float borderWitdh = 1f;
        private const float fixedAspect = 16f / 9f; // Фиксируем аспект

        private Entity borderTop;
        private Entity borderRight;
        private Entity borderBottom; // Исправил опечатку Botton -> Bottom
        private Entity borderLeft;

        [Inject]
        private void Construct(CameraReferences cameraReferences, BoxColliderInstaller boxColliderInstaller, SpriteRendererInstaller spriteRendererInstaller)
        {
            _references = cameraReferences;
            _boxColliderInstaller = boxColliderInstaller;
            _spriteRendererInstaller = spriteRendererInstaller;
        }

        void Start()
        {
            // Создаем сущности один раз при старте
            // Позиции и масштабы инициализируем нулевыми, UpdateBounds все поправит
            borderTop = CreateSceneObject(new float3(1, 1, 1), new float3(0, 0, 0));
            borderBottom = CreateSceneObject(new float3(1, 1, 1), new float3(0, 0, 0));
            borderRight = CreateSceneObject(new float3(1, 1, 1), new float3(0, 0, 0));
            borderLeft = CreateSceneObject(new float3(1, 1, 1), new float3(0, 0, 0));
            
            UpdateBounds();
        }

        private void Update()
        {
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            if (_references?.playCamera == null) return;

            // Используем фиксированный аспект вместо _references.playCamera.aspect
            float height = _references.playCamera.orthographicSize;
            float width = height * fixedAspect; 
            
            Vector3 center = _references.playCamera.transform.position;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Обновляем каждую границу
            UpdateEntityTransform(entityManager, borderTop, 
                new float3(width * 2, borderWitdh, 10f), 
                new float3(center.x, center.y + height + borderWitdh / 2f, 0));

            UpdateEntityTransform(entityManager, borderBottom, 
                new float3(width * 2, borderWitdh, 10f), 
                new float3(center.x, center.y - height - borderWitdh / 2f, 0));

            UpdateEntityTransform(entityManager, borderRight, 
                new float3(borderWitdh, height * 2, 10f), 
                new float3(center.x + width + borderWitdh / 2f, center.y, 0));

            UpdateEntityTransform(entityManager, borderLeft, 
                new float3(borderWitdh, height * 2, 10f), 
                new float3(center.x - width - borderWitdh / 2f, center.y, 0));
        }

        // Вспомогательный метод для обновления позиции и масштаба сущности
        private void UpdateEntityTransform(EntityManager em, Entity entity, float3 scale, float3 position)
        {
            if (entity == Entity.Null || !em.Exists(entity)) return;

            em.SetComponentData(entity, new LocalTransform 
            { 
                Position = position, 
                Rotation = quaternion.identity, 
                Scale = 1f // Используем 1, так как реальный размер задаем через Matrix
            });

            em.SetComponentData(entity, new PostTransformMatrix
            {
                Value = float4x4.Scale(scale)
            });
        }

        internal Entity CreateSceneObject(float3 scale, float3 position)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = entityManager.CreateEntity();
    
            entityManager.AddComponent<EntityActiveTag>(entity);
            entityManager.AddComponent<LocalTransform>(entity);
            entityManager.AddComponent<PostTransformMatrix>(entity);
            entityManager.AddComponent<LocalToWorld>(entity);

            _boxColliderInstaller.Install(entity);
            
            UpdateEntityTransform(entityManager, entity, scale, position);

            return entity;
        }
    }
}