using System.Collections.Generic;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.SpriteLoader
{
    public class SpriteGallery : MonoBehaviour
    {
        [SerializeField] private RectTransform cardRoot;
        [SerializeField] private GalleryCard galleryCard;
        [SerializeField] private SpriteEdit spriteEdit;
        
        List<GalleryCard> cards = new List<GalleryCard>();
        
        private GameEventBus _gameEventBus;
        private CustomSpriteStorage _customSpriteStorage;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, CustomSpriteStorage customSpriteStorage)
        {
            _gameEventBus = gameEventBus;
            _customSpriteStorage = customSpriteStorage;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref SpriteStorageAddSpriteEvent data) =>
            {
                AddSprite(data.Data.Key, data.Data.Value);
            });
        }
        
        private void SortCardsAlphabetically()
        {
            // 1. Сортируем список по имени (предположим, у GalleryCard есть свойство Name или доступ к тексту)
            // Если имя хранится в spriteParameter или TextureData, используйте их.
            cards.Sort((a, b) => string.Compare(a.GetName(), b.GetName(), System.StringComparison.OrdinalIgnoreCase));

            // 2. Устанавливаем порядок в иерархии согласно отсортированному списку
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].transform.SetSiblingIndex(i);
            }
        }

        private void AddSprite(TextureData textureData, SpriteParameter spriteParameter)
        {
            GalleryCard card = Instantiate(galleryCard, cardRoot).GetComponent<GalleryCard>();
            card.Setup(spriteParameter, textureData, () =>
            {
                spriteEdit.Setup(spriteParameter, textureData, () => card.UpdateCard());
            }, () =>
            {
                _gameEventBus.Raise(new SpriteStorageRemoveSpriteEvent(textureData));
                // _customSpriteStorage.RemoveSprite(textureData);
                Destroy(card.gameObject);
                cards.Remove(card);
            });
            cards.Add(card);
            
            SortCardsAlphabetically();
        }
    }
}