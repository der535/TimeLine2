using System;
using UnityEngine.EventSystems;

namespace TimeLine.LevelEditor.Helpers
{
    public static class UIUtils
    {
        public static void AddPointerListener(EventTrigger trigger, EventTriggerType type, Action action)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
        }
    }
}