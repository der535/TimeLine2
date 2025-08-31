using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using UnityEngine;
using Zenject;
using System;

namespace TimeLine
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private MusicDataSO musicData;
        [SerializeField] private float offset; // in seconds
        [Space]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float minResetOffset; // in seconds

        internal const double TICKS_PER_BEAT = 96.0; // 96 ticks per quarter note
        private const double SECONDS_IN_MINUTE = 60.0;
        
        public MusicDataSO MusicDataSo => musicData;
    
        public float CurrentTime => audioSource.time;

        private double _smoothTimeInTicks;
        private double _exactTimeInTicks;
        private double _lastAudioTime;
    
        private GameEventBus _gameEventBus;
        private TimeLineSettings _timeLineSettings;
    
        // Conversion properties
        private double SecondsPerTick => SECONDS_IN_MINUTE / (MusicDataSo.bpm * TICKS_PER_BEAT);
        private double TicksPerSecond => 1.0 / SecondsPerTick;

        [Inject]
        private void Construct(GameEventBus gameEventBus, TimeLineSettings timeLineSettings)
        {
            _gameEventBus = gameEventBus;
            _timeLineSettings = timeLineSettings;
        }

        private void Awake()
        {
            audioSource.clip = musicData.music;
        }

        
        public float Offset() => offset * _timeLineSettings.DistanceBetweenBeatLines;

        internal double SecondsToTicks(double seconds)
        {
            return seconds * TicksPerSecond;
        }

        internal double TicksToSeconds(double ticks)
        {
            return ticks * SecondsPerTick;
        }
        
        
    
        public void Play()
        {
            audioSource.Play();
            _lastAudioTime = audioSource.time;
            _exactTimeInTicks = SecondsToTicks(audioSource.time);
            _smoothTimeInTicks = _exactTimeInTicks;
        }
    
        public void SetTime(float beats)
        {
            double ticks = beats * TICKS_PER_BEAT;
            SetTimeInTicks(ticks);
        }

        internal void SetTimeInTicks(double ticks)
        {
            double timeInSeconds = TicksToSeconds(ticks);
            audioSource.time = (float)timeInSeconds;
            _smoothTimeInTicks = ticks;
            _exactTimeInTicks = ticks;
            _lastAudioTime = timeInSeconds;
            
            _gameEventBus.Raise(new SmoothTimeEvent((float)timeInSeconds));
            _gameEventBus.Raise(new ExactTimeEvent((float)timeInSeconds));
            
            _gameEventBus.Raise(new TickSmoothTimeEvent(ticks));
            _gameEventBus.Raise(new TickExactTimeEvent(ticks));
        }

        public void Pause()
        {
            audioSource.Pause();
            _exactTimeInTicks = SecondsToTicks(audioSource.time);
            _smoothTimeInTicks = _exactTimeInTicks;
            _lastAudioTime = audioSource.time;
        }

        private void Update()
        {
            if (!audioSource.isPlaying) return;

            // Update exact time from audio source
            _exactTimeInTicks = SecondsToTicks(audioSource.time);
            
            // Update smooth time using Time.deltaTime
            _smoothTimeInTicks += SecondsToTicks(Time.deltaTime);
            
            // Apply offset to get the visual position
            double visualOffsetTicks = SecondsToTicks(offset);
            double exactVisualTimeInTicks = _exactTimeInTicks - visualOffsetTicks;
            double smoothVisualTimeInTicks = _smoothTimeInTicks - visualOffsetTicks;
            
            // Convert back to seconds for events
            float exactTimeInSeconds = (float)TicksToSeconds(exactVisualTimeInTicks);
            float smoothTimeInSeconds = (float)TicksToSeconds(smoothVisualTimeInTicks);
            
            // Raise events
            _gameEventBus.Raise(new ExactTimeEvent(exactTimeInSeconds));
            _gameEventBus.Raise(new SmoothTimeEvent(smoothTimeInSeconds));
            
            _gameEventBus.Raise(new TickSmoothTimeEvent(exactVisualTimeInTicks));
            _gameEventBus.Raise(new TickExactTimeEvent(smoothVisualTimeInTicks));
            
            // Reset if too far off (using tick-based comparison)
            double resetThresholdTicks = SecondsToTicks(minResetOffset);
            if (Math.Abs(_smoothTimeInTicks - _exactTimeInTicks) > resetThresholdTicks)
            {
                _smoothTimeInTicks = _exactTimeInTicks;
            }
            
            // Store current audio time for next frame
            _lastAudioTime = audioSource.time;
        }
    }
}