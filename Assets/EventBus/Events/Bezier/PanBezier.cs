using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public struct PanBezier : IEvent
    {
        public float OldPan { get; }
        public float Pan { get; }

        public PanBezier(float pan, float oldPan)
        {
            Pan = pan;
            OldPan = oldPan;
        }
    }
}