using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.Input;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

public class ScrollOld : MonoBehaviour
{
    private float _oldPan;

    private MainObjects _mainObjects;
    private TimeLineConverter _timeLineConverter;
    private TimeLineRenderer _timeLineRenderer;
    private GameEventBus _gameEventBus;

    [Inject]
    private void Construct(MainObjects mainObjects, TimeLineConverter timeLineConverter,
        TimeLineRenderer timeLineRenderer, GameEventBus gameEventBus)
    {
        _mainObjects = mainObjects;
        _timeLineConverter = timeLineConverter;
        _timeLineRenderer = timeLineRenderer;
        _gameEventBus = gameEventBus;
    }

    private void Start()
    {
        _gameEventBus.SubscribeTo((ref ScrollTimeLineEvent scrollEvent) =>
        {
            _mainObjects.ContentRectTransform.offsetMin += new Vector2(scrollEvent.ScrollOffset, 0); //Left
            _mainObjects.ContentRectTransform.offsetMax += new Vector2(scrollEvent.ScrollOffset, 0); //Right
            _mainObjects.NotifyContentRectChanged();
        }, 1);
        _gameEventBus.SubscribeTo((ref OldPanEvent oldPanEvent) => _oldPan = oldPanEvent.OldPanOffset,1);
        _gameEventBus.SubscribeTo((ref PanEvent oldPanEvent) =>
        {
            float curPos = _timeLineConverter.GetCursorBeatPosition(_oldPan, 0);
            _timeLineRenderer.SetPosition(-(_timeLineConverter.GetAnchorPositionFromBeatPosition(curPos) -
                                            _timeLineConverter.CursorPosition().x));
        },1);
    }


    public void SetPosition(float position)
    {
        _mainObjects.ContentRectTransform.offsetMin = new Vector2(position, 0); //Left
        _mainObjects.ContentRectTransform.offsetMax = new Vector2(position, 0); //Right
        _mainObjects.NotifyContentRectChanged();
    }
}