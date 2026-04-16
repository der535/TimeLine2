using System.IO;
using EventBus;
using TimeLine.LevelEditor.Save;
using TimeLine.Select_levels;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.SpriteLoader
{
    public class RemoveGalleryPicture : MonoBehaviour
    {
        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref SpriteStorageRemoveSpriteEvent eventData) =>
            {
                string galleryPath = $"{Application.persistentDataPath}/Levels/{LevelBaseInfoStorage.levelBaseInfo.levelName}/Pictures/{eventData.TextureData.Id}.png";
                File.Delete(galleryPath);
            });
        }
    }
}