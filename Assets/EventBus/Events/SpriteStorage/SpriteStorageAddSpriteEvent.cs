using System.Collections.Generic;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.SpriteLoader;
using UnityEngine;

namespace TimeLine
{
    public class SpriteStorageAddSpriteEvent : IEvent
    {
        public KeyValuePair<TextureData, SpriteParameter> Data { get; }

        public SpriteStorageAddSpriteEvent(KeyValuePair<TextureData, SpriteParameter> data)
        {
            Data = data;
        }
    }
}
