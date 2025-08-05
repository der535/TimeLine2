using DG.Tweening;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using UnityEngine;
using Zenject;

public class CircleBeat : MonoBehaviour
{
    [SerializeField] private AudioSource kickSound;

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float animationDuraction;

    [Header("Punch settings")] 
    [SerializeField] private float scaleMultiplier;
    [SerializeField] private float vibrato;
    [SerializeField] private int elasticity;

    private Tween _tween;
    private Vector2 _originalScale;
    private GameEventBus _gameEventBus;

    [Inject]
    private void Construct(GameEventBus gameEventBus)
    {
        _gameEventBus = gameEventBus;
    }

    private void Awake()
    {
        _originalScale = rectTransform.localScale;
        _gameEventBus.SubscribeTo<BeatEvent>(PlayAnimation);
    }

    public void PlayAnimation(ref BeatEvent beatEvent)
    {
        kickSound.Play();
        _tween?.Kill();
        rectTransform.localScale = _originalScale;
        _tween = rectTransform.DOPunchScale(new Vector3(scaleMultiplier, scaleMultiplier, 0), animationDuraction,
            elasticity);
    }
}