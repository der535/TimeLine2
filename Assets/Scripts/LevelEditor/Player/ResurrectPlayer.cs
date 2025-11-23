using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Player
{
    public class ResurrectPlayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject player;
        
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        private Main _main;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap, Main main)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _main = main;
        }
        private void Start()
        {
            _gameEventBus.SubscribeTo((ref RestartGameEvent _) => Resurrect());
        }
        private void Resurrect()
        {
            player.transform.position = new Vector3(0, 0, 0);
            spriteRenderer.enabled = true;
            _actionMap.Player.Enable();
        }
    }
}