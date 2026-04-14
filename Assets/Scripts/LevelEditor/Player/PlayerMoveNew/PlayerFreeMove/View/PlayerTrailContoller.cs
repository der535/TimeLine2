using TimeLine.LevelEditor.Player;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerTrailContoller : MonoBehaviour
    {
        PlayerComponents _playerComponents;

        [Inject]
        private void Construct(PlayerComponents playerComponents)
        {
            _playerComponents = playerComponents;
        }

        void Update()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if(entityManager.Exists(_playerComponents.Player))
                transform.position = entityManager.GetComponentData<LocalToWorld>(_playerComponents.Player).Position;
        }
    }
}
