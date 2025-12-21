using System;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.SpriteLoader
{
    public class GetSpriteName : MonoBehaviour
    {
        private BaseSpriteStorage _spriteStorage;
        private CustomSpriteStorage _customSpriteStorage;

        [Inject]
        private void Constructor(CustomSpriteStorage customSpriteStorage, BaseSpriteStorage baseSpriteStorage)
        {
            _customSpriteStorage = customSpriteStorage;
            _spriteStorage = baseSpriteStorage;
        }

        public string Get(SpriteParameter spriteParameter)
        {
            // Check base storage

            foreach (var sprite in _spriteStorage.Sprites)
            {
                if (sprite.name == spriteParameter.Value.name) return sprite.name;
            }
            
            // Check custom storage
            foreach (var data in _customSpriteStorage.TextureData)
            {
                if (data.Value.Value.name == spriteParameter.Value.name) return data.Key.SpriteName;
            }


            return String.Empty;
        }
    }
}