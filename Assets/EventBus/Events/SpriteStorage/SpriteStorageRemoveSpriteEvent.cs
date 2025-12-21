using System.Collections.Generic;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.SpriteLoader;
using UnityEngine;

namespace TimeLine
{
    public class SpriteStorageRemoveSpriteEvent : IEvent
    {
        public TextureData TextureData { get; }

        public SpriteStorageRemoveSpriteEvent(TextureData textureDataData)
        {
            TextureData = textureDataData;
        }
    }
}
