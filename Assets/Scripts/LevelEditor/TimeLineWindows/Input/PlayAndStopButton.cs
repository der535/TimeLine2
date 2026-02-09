using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.Misc;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayAndStopButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [Space]
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite stopSprite;
    
    private GameEventBus _gameEventBus;
    private Main _main;

    public bool _isPlaying;

    [Inject]
    private void Construct(Main main, GameEventBus gameEventBus)
    {
        _gameEventBus = gameEventBus;
        _main = main;
    }

    public void Turn()
    {
        _isPlaying = !_isPlaying;
        if (_isPlaying)
        {
            _gameEventBus.Raise(new ChangePlayMode(true));
            _main.Play();
        }
        else
        {
            _gameEventBus.Raise(new ChangePlayMode(false));
            _main.Pause();
        }
        image.sprite = _isPlaying ? stopSprite  : playSprite;
    }
}
