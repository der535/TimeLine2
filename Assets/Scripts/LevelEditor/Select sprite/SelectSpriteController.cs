using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.SpriteLoader;
using Unity.Rendering;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class SelectSpriteController : MonoBehaviour
    {
        [SerializeField] private RectTransform windows;
        [SerializeField] private SpriteCard prefab;
        [SerializeField] private RectTransform content;
        [SerializeField] private BaseSpriteStorage storage;
        [SerializeField] private CustomSpriteStorage customStorage;

        private List<SpriteCard> _spriteCards = new();
        private bool _isInitialized;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            foreach (var card in storage.Sprites)
            {
                SpriteCard spriteCard = Instantiate(prefab, content);
                spriteCard.Setup(card, null); // Изначально без действия
                _spriteCards.Add(spriteCard);
            }
            
            _gameEventBus.SubscribeTo((ref SpriteStorageAddSpriteEvent spriteStorageAddSpriteEvent) =>
            {
                SpriteCard spriteCard = Instantiate(prefab, content);
                spriteCard.Setup(spriteStorageAddSpriteEvent.Data.Value, spriteStorageAddSpriteEvent.Data.Key,
                    null); // Изначально без действия
                _spriteCards.Add(spriteCard);
            });
            
            _gameEventBus.SubscribeTo((ref SpriteStorageRemoveSpriteEvent data) =>
            {
                foreach (var card in _spriteCards)
                {
                    if (card.textureData == data.TextureData)
                    {
                        Destroy(card.gameObject);
                    }
                }
            });
        }

        internal void CheckSpriteRendererAndAdd(SpriteParameter spriteParameter)
        {
            foreach (var card in _spriteCards)
            {

                if (card.SpriteParameter != null && card.SpriteParameter.Value == spriteParameter.Value)
                {
                    customStorage.AddSpriteRenderer(card.textureData, spriteParameter);
                    return;
                }
            }
        }

        internal void Setup(SpriteParameter spriteParameter)
        {
            windows.gameObject.SetActive(true);

            foreach (var card in _spriteCards)
            {
                Action onSelect = () =>
                {
                    if (card.SpriteParameter != null)
                    {
                        spriteParameter.Value = card.SpriteParameter.Value;
                        customStorage.CheckAndRemoveSpriteRenderer(spriteParameter);
                        customStorage.AddSpriteRenderer(card.textureData, spriteParameter);
                    }
                    else
                    {
                        spriteParameter.Value = card.sprite;
                        customStorage.CheckAndRemoveSpriteRenderer(spriteParameter);
                    }

                    windows.gameObject.SetActive(false);
                    // _gameEventBus.Raise(new SelectedNewSpriteEvent(spriteParameter));
                };

                if (card.SpriteParameter != null)
                {
                    card.Setup(card.SpriteParameter, card.textureData, onSelect);
                }
                else
                {
                    card.Setup(card.sprite, onSelect);
                }
            }
        }
        
        internal void Setup(Sprite sprite, Action<Texture> onValueChanged)
        {
            windows.gameObject.SetActive(true);

            foreach (var card in _spriteCards)
            {
                Action onSelect = () =>
                {
                    if (card.SpriteParameter != null)
                    {
                        onValueChanged.Invoke(card.SpriteParameter.Value.texture);
                    }
                    else
                    {
                        Debug.Log(card.sprite.texture.name);

                        onValueChanged.Invoke(card.sprite.texture);
                    }

                    windows.gameObject.SetActive(false);
                    
                    if(sprite != null)
                        _gameEventBus.Raise(new SelectedNewSpriteEvent(sprite));
                };

                if (card.SpriteParameter != null)
                {
                    card.Setup(card.SpriteParameter, card.textureData, onSelect);
                }
                else
                {
                    card.Setup(card.sprite, onSelect);
                }
            }
        }
    }
}