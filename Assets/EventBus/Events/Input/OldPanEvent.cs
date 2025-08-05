using EventBus;
using UnityEngine;

namespace TimeLine
{
    public class OldPanEvent : IEvent
    {
        public float OldPanOffset { get; }

        public OldPanEvent(float oldPan)
        {
            OldPanOffset = oldPan;
        }
    }
}
