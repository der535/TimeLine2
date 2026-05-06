using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class ActionMapController : MonoBehaviour
    {
        [FormerlySerializedAs("_windowsFocus")]
        [SerializeField] private WindowsFocus _sceneWindows;

        private ActionMap _actionMap;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(ActionMap actionMap, GameEventBus gameEventBus)
        {
            _actionMap = actionMap;
            _gameEventBus = gameEventBus;
        }

        private void Awake()
        {
            SetActive(true);
            _actionMap.Editor.Disable();

            _gameEventBus.SubscribeTo((ref OpenEditorEvent openEditorEvent) => { _actionMap.Editor.Enable(); });

            _gameEventBus.SubscribeTo((ref TurnToPlayModeEvent data) =>
            {
                _actionMap.Editor.Space.Disable();
                _actionMap.Player.Enable();
            });
            _gameEventBus.SubscribeTo((ref ExitPlayEvent data) =>
            {
                _actionMap.Editor.Space.Enable();
                if (_sceneWindows.IsFocused) _actionMap.Player.Enable();
                else _actionMap.Player.Disable();
            });
            _sceneWindows._changeActive += (active) =>
            {
                if (active) _actionMap.Player.Enable();
                else _actionMap.Player.Disable();
            };
        }

        private void SetActive(bool active)
        {
            if (active)
                _actionMap.Enable();
            else
                _actionMap.Disable();
        }
    }
}