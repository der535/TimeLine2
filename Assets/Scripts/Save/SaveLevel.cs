using System.Collections.Generic;
using System.IO;
using EventBus;
using NaughtyAttributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class SaveLevel : MonoBehaviour
    {
        // === Serialized Fields ===
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;

        // === Private Fields ===
        private string _data;
        private GameObjectSaveData _saveData = new();
        private GroupGameObjectSaveData _groupSaveData = new();
        
        private SaveLevelDTO _saveLevelDto = new();

        private GameEventBus _gameEventBus = new();
        private LevelBaseInfo _levelBaseInfo;
        
        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        // === Unity Lifecycle ===
        private void Awake()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = { new ColorConverter() },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            
            _gameEventBus.SubscribeTo((ref OpenEditorEvent eventData) =>
            {
                _levelBaseInfo = eventData.LevelInfo;
                Load();
            });
        }

        // return JsonConvert.SerializeObject(saveData, Formatting.Indented); Сериализация
        
        #region Buttons
        // === Public API / Button Methods ===

        public void Save()
        {
            foreach (var track in trackObjectStorage.TrackObjectGroups)
            {
                _saveLevelDto.groupGameObjectSaveData.Add(SaveGroup(track));
            }
            
            foreach (var track in trackObjectStorage.TrackObjects)
            {
                _saveLevelDto.gameObjectSaveData.Add(SaveGameObject(track));
            }

            File.WriteAllText($"{Application.persistentDataPath}/Levels/{_levelBaseInfo.levelName}/LevelObjects.json", JsonConvert.SerializeObject(_saveLevelDto, Formatting.Indented));
        }

        private void Load()
        {
            string path = $"{Application.persistentDataPath}/Levels/{_levelBaseInfo.levelName}/LevelObjects.json";
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            _saveLevelDto = JsonConvert.DeserializeObject<SaveLevelDTO>(json);

            // Сначала загружаем обычные объекты
            foreach (var saveData in _saveLevelDto.gameObjectSaveData)
            {
                LoadGameObject(saveData);
            }
        }

        private void LoadGameObject(GameObjectSaveData data)
        {
            
        }
        
        #endregion

        public GroupGameObjectSaveData SaveGroup(TrackObjectGroup group)
        {
            GameObjectSaveData data = SaveGameObject(group);
            _groupSaveData = new GroupGameObjectSaveData();

            _groupSaveData.gameObjectName = data.gameObjectName;
            _groupSaveData.startTime = data.startTime;
            _groupSaveData.duractionTime = data.duractionTime;
            _groupSaveData.branch = data.branch;
            _groupSaveData.Components = data.Components;
            _groupSaveData.tracks = data.tracks;
            
            _groupSaveData.gameObjectSaveData = new List<GameObjectSaveData>();
            
            foreach (var trackObject in group.TrackObjectDatas)
            {
                _groupSaveData.gameObjectSaveData.Add(SaveGameObject(trackObject));
            }

            return _groupSaveData;
        }
        
        // === Save Logic ===
        public GameObjectSaveData SaveGameObject(TrackObjectData trackObject)
        {
            _saveData = new GameObjectSaveData();
            
            _saveData = new GameObjectSaveData
            {
                gameObjectName = trackObject.sceneObject.name,
                startTime = trackObject.trackObject.StartTimeInTicks,
                duractionTime = trackObject.trackObject.TimeDuractionInTicks,
                branch = trackObject.branch.ToSaveData()
            };

            // Save all IParameterComponent components
            var parameterComponents = trackObject.sceneObject.GetComponents<IParameterComponent>();
            foreach (var component in parameterComponents)
            {
                var compData = new ComponentData
                {
                    ComponentType = component.GetComponentTypeName(),
                    Parameters = component.GetParameterData()
                };
                _saveData.Components.Add(compData);
            }

            // Save recursively KeyframeTracks
            SaveKeyframeTrack(trackObject.branch.Root);

            return _saveData;
        }

        private void SaveKeyframeTrack(TreeNode node)
        {
            print(node.Path);
            Track track = keyframeTrackStorage.GetTrack(node);
            if (track != null)
            {
                _saveData.tracks.Add(new TrackSaveData
                {
                    branchPath = node.Path,
                    animationColor = track.AnimationColor,
                    keyframeSaveData = track.SaveKeyframes()
                });
            }

            foreach (var child in node.Children)
            {
                SaveKeyframeTrack(child);
            }
        }

        // === Load Logic (stubbed) ===
        public static void LoadGameObject(GameObject go, string json)
        {
            // var saveData = JsonConvert.DeserializeObject<GameObjectSaveData>(json);
            //
            // var existingComponents = go.GetComponents<IParameterComponent>()
            //     .ToDictionary(c => c.GetComponentTypeName(), c => c);
            //
            // foreach (var compData in saveData.Components)
            // {
            //     if (existingComponents.TryGetValue(compData.ComponentType, out var component))
            //     {
            //         component.SetParameterData(compData.Parameters);
            //     }
            //     else
            //     {
            //         Debug.LogWarning($"Component {compData.ComponentType} not found on {go.name} during load.");
            //     }
            // }
        }
    }

    #region DTO

    [System.Serializable]
    public class SaveLevelDTO
    {
        public List<GroupGameObjectSaveData> groupGameObjectSaveData = new List<GroupGameObjectSaveData>();
        public List<GameObjectSaveData> gameObjectSaveData = new List<GameObjectSaveData>();
    }
        
    [System.Serializable]
    public class GroupGameObjectSaveData : GameObjectSaveData
    {
        public List<GameObjectSaveData> gameObjectSaveData;
    }
    
    [System.Serializable]
    public class GameObjectSaveData
    {
        public string gameObjectName;
        
        public double startTime;
        public double duractionTime;
        
        public BranchSaveData branch;
        
        public List<ComponentData> Components = new();
        public List<TrackSaveData> tracks = new();
    }
    
    [System.Serializable]
    public class ComponentData
    {
        public string ComponentType;
        public Dictionary<string, object> Parameters;
    }
    
    [System.Serializable]
    public class TreeNodeSaveData
    {
        public string Path;
        public string Name;
    }

    [System.Serializable]
    public class BranchSaveData
    {
        public string ID;
        public string Name;
        public List<TreeNodeSaveData> Nodes = new();
    }

    [System.Serializable]
    public class KeyframeSaveData
    {
        public double Ticks;
        public double OutTangent;
        public double InTangent;
        public double InWeight;
        public double OutWeight;
        public string DataType;
        public JObject Data;
    }

    [System.Serializable]
    public class TrackSaveData
    {
        public string branchPath;
        public Color animationColor;
        public List<KeyframeSaveData> keyframeSaveData = new();
    }
    
    #endregion

}