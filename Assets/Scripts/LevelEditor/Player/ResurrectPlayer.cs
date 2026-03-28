using EventBus;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Player
{
    public class ResurrectPlayer : IInitializable
    {
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap; 
        private PlayerComponents _playerComponents;

        // Внедрение зависимостей
        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap, PlayerComponents playerComponents)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _playerComponents = playerComponents;
        }

        public void Initialize()
        {
            _gameEventBus.SubscribeTo((ref RestartGameEvent _) => Resurrect());
            _gameEventBus.SubscribeTo((ref TurnToPlayModeEvent _) => Resurrect());
        }

        private void Resurrect()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(_playerComponents.Player);
            playerTransform.Position = new Vector3(0, 0, playerTransform.Position.z);
            entityManager.SetComponentData(_playerComponents.Player, playerTransform);
            _playerComponents.SetActive(true);
            _actionMap.Player.Enable(); // Включаем управление
        }
    }
}