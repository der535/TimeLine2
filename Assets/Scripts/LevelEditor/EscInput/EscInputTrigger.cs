using EventBus;
using TimeLine.EventBus.Events.Grid;
using Zenject;

namespace TimeLine.LevelEditor.EscInput
{
    public class EscInputTrigger : IInitializable
    {
        private readonly ActionMap _actionMap;
        private readonly GameEventBus _gameEventBus;

        [Inject]
        private EscInputTrigger(ActionMap actionMap, GameEventBus gameEventBus)
        {
            _actionMap = actionMap;
            _gameEventBus = gameEventBus;
        }

        public void Initialize()
        {
            _actionMap.Editor.ESC.started += _ => { _gameEventBus.Raise(new EscapePressedEvent()); };
        }
    }
}