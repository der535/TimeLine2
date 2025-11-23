using DG.Tweening;
using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine
{
    public class TimeLineRestartAnimation : MonoBehaviour
    {
        private TrackObjectStorage _trackObjectStorage;
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        private Main _main;
        
        internal bool IsPlayerDeath;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap, Main main, TrackObjectStorage trackObjectStorage)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _main = main;
            _trackObjectStorage = trackObjectStorage;
        }
        
        internal void Play()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(DOVirtual.Float(1f, 0.3f, 0.5f, (value) =>
            {
                _main.SetSpeed(value);
            }));
            sequence.Append(DOVirtual.Float(0.3f, -1f, 0.8f, (value) =>
            {
                _main.SetSpeed(value);
                
            })).OnComplete(() =>
            {
                _main.Pause();
                _main.SetSpeed(1);
                _gameEventBus.Raise(new RestartGameEvent());
            });
            sequence.Play();
        }
    }
}