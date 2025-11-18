using EventBus;

namespace TimeLine.EventBus.Events.Input
{
    public struct SetBPMEvent: IEvent
    {
        public float BPM { get; }

        public SetBPMEvent(float bpm)
        {
            BPM = bpm;
        }
    }
}