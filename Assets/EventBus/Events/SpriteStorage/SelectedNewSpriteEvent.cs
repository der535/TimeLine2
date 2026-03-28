using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;

namespace TimeLine
{
    public class SelectedNewSpriteEvent: IEvent
    {
        public Sprite Sprite { get; }

        public SelectedNewSpriteEvent(Sprite sprite)
        {
            Sprite = sprite;
        }
    }
}