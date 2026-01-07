using EventBus;
using UnityEngine;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public struct MusicLoadedEvent : IEvent
    {
        public AudioClip audioClip;

        public MusicLoadedEvent(AudioClip audioClip)
        {
            this.audioClip = audioClip;
        }
    }
}