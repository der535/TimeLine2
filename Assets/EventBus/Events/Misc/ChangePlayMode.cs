using EventBus;

namespace TimeLine.EventBus.Events.Misc
{
    public class ChangePlayMode : IEvent
    {
        public bool IsPlaying { get; }

        public ChangePlayMode(bool isPlaying)
        {
            IsPlaying = isPlaying;
        }
    }
}