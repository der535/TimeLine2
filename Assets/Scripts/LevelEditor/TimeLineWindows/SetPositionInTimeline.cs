using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.Input;
using TimeLine.LevelEditor.Core;
using Zenject;

public class SetPositionInTimeline
{
    private TimeLineRenderer _timeLineRenderer;
    private TimeLineConverter _timeLineConverter;
    private TimeLineScroll _timeLineScroll;
    private GameEventBus _eventBus;
    
    [Inject]
    public SetPositionInTimeline(TimeLineRenderer timeLineRenderer, TimeLineConverter timeLineConverter, GameEventBus eventBus, TimeLineScroll timeLineScroll)
    {
        _timeLineRenderer = timeLineRenderer;
        _eventBus = eventBus;
        _timeLineConverter = timeLineConverter;
        _timeLineScroll = timeLineScroll;
    }
    /// <summary>
    /// Устанавливает позицию в тамйлане на основе anchor position
    /// </summary>
    public void SetAnchorPosition(float position)
    {
        _timeLineRenderer.SetPosition(position);
        _eventBus.Raise(new ScrollTimeLineEvent(0));
    }

    /// <summary>
    /// Устанавливает позицию в тамйлане на основе тиков
    /// </summary>
    public void SetPosition(float ticks)
    {
        SetAnchorPosition(_timeLineConverter.TicksToPositionX(-ticks, _timeLineScroll.Zoom));
    }
    
}