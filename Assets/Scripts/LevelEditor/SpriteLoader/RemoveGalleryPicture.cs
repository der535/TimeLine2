using System.IO;
using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.SpriteLoader
{
    public class RemoveGalleryPicture : MonoBehaviour
    {
        private GameEventBus _gameEventBus;
        private SaveLevel _saveLevel;
        
        [Inject]
        private void Constructor(GameEventBus gameEventBus, SaveLevel saveLevel)
        {
            _gameEventBus = gameEventBus;
            _saveLevel = saveLevel;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref SpriteStorageRemoveSpriteEvent eventData) =>
            {
                string galleryPath = $"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}/Pictures/{eventData.TextureData.Id}.png";
                File.Delete(galleryPath);
            });
        }
    }
}