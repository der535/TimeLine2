using EventBus;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

public class GoToCurrentTime : MonoBehaviour
{
    private GameEventBus _eventBus;
    private SetPositionInTimeline _setPositionInTimeline;
    private CurrentTimeMarkerRenderer _currentTimeMarkerRenderer;

    [Inject]
    private void Construct(GameEventBus eventBus, SetPositionInTimeline setPositionInTimeline, CurrentTimeMarkerRenderer timeLineMarkerRenderer)
    {
        this._eventBus = eventBus;
        _setPositionInTimeline  = setPositionInTimeline;
        _currentTimeMarkerRenderer = timeLineMarkerRenderer;
    }
    
    [Button]
    public void Go()
    {
        _setPositionInTimeline.SetAnchorPosition(-_currentTimeMarkerRenderer.TimeLineAnchoredPosition);
    }
}