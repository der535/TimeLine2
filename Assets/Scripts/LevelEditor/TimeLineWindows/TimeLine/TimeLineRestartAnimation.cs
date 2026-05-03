using DG.Tweening;
using EventBus;
using TimeLine.LevelEditor.Controllers;
using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.Player.PlayerMoveNew.PlayerFreeMove.View;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine
{
    public class TimeLineRestartAnimation
    {
        private GameEventBus _gameEventBus;
        private TimeLineSpeedController _timeLineSpeedController;
        private M_AudioPlaybackService _audioPlaybackService;
        private PlayerTrailContoller _playerTrailContoller;
        private PlayerComponents _playerComponents;

        internal bool IsPlayerDeath;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, 
            TimeLineSpeedController timeLineSpeedController,
            M_AudioPlaybackService playbackService,
            PlayerTrailContoller playerTrailContoller,
            PlayerComponents playerComponents)
        {
            _gameEventBus = gameEventBus;
            _timeLineSpeedController = timeLineSpeedController;
            _audioPlaybackService = playbackService;
            _playerTrailContoller = playerTrailContoller;
            _playerComponents = playerComponents;
        }

        internal void Play()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(DOVirtual.Float(1f, 0.3f, 0.5f, (value) =>
            {
                _timeLineSpeedController.SetSpeed(value);
            }));
            sequence.Append(DOVirtual.Float(0.3f, -1f, 0.8f, (value) =>
                {
                    _timeLineSpeedController.SetSpeed(value);
                }))
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