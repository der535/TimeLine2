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
            });
        }
    }
}