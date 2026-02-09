using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace TimeLine.LevelEditor.SpriteLoader
{
    public class SpriteLoadController : MonoBehaviour
    {
        [SerializeField] private FileBrowserSelectPicture fileBrowser;
        [SerializeField] private SpriteGallery gallery;

        private SaveLevel _saveLevel;
        private CustomSpriteStorage _customSpriteStorage;

        [Inject]
        private void Constructor(SaveLevel saveLevel, CustomSpriteStorage customSpriteStorage)
        {
            _saveLevel = saveLevel;
            _customSpriteStorage = customSpriteStorage;
        }

        public void SelectSprite()
        {
            fileBrowser.OpenFilePanel((value) =>
            {
                if (value.Count == 0) return;
                foreach (var VARIABLE in value)
                {
                    var sourcePath = VARIABLE;
                    print(VARIABLE);
                    string fileName = Path.GetFileNameWithoutExtension(VARIABLE);
                    
                    var id = System.Guid.NewGuid().ToString("N");
                    var pngFileName = $"{id}.png";
                    var destinationDir = Path.Combine(Application.persistentDataPath, "Levels", _saveLevel.LevelBaseInfo.levelName, "Pictures");
                    Directory.CreateDirectory(destinationDir);
                    var destinationPath = Path.Combine(destinationDir, pngFileName);

                    // Загружаем изображение как байты
                    var fileData = File.ReadAllBytes(sourcePath);

                    // Создаём временный Texture2D
                    var texture = new Texture2D(2, 2);
                    texture.LoadImage(fileData); // автоматически обрабатывает jpg, png и др.

                    // Конвертируем в PNG
                    var pngData = texture.EncodeToPNG();

                    // Сохраняем
                    File.WriteAllBytes(destinationPath, pngData);

                    // // Очищаем текстуру (опционально)
                    // DestroyImmediate(texture);

                    CreateTexture(destinationPath, id, fileName);
                }
             
            });
        }

        public void CreateTexture(string filePath, string fileId, string fileName)
        {
            TextureData textureData = new TextureData(fileId, fileName, FilterMode.Bilinear, 100f);
            
            StartCoroutine(SpriteLoad.LoadSpriteFromPath(filePath, textureData, (sprite) =>
            {
                _customSpriteStorage.AddSprite(sprite, textureData);
            }));
        }

        public void LoadTextureFromSave(TextureData textureData, Action onLoaded)
        {
            // print("load sprite from save");
            string filePath =$"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}/Pictures/{textureData.Id}.png";
            // print(filePath);
            StartCoroutine(SpriteLoad.LoadSpriteFromPath(filePath, textureData, (sprite) =>
            {
                // print(sprite);
                // print(textureData);
                _customSpriteStorage.AddSprite(sprite, textureData);
                onLoaded?.Invoke();
            }));
        }
    }

    public class TextureData
    {
        public TextureData(string id, string spriteName, FilterMode filterMode, float pixelsPerUnit)
        {
            Id = id;
            SpriteName = spriteName;
            FilterMode = filterMode;
            PixelsPerUnit = pixelsPerUnit;
        }

        public Action OnValueChanged;
        public string Id; // File name
        public string SpriteName;
        public FilterMode FilterMode;
        public float PixelsPerUnit;
    }
}