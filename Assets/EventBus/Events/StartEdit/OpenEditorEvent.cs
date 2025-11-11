using EventBus;
using UnityEngine;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public struct OpenEditorEvent : IEvent
    {
        public LevelBaseInfo LevelInfo { get; set; }

        public OpenEditorEvent(LevelBaseInfo levelBaseInfo)
        {
            Debug.LogError("Double raise OpenEditorEvent");
            LevelInfo = levelBaseInfo;
        }
    }
}