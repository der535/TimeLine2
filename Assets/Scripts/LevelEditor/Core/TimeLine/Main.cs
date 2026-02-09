using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using UnityEngine;
using Zenject;
using System;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.LevelEditor.Controllers;
using TimeLine.LevelEditor.Core.MusicOffset;
using TimeLine.TimeLine;

namespace TimeLine
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private float minResetOffset; // in seconds

        private TimeLineConverter _timeLineConverter;
        private CurrentTimeMarkerRenderer _currentTimeMarkerRenderer;
        private M_MusicOffsetData _musicOffsetData;
        private M_PlaybackState _state;
        private M_AudioPlaybackService _audioPlaybackService;
        private PlayAndStopButton _playAndStopButton;
        
        private GameEventBus _gameEventBus;
        
        private TimeLineSpeedController TimeLineSpeedController;

        [Inject]
        private void Construct(
            GameEventBus gameEventBus, 
            TimeLineConverter timeLineConverter, 
            CurrentTimeMarkerRenderer currentTimeMarkerRenderer,
            TimeLineSpeedController timeLineSpeedController,
            M_MusicOffsetData musicOffsetData,
            M_AudioPlaybackService audioPlaybackService,
            M_PlaybackState playbackState,
            PlayAndStopButton playAndStopButton)
        {
            _gameEventBus = gameEventBus;
            _timeLineConverter = timeLineConverter;
            _currentTimeMarkerRenderer  = currentTimeMarkerRenderer;
            TimeLineSpeedController = timeLineSpeedController;
            _musicOffsetData = musicOffsetData;
            _audioPlaybackService = audioPlaybackService;
            _state = playbackState;
            _playAndStopButton = playAndStopButton;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref MusicLoadedEvent data) =>
            {
                _audioPlaybackService.SetClip(data.audioClip);
            });
        }

        public void Play()
        {
            _state.IsPlaying = true;

            if (_state.IsFirstPlaying)
            {
                SetTimeInTicks(_state.SmoothTimeInTicks);
                _state.IsFirstPlaying = false;
            }
            
            if (_state.SmoothTimeInTicks >= 0)
            {
                _audioPlaybackService.Play();
                _state.ExactTimeInTicks = TimeLineConverter.Instance.SecondsToTicks(_audioPlaybackService.CurrentTime);
                _state.SmoothTimeInTicks = _state.ExactTimeInTicks;
            }
        }

        internal void SetTimeInTicks(double ticks, bool ignoreLastTime = false)
        {
            if(ignoreLastTime == false) 
                if(Math.Abs(_state.LastSetTime - _state.SmoothTimeInTicks) < 0.1f) return;
            double timeInSeconds = TimeLineConverter.Instance.TicksToSeconds(ticks);
            timeInSeconds += _musicOffsetData.Value;

            if(_audioPlaybackService.Clip != null)
                _audioPlaybackService.SetTime( timeInSeconds < 0 ? 0 : (float)timeInSeconds);
            _state.SmoothTimeInTicks = ticks;
            _state.ExactTimeInTicks = ticks;
            _state.LastSetTime = _state.SmoothTimeInTicks;

            _gameEventBus.Raise(new TickSmoothTimeEvent(ticks));
            _gameEventBus.Raise(new TickExactTimeEvent(ticks));
        }

        public void Pause()
        {
            _state.IsPlaying = false;

            _audioPlaybackService.Pause();
            _state.ExactTimeInTicks = TimeLineConverter.Instance.SecondsToTicks(_audioPlaybackService.CurrentTime);
            _state.SmoothTimeInTicks = _state.ExactTimeInTicks;
        }

        private void Update()
        {
            if (_state.SmoothTimeInTicks > 0 && _state.LastSetTime >= 0)
            {
                // _isPlaying = audioSource.isPlaying;
                if(_audioPlaybackService.IsPlaying == false && _playAndStopButton._isPlaying)
                    _playAndStopButton.Turn();
            }
             
            if (!_state.IsPlaying) return;
            
            // if(audioSource.time >= audioSource.clip.length) Pause();

            if (_state.SmoothTimeInTicks + _timeLineConverter.SecondsToTicks(_musicOffsetData.Value)  >= 0)
            {
                if (!_audioPlaybackService.IsPlaying) _audioPlaybackService.Play();
                // Update exact time from audio source
                _state.ExactTimeInTicks = TimeLineConverter.Instance.SecondsToTicks(_audioPlaybackService.CurrentTime);

                // Update smooth time using Time.deltaTime
                _state.SmoothTimeInTicks += TimeLineConverter.Instance.SecondsToTicks(Time.deltaTime) * TimeLineSpeedController.CurrentSpeed;

                // Apply offset to get the visual position
                double visualOffsetTicks = TimeLineConverter.Instance.SecondsToTicks(_musicOffsetData.Value);
                double exactVisualTimeInTicks = _state.ExactTimeInTicks - visualOffsetTicks;
                double smoothVisualTimeInTicks = _state.SmoothTimeInTicks - visualOffsetTicks;

                // Raise events
                _gameEventBus.Raise(new TickExactTimeEvent(exactVisualTimeInTicks));
                _gameEventBus.Raise(new TickSmoothTimeEvent(smoothVisualTimeInTicks));

                // Reset if too far off (using tick-based comparison)
                double resetThresholdTicks = TimeLineConverter.Instance.SecondsToTicks(minResetOffset);
                if (Math.Abs(_state.SmoothTimeInTicks - _state.ExactTimeInTicks) > resetThresholdTicks)
                {
                    _state.SmoothTimeInTicks = _state.ExactTimeInTicks;
                }
            }
            else
            {
                // Update smooth time using Time.deltaTime
                _state.SmoothTimeInTicks += TimeLineConverter.Instance.SecondsToTicks(Time.deltaTime) * TimeLineSpeedController.CurrentSpeed;
                _state.ExactTimeInTicks = _state.SmoothTimeInTicks;

                // Apply offset to get the visual position
                double visualOffsetTicks = 0;
                double exactVisualTimeInTicks = _state.ExactTimeInTicks - visualOffsetTicks;
                double smoothVisualTimeInTicks = _state.SmoothTimeInTicks - visualOffsetTicks;

                // Raise events
                _gameEventBus.Raise(new TickExactTimeEvent(exactVisualTimeInTicks));
                _gameEventBus.Raise(new TickSmoothTimeEvent(smoothVisualTimeInTicks));
            }
        }
    }

}