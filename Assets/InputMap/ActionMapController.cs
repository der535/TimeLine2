using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class ActionMapController : MonoBehaviour
    {
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

            _gameEventBus.SubscribeTo((ref OpenEditorEvent openEditorEvent) =>
            {
                // print(_actionMap.Editor.enabled);
                _actionMap.Editor.Enable();
                // print(_actionMap.Editor.enabled);
            });
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