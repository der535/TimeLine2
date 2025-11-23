using NaughtyAttributes;
using UnityEngine;
using Zenject;

public class GoToCurrentTime : MonoBehaviour
{
    private TimeLineRenderer _timeLineRenderer;
    private CurrentTimeMarkerRenderer _currentTimeMarkerRenderer;

    [Inject]
    private void Construct(TimeLineRenderer timeLineRenderer,
        CurrentTimeMarkerRenderer currentTimeMarkerRenderer)
    {
        this._timeLineRenderer = timeLineRenderer;
        this._currentTimeMarkerRenderer = currentTimeMarkerRenderer;
    }
    
    [Button]
    public void Go()
    {
        _timeLineRenderer.SetPosition(-_currentTimeMarkerRenderer.TimeLineAnchoredPosition);
    }
}
