using EventBus;
using UnityEngine;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public struct OpenEditorEvent : IEvent
    {
        public LevelBaseInfo LevelInfo { get; set; }

        public OpenEditorEvent(LevelBaseInfo levelBaseInfo)
        {
            LevelInfo = levelBaseInfo;
        }
    }
}