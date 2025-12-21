using System;
using System.Collections.Generic;
using System.IO;
using EventBus;
using NaughtyAttributes;
using Newtonsoft.Json;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.SpriteLoader
{
    public class CustomSpriteStorage : MonoBehaviour
    {
        private SaveLevel _saveLevel;
        private Main _main;
        private SpriteLoadController _spriteLoadController;
        public GameEventBus _eventBus; // Добавить евент добовления спрайта и сделать подписку на обноление галереи спрайтов
        
        private Dictionary<TextureData, SpriteParameter> _spriteRenderers = new();
        
        private static CustomSpriteStorage _instance;
        public static CustomSpriteStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Пытаемся найти существующий объект на сцене
                    _instance = FindObjectOfType<CustomSpriteStorage>();

                    // Если не найден — создаём временный объект (только в рантайме!)
                    if (_instance == null && Application.isPlaying)
                    {
                        GameObject obj = new GameObject("CustomSpriteStorage");
                        _instance = obj.AddComponent<CustomSpriteStorage>();
                    }
                }
                return _instance;
            }
        }

        [Inject]
        private void Constructor(SaveLevel saveLevel, GameEventBus eventBus, Main main, SpriteLoadController spriteLoadController)
        {
            _saveLevel = saveLevel;
            _main = main;
            _eventBus = eventBus;
            _spriteLoadController = spriteLoadController;
        }

        public readonly Dictionary<TextureData, SpriteParameter> TextureData = new();

        private void Start()
        {
            _eventBus.SubscribeTo((ref SpriteStorageRemoveSpriteEvent data) =>
            {
                RemoveSprite(data.TextureData);
            } );
        }

        public void AddSprite(Sprite sprite, TextureData textureData)
        {
            
            var param = new SpriteParameter(textureData.Id, sprite, Color.black);
            TextureData[textureData] = param; // или TryAdd, если нужно избегать перезаписи
            _eventBus.Raise(new SpriteStorageAddSpriteEvent(new KeyValuePair<TextureData, SpriteParameter>(textureData, param)));
            _spriteRenderers.Add(textureData, param);
        }

        // public string GetSpriteName(SpriteParameter spriteParameter)
        // {
        //     foreach (var data in _spriteRenderers)
        //     {
        //         if(data.Value.Value.name == spriteParameter.Value.name) return data.Key.SpriteName;
        //     }
        //     return String.Empty;
        // }

        public Sprite GetSpriteFromID(string id)
        {
            foreach (var data in _spriteRenderers)
            {
                print(data.Key.Id);
                if(data.Key.Id == id) return data.Value.Value;
            }
            return null;
        }

        [Button]
        public void Save()
        {
            List<TextureData> sprites = new();
            foreach (var data in TextureData)
            {
                sprites.Add(data.Key);
            }
            string json = JsonConvert.SerializeObject(sprites);
            string galleryPath = $"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}/Gallery.json";
            File.WriteAllText(galleryPath, json);
        }

        [Button]
        public void Load(Action onFinish)
        {
            onLoaded = onFinish;
            coutTask = 0;
            List<TextureData> sprites = new();
            string galleryPath = $"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}/Gallery.json";
            if (File.Exists(galleryPath))
            {
                sprites = JsonConvert.DeserializeObject<List<TextureData>>(File.ReadAllText(galleryPath));
                coutTask = sprites.Count;
                foreach (var VARIABLE in sprites)
                {
                    _spriteLoadController.LoadTextureFromSave(VARIABLE, CheckTasks);
                }
            }
            
        }

        private Action onLoaded;
        int coutTask;
        
        private void CheckTasks()
        {
            coutTask--;
            if (coutTask <= 0)
            {
                onLoaded.Invoke();
            }
        }

        public void AddSpriteRenderer(TextureData textureData, SpriteParameter spriteParameter)
        {
            _spriteRenderers.TryAdd(textureData, spriteParameter);
            // Optionally: else Debug.LogWarning("Key already exists, skipped add.");
        }

        public void CheckAndRemoveSpriteRenderer(SpriteParameter textureData)
        {
            var data = Check(textureData)?.Key;
            if (data != null) _spriteRenderers.Remove(data);
        }

        public void RemoveSprite(TextureData textureData)
        {
            foreach (var VARIABLE in CheckTextureData(textureData))
            {
                VARIABLE.Value.Value = null;
                _spriteRenderers.Remove(VARIABLE.Key);
            }
        }

        private List<KeyValuePair<TextureData, SpriteParameter>> CheckTextureData(TextureData textureData)
        {
            List<KeyValuePair<TextureData, SpriteParameter>> result = new();
            foreach (KeyValuePair<TextureData, SpriteParameter> pair in _spriteRenderers)
            {
                if (pair.Key == textureData)
                {
                    result.Add(pair);
                }
            }
            return result;
        }

        private KeyValuePair<TextureData, SpriteParameter>? Check(SpriteParameter spriteParameter)
        {
            foreach (KeyValuePair<TextureData, SpriteParameter> pair in _spriteRenderers)
            {
                if (pair.Value == spriteParameter)
                {
                    return pair;
                }
            }
            return null;
        }

        private void UpdateSpriteRenderers(Sprite sprite)
        {
            print("UpdateSpriteRenderers");
            foreach (var renderer in _spriteRenderers)
            {
                print(renderer.Key.Id == sprite.name);
                if (renderer.Key.Id == sprite.name)
                {
                    print("set value");
                    renderer.Value.Value = sprite;
                }
            }
        }

        public void UpdateCard(TextureData textureData)
        {
            print("UpdateCard");
            print(textureData);
            foreach (KeyValuePair<TextureData, SpriteParameter> data in _spriteRenderers)
            {
                if (data.Key == textureData)
                { 
                    print(textureData.Id);
                   StartCoroutine(SpriteLoad.LoadSpriteFromPath(
                        $"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}/Pictures/{textureData.Id}.png",
                        textureData, (value) =>
                        {
                            data.Value.Value = value;
                            UpdateSpriteRenderers(data.Value.Value);
                        }));
                    return;
                }
            }
        }
    }
}