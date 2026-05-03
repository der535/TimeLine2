using DG.Tweening;
using EventBus;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Player
{
    public class ResurrectPlayer : IInitializable
    {
        private GameEventBus _gameEventBus;
        private PlayerInputView _playerInputView;
        private PlayerComponents _playerComponents;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, PlayerInputView playerInputView, PlayerComponents playerComponents)
        {
            _gameEventBus = gameEventBus;
            _playerInputView = playerInputView;
            _playerComponents = playerComponents;
        }

        public void Initialize()
        {
            _gameEventBus.SubscribeTo((ref RestartGameEvent _) => Resurrect(), -1);
            _gameEventBus.SubscribeTo((ref TurnToPlayModeEvent _) => Resurrect());
        }

        private void Resurrect()
        {
            _gameEventBus.Raise(new PlayerInvulnerabilityStartedEvent());

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(_playerComponents.Player);
            playerTransform.Position = new Vector3(0, 0, playerTransform.Position.z);
            entityManager.SetComponentData(_playerComponents.Player, playerTransform);
            _playerComponents.ChangeActive(true);
            _playerInputView.OnEnable();
        }
    }
}