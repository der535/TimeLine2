using TimeLine;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
using UnityEngine.Serialization;
using Zenject;

public class Main : MonoBehaviour
{
    [SerializeField] private MusicDataSO musicData;
    [SerializeField] private float offset;
    [Space]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float minResetOffset;

    private const float SecondsInMunit = 60f;
    public MusicDataSO MusicDataSo => musicData;
    
    public float CurrentTime => audioSource.time;

    private float _smoothTime;

    
    private GameEventBus _gameEventBus;
    private TimeLineSettings _timeLineSettings;
    

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

    // void Init()
    // {
    //     onTimeChangedUnsmooth.Raise(this,0);
    //     onTimeChangedSmooth.Raise(this, 0);
    // }

    public void Play()
    {
        audioSource.Play();
    }
    
    // public void Evaluate(float time)
    // {
    //     foreach (Track track in tracks)
    //     {
    //         track.Evaluate(time);
    //     }
    // }
    
    
    public void SetTime(float beats)
    {
        float time = beats * (SecondsInMunit / MusicDataSo.bpm);
        audioSource.time = time;
        _smoothTime = time;
        _gameEventBus.Raise(new SmoothTimeEvent(time));
        _gameEventBus.Raise(new ExactTimeEvent(time));
        // Evaluate(time);
    }

    public void Pause()
    {
        audioSource.Pause();
        _smoothTime = audioSource.time;
    }

    private void Update()
    {
        if (audioSource.isPlaying && _smoothTime == 0 && audioSource.time > 0)
        {
            _smoothTime = audioSource.time;
            _gameEventBus.Raise(new ExactTimeEvent(audioSource.time- offset));
            _gameEventBus.Raise(new SmoothTimeEvent(_smoothTime- offset));
        }
        else if (audioSource.isPlaying&& audioSource.time > 0)
        {
            _smoothTime += Time.deltaTime;
            
            _gameEventBus.Raise(new ExactTimeEvent(audioSource.time- offset));
            _gameEventBus.Raise(new SmoothTimeEvent(_smoothTime- offset));
        }

        if (minResetOffset < audioSource.time - _smoothTime)
        {
            audioSource.time = _smoothTime;
        }
    }
}