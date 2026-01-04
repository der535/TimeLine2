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
        public GameEventBus _eventBus; 
        
        // Теперь словарь хранит СПИСОК параметров для каждого TextureData
        private Dictionary<TextureData, List<SpriteParameter>> _spriteRenderers = new();
        public Dictionary<TextureData, SpriteParameter> TextureData = new();
        
        private static CustomSpriteStorage _instance;
        public static CustomSpriteStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CustomSpriteStorage>();
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

        private void Awake()
        {
            _spriteRenderers.Clear();
            TextureData.Clear();
            _eventBus.SubscribeTo((ref SpriteStorageRemoveSpriteEvent data) =>
            {
                RemoveSprite(data.TextureData);
            });
        }

        public void AddSprite(Sprite sprite, TextureData textureData)
        {
            var param = new SpriteParameter(textureData.Id, sprite, Color.black);
            if (!TextureData.ContainsKey(textureData))
            {
                TextureData.Add(textureData, param);
            }
            _eventBus.Raise(new SpriteStorageAddSpriteEvent(new KeyValuePair<TextureData, SpriteParameter>(textureData, param)));
        }

        public Sprite GetSpriteFromID(string id)
        {
            foreach (var data in TextureData)
            {
                if(data.Key.Id == id) return data.Value.Value;
            }
            return null;
        }

        [Button]
        public void Save()
        {
            List<TextureData> sprites = new List<TextureData>(TextureData.Keys);
            string json = JsonConvert.SerializeObject(sprites);
            string galleryPath = $"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}/Gallery.json";
            File.WriteAllText(galleryPath, json);
        }

        [Button]
        public void Load(Action onFinish)
        {
            onLoaded = onFinish;
            string galleryPath = $"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}/Gallery.json";
            
            if (File.Exists(galleryPath))
            {
                var sprites = JsonConvert.DeserializeObject<List<TextureData>>(File.ReadAllText(galleryPath));
                coutTask = sprites?.Count ?? 0;
                
                if (coutTask <= 0)
                {
                    onFinish?.Invoke();
                    return;
                }

                foreach (var data in sprites)
                {
                    _spriteLoadController.LoadTextureFromSave(data, CheckTasks);
                }
            }
            else
            {
                onFinish?.Invoke();
            }
        }

        private Action onLoaded;
        private int coutTask;
        
        private void CheckTasks()
        {
            coutTask--;
            if (coutTask <= 0)
            {
                onLoaded?.Invoke();
            }
        }

        // Добавляет рендерер в список по ключу
        public void AddSpriteRenderer(TextureData textureData, SpriteParameter spriteParameter)
        {
            if (!_spriteRenderers.ContainsKey(textureData))
            {
                _spriteRenderers[textureData] = new List<SpriteParameter>();
            }
            
            if (!_spriteRenderers[textureData].Contains(spriteParameter))
            {
                _spriteRenderers[textureData].Add(spriteParameter);
            }
        }

        // Удаляет конкретный рендерер из всех списков
        public void CheckAndRemoveSpriteRenderer(SpriteParameter spriteParameter)
        {
            foreach (var list in _spriteRenderers.Values)
            {
                if (list.Contains(spriteParameter))
                {
                    list.Remove(spriteParameter);
                    break; 
                }
            }
        }

        // Полностью удаляет спрайт и очищает все связанные рендереры
        public void RemoveSprite(TextureData textureData)
        {
            if (_spriteRenderers.TryGetValue(textureData, out var renderers))
            {
                foreach (var renderer in renderers)
                {
                    renderer.Value = null;
                }
                _spriteRenderers.Remove(textureData);
            }
            
            if (TextureData.ContainsKey(textureData))
            {
                TextureData.Remove(textureData);
            }
        }

        // Метод для обновления всех рендереров, подписанных на этот TextureData
        private void UpdateAllRenderersForKey(TextureData textureData, Sprite sprite)
        {
            if (_spriteRenderers.TryGetValue(textureData, out var renderers))
            {
                foreach (var renderer in renderers)
                {
                    renderer.Value = sprite;
                }
                Debug.Log($"Updated {renderers.Count} renderers for {textureData.Id}");
            }
        }

        public void UpdateCard(TextureData textureData)
        {
            // Загружаем новый спрайт и обновляем ВСЕ связанные объекты
            string path = $"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}/Pictures/{textureData.Id}.png";
            
            StartCoroutine(SpriteLoad.LoadSpriteFromPath(path, textureData, (newSprite) =>
            {
                // Обновляем основное хранилище
                if (TextureData.TryGetValue(textureData, out var mainParam))
                {
                    mainParam.Value = newSprite;
                }

                // Обновляем все зарегистрированные SpriteRenderers
                UpdateAllRenderersForKey(textureData, newSprite);
            }));
        }
    }
}