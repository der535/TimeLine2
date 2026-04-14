using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.SpriteLoader;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class DashParticleAnimation : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particle;

        private PlayerComponents _playerComponents;
        private ActionMap _actionMap;

        [Inject]
        private void Construct(PlayerComponents playerComponents, ActionMap actionMap)
        {
            _playerComponents = playerComponents;
            _actionMap = actionMap;
        }

        public void Play()
        {
            particle.transform.position = _playerComponents.GetPosition();
            particle.Play();

            Vector2 moveInput = _actionMap.Player.PlayerMove.ReadValue<Vector2>();
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90;
            transform.rotation = Quaternion.Euler(new Vector3(0,0,angle));
        }
    }
}