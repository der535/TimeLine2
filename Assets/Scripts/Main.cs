using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using UnityEngine;
using Zenject;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Threading;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.TimeLine;
using UnityEngine.Networking;

namespace TimeLine
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private float offset; // in seconds
        [Space] [SerializeField] private AudioSource audioSource;
        [SerializeField] private float minResetOffset; // in seconds

        internal const double TICKS_PER_BEAT = 96.0; // 96 ticks per quarter note
        private const double SECONDS_IN_MINUTE = 60.0;

        public MusicData MusicData;
        private TimeLineConverter _timeLineConverter;

        public double TicksCurrentTime()
        {
            return _timeLineConverter.SecondsToTicks(audioSource.time);
        }

        private double _smoothTimeInTicks;
        private double _exactTimeInTicks;

        private GameEventBus _gameEventBus;
        private TimeLineSettings _timeLineSettings;

        private bool _isPlaying;

        // Conversion properties
        private double SecondsPerTick => SECONDS_IN_MINUTE / (MusicData.bpm * TICKS_PER_BEAT);
        private double TicksPerSecond => 1.0 / SecondsPerTick;

        [Inject]
        private void Construct(GameEventBus gameEventBus, TimeLineSettings timeLineSettings,
            TimeLineConverter timeLineConverter)
        {
            _gameEventBus = gameEventBus;
            _timeLineSettings = timeLineSettings;
            _timeLineConverter = timeLineConverter;
        }

        private void Awake()
        {
            MusicData = new MusicData();
            _gameEventBus.SubscribeTo((ref OpenEditorEvent data) =>
            {
                MusicData.bpm = data.LevelInfo.bpm;
                StartCoroutine(LoadAudioClip(
                    $"{Application.persistentDataPath}/Levels/{data.LevelInfo.levelName}/{data.LevelInfo.songName}"));
            });
            _gameEventBus.SubscribeTo((ref OpenEditorEvent data) => { SetTimeInTicks(0); }, 1);


            CultureInfo culture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        IEnumerator LoadAudioClip(string filePath)
        {
            // Определяем расширение файла и сопоставляем с AudioType
            AudioType audioType = GetAudioTypeFromPath(filePath);

            if (audioType == AudioType.UNKNOWN)
            {
                Debug.LogError("Неизвестный формат аудиофайла: " + filePath);
                yield break;
            }

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = clip;
                    MusicData.music = clip;
                    _gameEventBus.Raise(new MusicLoadedEvent());
                }
                else
                {
                    Debug.LogError("Ошибка загрузки аудио: " + www.error);
                }
            }
        }

        private AudioType GetAudioTypeFromPath(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            return extension switch
            {
                ".mp3" => AudioType.MPEG,
                ".wav" => AudioType.WAV,
                ".ogg" => AudioType.OGGVORBIS,
                ".aif" or ".aiff" => AudioType.AIFF,
                ".xm" => AudioType.XM,
                ".mod" => AudioType.MOD,
                ".s3m" => AudioType.S3M,
                ".it" => AudioType.IT,
                _ => AudioType.UNKNOWN
            };
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
            _isPlaying = true;

            if (_smoothTimeInTicks >= 0)
            {
                audioSource.Play();
                _exactTimeInTicks = SecondsToTicks(audioSource.time);
                _smoothTimeInTicks = _exactTimeInTicks;
            }
        }

        public void SetTime(float beats)
        {
            double ticks = beats * TICKS_PER_BEAT;
            SetTimeInTicks(ticks);
        }

        internal void SetTimeInTicks(double ticks)
        {
            double timeInSeconds = TicksToSeconds(ticks);
            // if (timeInSeconds < 0) timeInSeconds = 0;
            audioSource.time = timeInSeconds < 0 ? 0 : (float)timeInSeconds;
            _smoothTimeInTicks = ticks;
            _exactTimeInTicks = ticks;

            _gameEventBus.Raise(new TickSmoothTimeEvent(ticks));
            _gameEventBus.Raise(new TickExactTimeEvent(ticks));
        }

        public void Pause()
        {
            _isPlaying = false;

            audioSource.Pause();
            _exactTimeInTicks = SecondsToTicks(audioSource.time);
            _smoothTimeInTicks = _exactTimeInTicks;
        }

        private void Update()
        {
            if (!_isPlaying) return;

            if (_smoothTimeInTicks >= 0)
            {
                if (!audioSource.isPlaying) audioSource.Play();
                // Update exact time from audio source
                _exactTimeInTicks = SecondsToTicks(audioSource.time);

                // Update smooth time using Time.deltaTime
                _smoothTimeInTicks += SecondsToTicks(Time.deltaTime);

                // Apply offset to get the visual position
                double visualOffsetTicks = SecondsToTicks(offset);
                double exactVisualTimeInTicks = _exactTimeInTicks - visualOffsetTicks;
                double smoothVisualTimeInTicks = _smoothTimeInTicks - visualOffsetTicks;

                // Raise events
                _gameEventBus.Raise(new TickSmoothTimeEvent(smoothVisualTimeInTicks));
                _gameEventBus.Raise(new TickExactTimeEvent(exactVisualTimeInTicks));

                // Reset if too far off (using tick-based comparison)
                double resetThresholdTicks = SecondsToTicks(minResetOffset);
                if (Math.Abs(_smoothTimeInTicks - _exactTimeInTicks) > resetThresholdTicks)
                {
                    _smoothTimeInTicks = _exactTimeInTicks;
                }
            }
            else
            {
                // Update smooth time using Time.deltaTime
                _smoothTimeInTicks += SecondsToTicks(Time.deltaTime);
                _exactTimeInTicks = _smoothTimeInTicks;

                // Apply offset to get the visual position
                double visualOffsetTicks = SecondsToTicks(offset);
                double exactVisualTimeInTicks = _exactTimeInTicks - visualOffsetTicks;
                double smoothVisualTimeInTicks = _smoothTimeInTicks - visualOffsetTicks;

                // Raise events
                _gameEventBus.Raise(new TickSmoothTimeEvent(smoothVisualTimeInTicks));
                _gameEventBus.Raise(new TickExactTimeEvent(exactVisualTimeInTicks));
            }
        }
    }

    public class MusicData
    {
        public float bpm;
        public AudioClip music;
    }
}