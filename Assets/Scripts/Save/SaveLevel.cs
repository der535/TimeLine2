using System.Collections.Generic;
using System.IO;
using System.Linq;
using EventBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.Installers;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class SaveLevel : MonoBehaviour
    {
                // === Serialized Fields ===
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private TrackObjectSpawner trackObjectSpawner;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;

        // === Private Fields ===
        private LevelBaseInfo _levelBaseInfo;
        private GameEventBus _gameEventBus;
        private MainObjects _mainObjects;

        [Inject]
        private void Construct(GameEventBus gameEventBus, MainObjects mainObjects)
        {
            _gameEventBus = gameEventBus;
            _mainObjects = mainObjects;
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

            _gameEventBus.SubscribeTo((ref OpenEditorEvent eventData) =>
            {
                _levelBaseInfo = eventData.LevelInfo;
                Load();
            });
        }

        #region Buttons

        public void Save()
        {
            var saveLevelDto = new SaveLevelDTO();

            foreach (var group in trackObjectStorage.TrackObjectGroups)
            {
                saveLevelDto.groupGameObjectSaveData.Add(SaveGroup(group));
            }

            foreach (var track in trackObjectStorage.TrackObjects)
            {
                saveLevelDto.gameObjectSaveData.Add(SaveGameObject(track));
            }

            string directoryPath = $"{Application.persistentDataPath}/Levels/{_levelBaseInfo.levelName}";
            Directory.CreateDirectory(directoryPath);

            string filePath = $"{directoryPath}/LevelObjects.json";
            string json = JsonConvert.SerializeObject(saveLevelDto, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private void Load()
        {
            string path = $"{Application.persistentDataPath}/Levels/{_levelBaseInfo.levelName}/LevelObjects.json";
            if (!File.Exists(path)) return;

            string json = File.ReadAllText(path);
            var saveLevelDto = JsonConvert.DeserializeObject<SaveLevelDTO>(json);

            // 1. Загружаем корневые НЕ-группы (обычные объекты) в правильном порядке
            var sortedRootObjects = TopologicalSort(saveLevelDto.gameObjectSaveData);
            foreach (var saveData in sortedRootObjects)
            {
                trackObjectSpawner.LoadTrackObject(saveData);
            }

            // 2. Загружаем корневые ГРУППЫ в правильном порядке
            var rootGroupsAsBase = saveLevelDto.groupGameObjectSaveData.Cast<GameObjectSaveData>().ToList();
            var sortedRootGroups = TopologicalSort(rootGroupsAsBase);
            
            foreach (var groupBase in sortedRootGroups)
            {
                var group = (GroupGameObjectSaveData)groupBase;
                LoadGroup(group); // ← рекурсивная загрузка с сортировкой детей
            }
        }

        #endregion

        // === Рекурсивная загрузка группы с сортировкой детей ===
        private void LoadGroup(GroupGameObjectSaveData groupData)
        {
            // Сначала создаём саму группу
            trackObjectSpawner.LoadTrackObject(groupData);

            // Затем сортируем и загружаем её детей (могут быть и объекты, и подгруппы)
            var sortedChildren = TopologicalSort(groupData.children);
            foreach (var child in sortedChildren)
            {
                if (child is GroupGameObjectSaveData childGroup)
                {
                    LoadGroup(childGroup); // рекурсия
                }
                else
                {
                    trackObjectSpawner.LoadTrackObject(child);
                }
            }
        }

        // === УНИВЕРСАЛЬНАЯ топологическая сортировка для любого List<GameObjectSaveData> ===
        private static List<GameObjectSaveData> TopologicalSort(List<GameObjectSaveData> objects)
        {
            if (objects == null || objects.Count == 0)
                return new List<GameObjectSaveData>();

            var idToData = objects.ToDictionary(o => o.sceneObjectID, o => o);
            var childrenMap = new Dictionary<string, List<string>>();
            var allIds = new HashSet<string>(idToData.Keys);

            // Строим связи: parent → children
            foreach (var obj in objects)
            {
                if (!string.IsNullOrEmpty(obj.parentObjectID))
                {
                    if (allIds.Contains(obj.parentObjectID))
                    {
                        if (!childrenMap.ContainsKey(obj.parentObjectID))
                            childrenMap[obj.parentObjectID] = new List<string>();
                        childrenMap[obj.parentObjectID].Add(obj.sceneObjectID);
                    }
                    // Если родителя нет — объект остаётся корневым (игнорируем parentObjectID)
                }
            }

            // Находим корни: у кого parentObjectID пуст ИЛИ родитель отсутствует
            var rootIds = objects
                .Where(o => string.IsNullOrEmpty(o.parentObjectID) || !allIds.Contains(o.parentObjectID))
                .Select(o => o.sceneObjectID)
                .ToList();

            var sorted = new List<GameObjectSaveData>();
            var visited = new HashSet<string>();

            foreach (var rootId in rootIds)
            {
                Visit(rootId, idToData, childrenMap, visited, sorted);
            }

            // Если что-то не вошло — добавляем в конец (защита от циклов)
            if (sorted.Count < objects.Count)
            {
                var missing = objects.Where(o => !sorted.Contains(o)).ToList();
                Debug.LogWarning($"TopologicalSort: {missing.Count} объектов не вошли в порядок (возможно, цикл). Добавляем в конец.");
                sorted.AddRange(missing);
            }

            return sorted;
        }

        private static void Visit(
            string id,
            Dictionary<string, GameObjectSaveData> idToData,
            Dictionary<string, List<string>> childrenMap,
            HashSet<string> visited,
            List<GameObjectSaveData> result)
        {
            if (visited.Contains(id)) return;
            visited.Add(id);

            // 1. Добавляем текущий объект СРАЗУ
            if (idToData.TryGetValue(id, out var data))
            {
                result.Add(data);
            }

            // 2. Рекурсивно обходим детей
            if (childrenMap.TryGetValue(id, out var children))
            {
                foreach (var childId in children)
                {
                    Visit(childId, idToData, childrenMap, visited, result);
                }
            }
        }

        // === Save Logic (без изменений) ===

        private GameObjectSaveData SaveGameObject(TrackObjectData trackObject)
        {
            string parentObjectID = string.Empty;
            if (trackObject.sceneObject.transform.parent != _mainObjects.SceneObjectParent.transform)
                parentObjectID = trackObjectStorage
                    .GetTrackObjectData(trackObject.sceneObject.transform.parent.gameObject).sceneObjectID;

            var saveData = new GameObjectSaveData
            {
                gameObjectName = trackObject.sceneObject.name,
                startTime = trackObject.trackObject.StartTimeInTicks,
                duractionTime = trackObject.trackObject.TimeDuractionInTicks,
                sceneObjectID = trackObject.sceneObjectID,
                parentObjectID = parentObjectID,
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

        private GroupGameObjectSaveData SaveGroup(TrackObjectGroup group)
        {
            var baseData = SaveGameObject(group);
            var groupData = new GroupGameObjectSaveData
            {
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
                    groupData.children.Add(SaveGroup(childGroup));
                }
                else
                {
                    groupData.children.Add(SaveGameObject(child));
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
                    branchPath = $"{node.Path}/{node.Name}",
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
    }

    [System.Serializable]
    public class GameObjectSaveData
    {
        public string sceneObjectID;
        public string parentObjectID;
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
        // Может содержать как обычные объекты, так и другие группы
        public List<GameObjectSaveData> children = new();
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