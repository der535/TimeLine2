using UnityEngine;

public class M_AudioPlaybackService
{
    private readonly AudioSource _source;
    
    public M_AudioPlaybackService(AudioSource source) => _source = source;

    public float CurrentTime => _source.time;
    public bool IsPlaying => _source.isPlaying;
    public float ClipLength => _source.clip ? _source.clip.length : 0;
    public AudioClip Clip => _source.clip;

    public void Play() => _source.Play();
    public void Pause() => _source.Pause();
    public void SetClip(AudioClip clip) => _source.clip = clip;
    public void SetTime(float seconds) => _source.time = Mathf.Clamp(seconds, 0, ClipLength);
}