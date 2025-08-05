using EventBus;
using UnityEngine;

namespace TimeLine
{
    public class PanEvent : IEvent
    {
        public float PanOffset { get; }

        public PanEvent(float pan)
        {
            PanOffset = pan;
        }
    }
}
