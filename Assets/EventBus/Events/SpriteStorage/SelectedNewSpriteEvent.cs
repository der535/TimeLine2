using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;

namespace TimeLine
{
    public class SelectedNewSpriteEvent: IEvent
    {
        public SpriteParameter SpriteParameter { get; }

        public SelectedNewSpriteEvent(SpriteParameter spriteParameter)
        {
            SpriteParameter = spriteParameter;
        }
    }
}