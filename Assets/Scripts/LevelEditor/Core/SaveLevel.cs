using System;
using System.Collections.Generic;
using System.IO;
using EventBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using TimeLine.Parent;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class SaveLevel : MonoBehaviour
    {
        // === Serialized Fields ===
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [FormerlySerializedAs("objectSpawner")] [FormerlySerializedAs("trackObjectSpawner")] [SerializeField] private FacadeObjectSpawner facadeObjectSpawner;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;
        [SerializeField] private SaveComposition composition;
        [SerializeField] private SaveEditorSettings editorSettings;
        [SerializeField] private TrackStorage trackStorage;
        [SerializeField] private FinishLevelController finishLevelController;

        [SerializeField] private GroupCreater groupCreater;

        // === Private Fields ===
        private LevelBaseInfo _levelBaseInfo;
        private GameEventBus _gameEventBus;
        private MainObjects _mainObjects;
        private CustomSpriteStorage _customSpriteStorage;
        private ParentLinkRestorer _parentLinkRestorer;
        public LevelBaseInfo LevelBaseInfo => _levelBaseInfo;

        [Inject]
        private void Construct(GameEventBus gameEventBus, MainObjects mainObjects,
            CustomSpriteStorage customSpriteStorage, ParentLinkRestorer parentLinkRestorer)
        {
            _gameEventBus = gameEventBus;
            _mainObjects = mainObjects;
            _customSpriteStorage = customSpriteStorage;
            _parentLinkRestorer = parentLinkRestorer;
        }

        // === Unity Lifecycle ===
        private void Awake()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = { new ColorConverter() },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };

            _gameEventBus.SubscribeTo((ref SetBPMEvent data) => { LevelBaseInfo.bpm = data.BPM; });
            _gameEventBus.SubscribeTo((ref SetOffsetEvent data) => { LevelBaseInfo.offset = data.Offset; });

            _gameEventBus.SubscribeTo((ref OpenEditorEvent eventData) => { _levelBaseInfo = eventData.LevelInfo; });
        }


        public void Save()
        {
            BackupManager.CreateRollingBackup($"{Application.persistentDataPath}/Levels/{_levelBaseInfo.levelName}", maxBackups: 5);

            editorSettings.Save();
            composition.Save();
            finishLevelController.Save();
            _customSpriteStorage.Save();

            var saveLevelDto = new SaveLevelDTO();

            saveLevelDto.Lines = trackStorage.TrackLines.Count;

            foreach (var group in trackObjectStorage.TrackObjectGroups)
                saveLevelDto.groupGameObjectSaveData.Add(SaveGroup(group, true));

            foreach (var track in trackObjectStorage.TrackObjects)
                saveLevelDto.gameObjectSaveData.Add(SaveGameObject(track, ""));

            string directoryPath = $"{Application.persistentDataPath}/Levels/{_levelBaseInfo.levelName}";
            Directory.CreateDirectory(directoryPath);

            string filePath = $"{directoryPath}/LevelObjects.json";
            string json = JsonConvert.SerializeObject(saveLevelDto, Formatting.Indented);
            File.WriteAllText(filePath, json);
            print(filePath);
            print(json);

            string levelBaseInfoPath =
                $"{Application.persistentDataPath}/Levels/{_levelBaseInfo.levelName}/LevelBaseInfo.json";
            
            File.WriteAllText(levelBaseInfoPath, JsonConvert.SerializeObject(LevelBaseInfo, Formatting.Indented));
        }

        internal void Load(LevelBaseInfo levelBaseInfo)
        {
            _levelBaseInfo = levelBaseInfo;

            editorSettings.Load();
            composition.Load();
            finishLevelController.Load();
            _customSpriteStorage.Load(() =>
            {
                
                string path = $"{Application.persistentDataPath}/Levels/{_levelBaseInfo.levelName}/LevelObjects.json";
                if (!File.Exists(path)) return;

                string json = File.ReadAllText(path);
                var saveLevelDto = JsonConvert.DeserializeObject<SaveLevelDTO>(json);

                // 1. Загружаем корневые НЕ-группы (обычные объекты) в правильном порядке
                foreach (var saveData in saveLevelDto.gameObjectSaveData)
                {
                    facadeObjectSpawner.LoadObject(saveData);
                }
                
                // 2. Загружаем корневые ГРУППЫ в правильном порядке
                foreach (var groupBase in saveLevelDto.groupGameObjectSaveData)
                {
                    //
                    var group = groupBase;
                    GroupGameObjectSaveData groupGameObjectSaveData =
                        composition.FindCompositionDataById(group.compositionID);
                    facadeObjectSpawner.LoadComposition(group, group.compositionID, groupGameObjectSaveData);
                    // LoadGroup(group, ""); // ← рекурсивная загрузка с сортировкой детей
                }

                _parentLinkRestorer.Restor();
            });
        }
        
        internal GameObjectSaveData SaveGameObject(TrackObjectData trackObject, string groupID)
        {
            string parentObjectID = string.Empty;
            // if (trackObject.sceneObject.transform.parent != null && trackObject.sceneObject.transform.parent != _mainObjects.SceneObjectParent.transform)
            // {
            //     parentObjectID = trackObjectStorage
            //         .GetTrackObjectData(trackObject.sceneObject.transform.parent.gameObject).sceneObjectID;
            // }

            if (groupID == parentObjectID) parentObjectID = string.Empty;

            var saveData = new GameObjectSaveData
            {
                lineIndex = trackStorage.GetTrackLineIndex(trackObject.trackObject.TrackLine),
                gameObjectName = trackObject.sceneObject.name,
                startTime = trackObject.trackObject.StartTimeInTicks,
                duractionTime = trackObject.trackObject.TimeDuractionInTicks,
                sceneObjectID = trackObject.sceneObjectID,
                parentObjectID = trackObject.trackObject._parentID,
                branch = trackObject.branch.ToSaveData(),
                Components = new List<ComponentData>(),
                tracks = new List<TrackSaveData>()
            };

            var parameterComponents = trackObject.sceneObject.GetComponents<IParameterComponent>();
            foreach (var component in parameterComponents)
            {
                var compData = new ComponentData
                {
                    ComponentType = component.GetComponentTypeName(),
                    Parameters = component.GetParameterData()
                };
                saveData.Components.Add(compData);
            }

            SaveKeyframeTrack(trackObject.branch.Root, saveData);
            return saveData;
        }

        public GroupGameObjectSaveData SaveGroup(TrackObjectGroup group, bool saveGroupID = false)
        {
            var baseData = SaveGameObject(group, "");
            GroupGameObjectSaveData groupData;
            if (saveGroupID == false)
            {
                groupData = new GroupGameObjectSaveData
                {
                    lineIndex = trackStorage.GetTrackLineIndex(group.trackObject.TrackLine),
                    lastEditID = group.lastEditID,
                    sceneObjectID = baseData.sceneObjectID,
                    parentObjectID = baseData.parentObjectID,
                    gameObjectName = baseData.gameObjectName,
                    startTime = baseData.startTime,
                    duractionTime = baseData.duractionTime,
                    branch = baseData.branch,
                    Components = baseData.Components,
                    tracks = baseData.tracks,
                    children = new List<GameObjectSaveData>()
                };
            }
            else
            {
                groupData = new GroupGameObjectSaveData
                {
                    lineIndex = trackStorage.GetTrackLineIndex(group.trackObject.TrackLine),
                    lastEditID = group.lastEditID,
                    compositionID = group.compositionID,
                    sceneObjectID = baseData.sceneObjectID,
                    parentObjectID = baseData.parentObjectID,
                    startTime = baseData.startTime,
                    duractionTime = baseData.duractionTime,
                    branch = baseData.branch,
                    Components = baseData.Components,
                    tracks = baseData.tracks,
                };
            }

            if (saveGroupID == false)
            {
                foreach (var child in group.TrackObjectDatas)
                {
                    if (child is TrackObjectGroup childGroup)
                    {
                        groupData.children.Add(SaveGroup(childGroup, true));
                    }
                    else
                    {
                        groupData.children.Add(SaveGameObject(child, ""));
                    }
                }
            }


            return groupData;
        }

        internal GroupGameObjectSaveData FullSave(TrackObjectGroup group)
        {
            var baseData = SaveGameObject(group, "");
            GroupGameObjectSaveData groupData;

            groupData = new GroupGameObjectSaveData
            {
                lineIndex = trackStorage.GetTrackLineIndex(group.trackObject.TrackLine),
                sceneObjectID = baseData.sceneObjectID,
                parentObjectID = baseData.parentObjectID,
                compositionID = group.compositionID,
                lastEditID = group.lastEditID,
                gameObjectName = baseData.gameObjectName,
                startTime = baseData.startTime,
                duractionTime = baseData.duractionTime,
                branch = baseData.branch,
                Components = baseData.Components,
                tracks = baseData.tracks,
                children = new List<GameObjectSaveData>()
            };

            foreach (var child in group.TrackObjectDatas)
            {
                if (child is TrackObjectGroup childGroup)
                {
                    groupData.children.Add(FullSave(childGroup));
                }
                else
                {
                    groupData.children.Add(SaveGameObject(child, ""));
                }
            }

            return groupData;
        }

        private void SaveKeyframeTrack(TreeNode node, GameObjectSaveData saveData)
        {
            Track track = keyframeTrackStorage.GetTrack(node);
            if (track != null)
            {
                saveData.tracks.Add(new TrackSaveData
                {
                    branchPath = $"{node.Path}",
                    animationColor = track.AnimationColor,
                    keyframeSaveData = track.SaveKeyframes()
                });
            }

            foreach (var child in node.Children)
            {
                SaveKeyframeTrack(child, saveData);
            }
        }
    }

    #region DTO

    [System.Serializable]
    public class SaveLevelDTO
    {
        public List<GroupGameObjectSaveData> groupGameObjectSaveData = new();
        public List<GameObjectSaveData> gameObjectSaveData = new();
        public int Lines;
    }

    [System.Serializable]
    public class GameObjectSaveData
    {
        public int lineIndex = 0;
        public string sceneObjectID = string.Empty;
        public string parentObjectID = string.Empty;
        public string gameObjectName;
        public double startTime;
        public double duractionTime;
        public BranchSaveData branch;
        public List<ComponentData> Components = new();
        public List<TrackSaveData> tracks = new();
    }

    [System.Serializable]
    public class GroupGameObjectSaveData : GameObjectSaveData
    {
        public string compositionID;
        public string lastEditID;

        // Может содержать как обычные объекты, так и другие группы
        public List<GameObjectSaveData> children = new();

        internal GroupGameObjectSaveData DuplicateComposition()
        {
            return new GroupGameObjectSaveData()
            {
                gameObjectName = gameObjectName,
                startTime = startTime,
                duractionTime = duractionTime,
                branch = branch.Duplicate(),
                Components = new List<ComponentData>(Components),
                tracks = new List<TrackSaveData>(tracks),
                compositionID = Guid.NewGuid().ToString(),
                lastEditID = lastEditID,
                children = new List<GameObjectSaveData>(children)
            };
        }
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

        public TreeNodeSaveData Duplicate()
        {
            return new TreeNodeSaveData
            {
                Path = Path,
            };
        }
    }

    [System.Serializable]
    public class BranchSaveData
    {
        public string ID;
        public string Name;
        public List<TreeNodeSaveData> Nodes = new();

        public BranchSaveData Duplicate()
        {
            var duplicate = new BranchSaveData
            {
                ID = ID,
                Name = Name,
                Nodes = new List<TreeNodeSaveData>()
            };

            if (Nodes != null)
            {
                foreach (var node in Nodes)
                {
                    duplicate.Nodes.Add(node?.Duplicate());
                }
            }

            return duplicate;
        }
    }


    [System.Serializable]
    public class KeyframeSaveData
    {
        public double Ticks;
        public Keyframe.Keyframe.InterpolationType InterpolationType;
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