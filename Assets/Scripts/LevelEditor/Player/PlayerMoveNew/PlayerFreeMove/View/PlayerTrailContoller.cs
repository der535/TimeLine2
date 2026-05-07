using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Player.PlayerMoveNew.PlayerFreeMove.View
{
    public class PlayerTrailContoller : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particleSystem;
        
        private PlayerComponents _playerComponents;
        private PlayerMover _playerMover;
        
        [Inject]
        private void Construct(PlayerComponents playerComponents, PlayerMover playerMover)
        {
            _playerComponents = playerComponents;
            _playerMover = playerMover;
            _playerMover._onMovePerformed += vector2 =>
            {
                if(vector2.x != 0f || vector2.y != 0f)
                    particleSystem.Play();
                else
                {
                    particleSystem.Stop();
                }
                
                float3 eulerRadians = math.Euler(Quaternion.identity);
                float3 eulerDegrees = math.degrees(eulerRadians);
                float angle = Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg-90;
                particleSystem.gameObject.transform.rotation = quaternion.Euler(new Vector3(0,0,math.radians(angle)));
            };
            
            
        }
        
        void Update()
        {
            
            
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (entityManager.Exists(_playerComponents.Player))
                transform.position = entityManager.GetComponentData<LocalToWorld>(_playerComponents.Player).Position;
        }
    }
}