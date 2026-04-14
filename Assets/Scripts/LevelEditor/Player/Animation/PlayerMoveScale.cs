using System;
using EventBus;
using TimeLine.LevelEditor.ECS.Services;
using TimeLine.LevelEditor.Player;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerMoveScale : MonoBehaviour
    {
        [SerializeField] private float moveScale = 0.1f;
        [SerializeField] private float dashScale = 0.1f;
        
        private float3 _baseScale = float3.zero;
        private float currentScale;
        
        private PlayerComponents _playerComponents;
        private ActionMap _actionMap;
        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(ActionMap actionMap, PlayerComponents playerComponents, GameEventBus gameEventBus)
        {
            _actionMap = actionMap;
            _playerComponents = playerComponents;
            _gameEventBus = gameEventBus;
        }
        
        public void SetNormalScale() => currentScale = moveScale;
        public void SetDashScale() => currentScale = dashScale;

        private void Start()
        {
            _gameEventBus.SubscribeTo<LevelLoadedEvent>((ref LevelLoadedEvent data) =>
            {
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                // entityManager.AddComponent<PostTransformMatrix>(_playerComponents.Player);
                PostTransformMatrix postTransformMatrix = entityManager.GetComponentData<PostTransformMatrix>(_playerComponents.Player);
                float3 nonUniformScale =
                    new float3(postTransformMatrix.Value.c0.x, postTransformMatrix.Value.c1.y, postTransformMatrix.Value.c2.z);
                _baseScale = nonUniformScale;
            }, -1);
        }

        public void Update()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if(!entityManager.Exists(_playerComponents.Player) || _baseScale.x == 0)return;
            Vector2 moveInput = _actionMap.Player.PlayerMove.ReadValue<Vector2>();
            var postMatrix = entityManager.GetComponentData<PostTransformMatrix>(_playerComponents.Player);
            if (moveInput.x != 0 || moveInput.y != 0)
            {
                PostTransformMatrix postTransformMatrix = entityManager.GetComponentData<PostTransformMatrix>(_playerComponents.Player);
                float3 scale = GetScaleFromMatrix.Get(postTransformMatrix.Value); 

                var newScale = _baseScale;
                newScale.x -= currentScale;
                newScale.y += currentScale;
                postMatrix.Value = float4x4.Scale(newScale);
                entityManager.SetComponentData(_playerComponents.Player, postMatrix);
            }
            else
            {
                postMatrix.Value = float4x4.Scale(_baseScale);
                entityManager.SetComponentData(_playerComponents.Player, postMatrix);
            }
        }
    }
}