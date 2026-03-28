using System;
using System.Collections;
using EventBus;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerHitAnimation : ITickable
    {
        private const float CountCycle = 5;
        private const float Transparent = 0.1f;

        private float _timer = 0f;
        private float _totalDuration = 0f;
        private bool _isActive = false;
        private Action _onFinishCallback;
        
        private Entity _player;
        private Material _playerMaterial;
        
        private GameEventBus _gameEventBus;
        
        [Inject]
        PlayerHitAnimation(GameEventBus gameEventBus)
        {
            gameEventBus.SubscribeTo((ref LevelLoadedEvent _) => Initialize());
        }

        public void Initialize()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = entityManager.CreateEntityQuery(typeof(PlayerTag));
            _player = query.GetSingletonEntity();
            
            RenderMeshArray rma = entityManager.GetSharedComponentManaged<RenderMeshArray>(_player);  
            if (entityManager.HasComponent<MaterialMeshInfo>(_player))  
            {  
                var meshInfo = entityManager.GetComponentData<MaterialMeshInfo>(_player);  
  
                // Получаем текущий материал  
                _playerMaterial = rma.GetMaterial(meshInfo);  
            }
        }
        public void Play(float duration, Action onFinish)
        {
            _totalDuration = duration;
            _onFinishCallback = onFinish;
            _timer = 0f;
            _isActive = true;
        }

        // Этот метод нужно вызывать каждый кадр из какого-нибудь MonoBehaviour.Update()
        public void Tick()
        {
            if (!_isActive) return;

            _timer += Time.deltaTime;

            if (_timer >= _totalDuration)
            {
                StopAnimation();
                return;
            }

            // Логика мигания: делим общее время на количество циклов
            float cycleDuration = _totalDuration / CountCycle;
            // Если остаток от деления меньше половины цикла — делаем прозрачным
            bool isTransparent = (_timer % cycleDuration) < (cycleDuration / 2f);

            Color c = _playerMaterial.color;
            c.a = isTransparent ? Transparent : 1f;
            _playerMaterial.color = c;
        }

        private void StopAnimation()
        {
            _isActive = false;
            Color c = _playerMaterial.color;
            c.a = 1f;
            _playerMaterial.color = c;
            _onFinishCallback?.Invoke();
        }
    }
}