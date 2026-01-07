using DG.Tweening;
using EventBus;
using TimeLine.LevelEditor.Controllers;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine
{
    public class TimeLineRestartAnimation : MonoBehaviour
    {
        private TrackObjectStorage _trackObjectStorage;
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        private TimeLineSpeedController _timeLineSpeedController;
        private M_AudioPlaybackService _audioPlaybackService;

        internal bool IsPlayerDeath;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap,
            TrackObjectStorage trackObjectStorage, TimeLineSpeedController timeLineSpeedController, M_AudioPlaybackService playbackService)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _trackObjectStorage = trackObjectStorage;
            _timeLineSpeedController = timeLineSpeedController;
            _audioPlaybackService = playbackService;
        }

        internal void Play()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(DOVirtual.Float(1f, 0.3f, 0.5f, (value) => { _timeLineSpeedController.SetSpeed(value); }));
            sequence.Append(DOVirtual.Float(0.3f, -1f, 0.8f, (value) => { _timeLineSpeedController.SetSpeed(value); }))
                .OnComplete(() =>
                {
                    _audioPlaybackService.Pause();
                    _timeLineSpeedController.SetSpeed(1);
                    _gameEventBus.Raise(new RestartGameEvent());
                });
            sequence.Play();
        }
    }
}