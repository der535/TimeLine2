using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public struct ZoomBezier : IEvent
    {
        public float OldPan { get; }
        public float Pan { get; }

        public ZoomBezier(float pan, float oldPan)
        {
            Pan = pan;
            OldPan = oldPan;
        }
    }
}