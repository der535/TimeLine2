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
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.ComponentsLogic;
using TimeLine.LevelEditor.LoadingScreen.Controllers;
using TimeLine.LevelEditor.MaxObjectIndex.Controller;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using TimeLine.LevelEditor.UIAnimation;
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

        [FormerlySerializedAs("objectSpawner")] [FormerlySerializedAs("trackObjectSpawner")] [SerializeField]
        private FacadeObjectSpawner facadeObjectSpawner;

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
        private LoadingScreenController _loadingScreenController;
        private MaxObjectIndexController _maxObjectIndexController;
        private SaveButtonAnimation _saveButtonAnimation;
        public LevelBaseInfo LevelBaseInfo => _levelBaseInfo;

        [Inject]
        private void Construct(GameEventBus gameEventBus, MainObjects mainObjects,
            CustomSpriteStorage customSpriteStorage, ParentLinkRestorer parentLinkRestorer,
            LoadingScreenController loadingScreenController, MaxObjectIndexController maxObjectIndexController,
            SaveButtonAnimation saveButtonAnimation)
        {
            _gameEventBus = gameEventBus;
            _mainObjects = mainObjects;
            _customSpriteStorage = customSpriteStorage;
            _parentLinkRestorer = parentLinkRestorer;
            _loadingScreenController = loadingScreenController;
            _maxObjectIndexController = maxObjectIndexController;
            _saveButtonAnimation = saveButtonAnimation;
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
            _saveButtonAnimation.Saving();
            BackupManager.CreateRollingBackup($"{Application.persistentDataPath}/Levels/{_levelBaseInfo.levelName}",
                maxBackups: 5);

            editorSettings.Save();
            composition.Save();
            _maxObjectIndexController.Save();
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
            _saveButtonAnimation.Saved();
        }

        internal void Load(LevelBaseInfo levelBaseInfo)
        {
            _levelBaseInfo = levelBaseInfo;

            _loadingScreenController.Clear();
            // Сбрасываем старое состояние
            _loadingScreenController.AddStep("Загрузка настроек", () => editorSettings.Load());
            _loadingScreenController.AddStep("Загрузка настроек", () => finishLevelController.Load());
            _loadingScreenController.AddStep("Загрузка настроек", () => _maxObjectIndexController.Load());
            _loadingScreenController.AddStep("Загрузка композиций", () => composition.Load());

            // Используем наш новый метод для спрайтов
            _loadingScreenController.AddStep("Загрузка спрайтов", (onDone) =>
            {
                _customSpriteStorage.Load(onDone); // onDone вызовется внутри хранилища
            });

            _loadingScreenController.AddStep("Загрузка объектов таймлайна", () =>
            {
                LoadLevelObjects(); // Вынесли сложную логику в отдельный метод
            });

            _loadingScreenController.StartLoading();
        }

        private void LoadLevelObjects()
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
                facadeObjectSpawner.LoadComposition(group, group.compositionID, false, groupGameObjectSaveData);
                // LoadGroup(group, ""); // ← рекурсивная загрузка с сортировкой детей
            }

            _parentLinkRestorer.Restor();
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

            groupData.reduceRight = group.trackObject._reducedRight;
            groupData.reduceLeft = group.trackObject._reducedLeft;


            if (saveGroupID == false)
            {
                foreach (var child in group.TrackObjectDatas)
                {
                    if (child is TrackObjectGroup childGroup)
                    {
                        var data = SaveGroup(childGroup, true);
                        print(data.sceneObjectID);
                        groupData.children.Add(data);
                    }
                    else
                    {
                        var data = SaveGameObject(child, "");
                        print(data.sceneObjectID);
                        groupData.children.Add(data);
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
                children = new List<GameObjectSaveData>(),
            };

            groupData.reduceRight = group.trackObject._reducedRight;
            groupData.reduceLeft = group.trackObject._reducedLeft;

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

        public GameObjectSaveData Dublicate()
        {
            return new GameObjectSaveData()
            {
                lineIndex = lineIndex,
                sceneObjectID = sceneObjectID,
                parentObjectID = parentObjectID,
                gameObjectName = gameObjectName,
                startTime = startTime,
                duractionTime = duractionTime,
                branch = branch.Duplicate(),
                Components = new List<ComponentData>(Components) ,
                tracks = new List<TrackSaveData>(tracks) 
            };
        }
    }

    [System.Serializable]
    public class GroupGameObjectSaveData : GameObjectSaveData
    {
        public string compositionID;
        public string lastEditID { get; set; }
        public double reduceLeft;
        public double reduceRight;

        // Может содержать как обычные объекты, так и другие группы
        public List<GameObjectSaveData> children = new();

        internal GroupGameObjectSaveData DuplicateComposition()
        {
            List<GameObjectSaveData> newList = new List<GameObjectSaveData>();
            foreach (var data in children)
            {
                newList.Add(data.Dublicate());
            }
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
                children = newList,
                reduceLeft = reduceLeft,
                reduceRight = reduceRight
            };
        }
    }

    [System.Serializable]
    public class ComponentData
    {
        public string ComponentType;
        public Dictionary<string, ParameterPacket> Parameters;
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