using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public struct PanBezier : IEvent
    {
        public float Pan { get; }

        public PanBezier(float pan)
        {
            Pan = pan;
        }
    }
}