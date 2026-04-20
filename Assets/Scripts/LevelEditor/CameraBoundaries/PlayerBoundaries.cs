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

        private const float borderWitdh = 1;

        private Entity borderTop;
        private Entity borderRight;
        private Entity borderBotton;
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
            // Настройка LineRenderer для замыкания рамки
            UpdateBounds();
            
            float height = _references.playCamera.orthographicSize;
            float width = height * _references.playCamera.aspect;
            
            borderTop = CreateSceneObject(new float3(width*2, borderWitdh, 100), new float3(0, height + borderWitdh/2, 0));
            borderBotton = CreateSceneObject(new float3(width*2, borderWitdh, 100), new float3(0, -height - borderWitdh/2, 0));
            borderRight = CreateSceneObject(new float3(borderWitdh, height*2, 100), new float3(width + borderWitdh/2, 0, 0));
            borderLeft = CreateSceneObject(new float3(borderWitdh, height*2, 100), new float3(-width - borderWitdh/2, 0, 0));

            
        }

        internal Entity CreateSceneObject(float3 scale, float3 position)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = entityManager.CreateEntity();
    
            entityManager.AddComponent<EntityActiveTag>(entity);
            entityManager.AddComponent<LocalTransform>(entity);
            entityManager.AddComponent<PostTransformMatrix>(entity);
    
            // ДОБАВЬТЕ ЭТУ СТРОКУ:
            entityManager.AddComponent<LocalToWorld>(entity);

            var transform = LocalTransform.Identity;
            entityManager.SetComponentData(entity, transform);

            entityManager.SetComponentData(entity, new PostTransformMatrix
            {
                Value = float4x4.Scale(scale)
            });

            _boxColliderInstaller.Install(entity);
    
            // Теперь это можно безболезненно комментировать
            // _spriteRendererInstaller.Install(entity);
    
            LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(entity);
            localTransform.Position = position;
            entityManager.SetComponentData(entity, localTransform);

            return entity;
        }

        private void Update()
        {
            UpdateBounds();
        }


        private void UpdateBounds()
        {
            if (_references?.playCamera == null) return;

            Camera cam = _references.editSceneCamera;
            
            // 1. Вычисляем ширину линии в 1 пиксель
            // Используем pixelHeight камеры. Если камера рендерит в RenderTexture, 
            // cam.pixelHeight вернет высоту этой текстуры.
            float unitPerPixel = (cam.orthographicSize * 2f) / cam.pixelHeight;


            float height = _references.playCamera.orthographicSize;
            float width = height * _references.playCamera.aspect;
            Vector3 center = _references.playCamera.transform.position;

            // Смещение на пол-пикселя (0.5f * unitPerPixel), чтобы рамка шла 
            // строго по краю или чуть снаружи/внутри
            float halfPixel = unitPerPixel * 0.5f;

            // Вычисляем углы с учетом рассчитанной толщины
            Vector3 topLeft     = center + new Vector3(-width - halfPixel,  height + halfPixel, -center.z);
            Vector3 topRight    = center + new Vector3( width + halfPixel,  height + halfPixel, -center.z);
            Vector3 bottomRight = center + new Vector3( width + halfPixel, -height - halfPixel, -center.z);
            Vector3 bottomLeft  = center + new Vector3(-width - halfPixel, -height - halfPixel, -center.z);


        }
    }
}