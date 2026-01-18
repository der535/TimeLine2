using EventBus;
using UnityEngine;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public struct ThemeChangedEvent : IEvent
    {
        public ThemeSO Theme;

        public ThemeChangedEvent(ThemeSO theme)
        {
            Theme = theme;
        }
    }
}