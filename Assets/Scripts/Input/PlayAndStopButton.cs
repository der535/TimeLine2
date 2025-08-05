using TimeLine;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayAndStopButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [Space]
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite stopSprite;
    
    private Main _main;

    private bool _isPlaying;

    [Inject]
    private void Main(Main main)
    {
        _main = main;
    }

    public void Turn()
    {
        _isPlaying = !_isPlaying;
        if(_isPlaying) _main.Play();
        else _main.Pause();
        image.sprite = _isPlaying ? stopSprite  : playSprite;
    }
}
