using UnityEngine;

[CreateAssetMenu(fileName = "Default", menuName = "ScriptableObjects/Music data", order = 1)]
public class MusicDataSO : ScriptableObject
{
    public string musicName;
    public string authorName;
    public AudioClip music;
    public float bpm;
    public float startOffset;
}
