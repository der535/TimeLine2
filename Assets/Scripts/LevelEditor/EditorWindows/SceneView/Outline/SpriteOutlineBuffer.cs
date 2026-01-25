using System.Collections.Generic;
using EventBus;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace TimeLine.LevelEditor.outline
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteOutlineBuffer : MonoBehaviour
    {
        private static Dictionary<string, Sprite> _cachedSprites = new();

        private GameEventBus _gameEventBus;
        private BaseSpriteStorage _baseSpriteStorage;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, BaseSpriteStorage baseSpriteStorage)
        {
            _gameEventBus = gameEventBus;
            _baseSpriteStorage = baseSpriteStorage;
        }

        private void Start()
        {
            _cachedSprites.Clear();
            foreach (var sprite in _baseSpriteStorage.Sprites)
            {
                var outputSprite = SpritePaddingAdjuster.AdjustSpritePaddingPercent(sprite, 0.1f);
                _cachedSprites.Add(sprite.name, outputSprite);
            }
            
            _gameEventBus.SubscribeTo((ref SpriteStorageAddSpriteEvent data) =>
            {
                var outputSprite = SpritePaddingAdjuster.AdjustSpritePaddingPercent(data.Data.Value.Value, 0.1f);
                // print(data.Data.Value.Value.name);
                _cachedSprites.Add(data.Data.Value.Value.name, outputSprite);
            });
        }

        public void UpdateOutline(Sprite newSprite)
        {
            _cachedSprites[newSprite.name] = SpritePaddingAdjuster.AdjustSpritePaddingPercent(newSprite, 0.1f);
        }

        public Sprite GetSprite(string spriteID)
        {
            var containsKey = _cachedSprites.ContainsKey(spriteID);
            // print(spriteID);
            // print(_cachedSprites.ContainsKey(spriteID));
            if (containsKey)
                return _cachedSprites[spriteID];
            else
            {
                return null;
            }
        }
    }
}