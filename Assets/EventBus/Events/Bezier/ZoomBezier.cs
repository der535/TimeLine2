using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public struct ZoomBezier : IEvent
    {
        public float OldZoom { get; }
        public float Zoom { get; }

        public ZoomBezier(float zoom, float oldZoom)
        {
            Zoom = zoom;
            OldZoom = oldZoom;
        }
    }
}