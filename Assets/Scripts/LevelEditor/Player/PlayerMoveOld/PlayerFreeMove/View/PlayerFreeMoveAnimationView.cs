using DG.Tweening;
using UnityEngine;

public class PlayerFreeMoveAnimationView : MonoBehaviour
{
    [SerializeField] private GameObject scaleObject;
    [SerializeField] private GameObject centerObject;
    [Space] [SerializeField] private float _moveScaleX;
    [SerializeField] private float _moveScaleY;
    [SerializeField] private float _moveScaleDuraction;
    [SerializeField] private float _stopDuraction;
    [Space] [SerializeField] private float _dashScaleX;
    [SerializeField] private float _dashScaleY;
    [Space] [SerializeField] private float _rotateDuraction;

    private Vector2 _startScale;
    
    private Tween _tweenDash;
    private Tween _tweenRotate;
    private Tween _tweenScale;

    private void Awake()
    {
        _startScale = scaleObject.transform.localScale;
    }

    public void Dash(float dashDuraction)
    {
        scaleObject.transform.localScale = new Vector2(_dashScaleX, _dashScaleY);
        _tweenDash =
            scaleObject.transform.DOScale(new Vector3(_moveScaleX, _moveScaleY), dashDuraction);
    }

    public void Move()
    {
        if (_tweenScale != null && _tweenScale.IsActive() && _tweenScale.IsPlaying())
            _tweenScale?.Kill();
        _tweenScale = scaleObject.transform.DOScale(new Vector3(_moveScaleX, _moveScaleY), _moveScaleDuraction);
    }

    public void NotMove()
    {
        // IsActive(true) проверяет, не убит ли твин.
        if (_tweenDash != null && _tweenDash.IsActive() && _tweenDash.IsPlaying())
            _tweenDash?.Kill();
        scaleObject.transform.DOScale(_startScale, _stopDuraction);
    }

    public void SetVelocity(float angle)
    {
        // IsActive(true) проверяет, не убит ли твин.
        if (_tweenRotate != null && _tweenRotate.IsActive() && _tweenRotate.IsPlaying())
            _tweenRotate?.Kill();
        _tweenRotate = centerObject.transform.DORotate(new Vector3(0, 0, angle), _rotateDuraction);
    }

    private void OnDestroy()
    {
        _tweenDash.Kill();
        _tweenRotate.Kill();
        _tweenScale.Kill();
    }
}