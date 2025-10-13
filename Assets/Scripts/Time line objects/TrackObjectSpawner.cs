using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TimeLine.Components;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TrackObjectSpawner : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Transform root;
        [Space] [SerializeField] private GameObject scenePrefab;
        [SerializeField] private GameObject sceneObjectBasePrefab;
        [SerializeField] private GameObject trackPrefab;

        #endregion

        #region Private Fields

        private KeyframeTrackStorage _keyframeTrackStorage;
        private TrackObjectStorage _trackObjectStorage;
        private BranchCollection _branchCollection;
        private TrackStorage _trackStorage;
        private SelectObjectController _selectObjectController;
        private Main _main;
        private DiContainer _container;

        #endregion

        #region Injection

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            BranchCollection branchCollection,
            Main main,
            TrackStorage trackStorage,
            DiContainer container,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;
            _main = main;
            _trackStorage = trackStorage;
            _container = container;
            _keyframeTrackStorage = keyframeTrackStorage;
            _selectObjectController = selectObjectController;
        }

        #endregion

        #region Public Methods

        internal void Spawn(TrackObjectSO trackObjectSO)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            SceneTrackObject sceneTrackObject = CreateSceneTrackObject(trackObjectSO);

            // Создаем трек-объект
            TrackObject trackObject = CreateTrackObject(trackObjectSO, _trackStorage.TrackLines[0]);

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, trackObjectSO.name);

            // Добавляем в хранилище
            _trackObjectStorage.Add(sceneTrackObject.gameObject, trackObject, branch);
        }

        internal (TrackObjectData, GameObject, Branch) LoadTrackObject(GameObjectSaveData data)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            GameObject sceneObject = CreateBaseSceneTrackObject();

            // Создаем трек-объект
            TrackObject trackObject = CreateTrackObject(data.duractionTime, data.gameObjectName, _trackStorage.TrackLines[0], data.startTime);

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, data.gameObjectName);
            
            foreach (var node in data.branch.Nodes)
            {
                print($"Path:{node.Path} Name:{node.Name}");
                branch.AddNode(node.Path, node.Name);
            }
            
            // Добавляем в хранилище
            TrackObjectData trackObjectData = _trackObjectStorage.Add(sceneObject.gameObject, trackObject, branch);
            
            print(trackObjectData);
            
            //Добавляем необходимые компоненты
            foreach (var component in data.Components)
            {
                // print(ComponentRules.GetComponentType(component.ComponentType));
                IParameterComponent parameterComponent =
                    (IParameterComponent)ComponentRules.GetOrAddComponentSafely(component.ComponentType, sceneObject, _container);
                print(parameterComponent);
                parameterComponent.SetParameterData(component.Parameters);
            }

            // Добавляем трек и ключевые кадры
            foreach (var track in data.tracks)
            {
               
                Track trackm = new Track(sceneObject, track.branchPath, track.animationColor);
                _keyframeTrackStorage.AddTrack(branch.FindNode(track.branchPath), trackm, trackObjectData.trackObject);
                
                foreach (var saveData in track.keyframeSaveData)
                {
                    Keyframe.Keyframe keyframe = Keyframe.Keyframe.FromSaveData(saveData);
                    trackm.AddKeyframe(keyframe);
                }
            }
            
            return (trackObjectData, sceneObject, branch);
        }

        internal void LoadGroup(GroupGameObjectSaveData data)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();
            
            TrackObject trackObject = _container.InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform).GetComponent<TrackObject>();
            
            SceneTrackObject sceneTrackObject =
                _container.InstantiatePrefab(scenePrefab, root).GetComponent<SceneTrackObject>();
            sceneTrackObject.Setup(new TrackObjectSO()
            {
                name = "Group 1", 
                startLiveTime = (float)data.duractionTime,
            });
            
            
            List<TrackObjectData> trackObjectDatas = new List<TrackObjectData>();
             
            foreach (var childData in data.gameObjectSaveData)
            {
                TrackObjectData childTrackObject;
                GameObject childSceneObject;
                Branch childBranch;
                
                (childTrackObject, childSceneObject, childBranch) = LoadTrackObject(childData);
                
                trackObjectDatas.Add(childTrackObject);
                
                childSceneObject.transform.SetParent(sceneTrackObject.gameObject.transform);

                foreach (var node in childBranch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(trackObject);
                    }
                }
            }
            
            trackObject.Setup((float)data.duractionTime, "Group 1", _trackStorage.TrackLines[0], data.startTime, true);

            Branch branch = _branchCollection.AddBranch(data.branch.ID, data.branch.Name);
            
            _trackObjectStorage.AddGroup(sceneTrackObject.gameObject, trackObject, branch, trackObjectDatas);
        }

        #endregion

        #region Copy Methods

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                CopyObject(_selectObjectController.SelectObjects);
            }
        }

        /// <summary>
        /// Копирует список объектов трека
        /// </summary>
        internal List<TrackObjectData> CopyObject(List<TrackObjectData> list, bool isChildCopy = false)
        {
            List<TrackObjectData> result = new List<TrackObjectData>();
            var sortedList = list.OrderBy(x => x.trackObject.StartTimeInTicks).ToList();

            if (sortedList.Count == 0) return result;

            double minTimeOuter = sortedList[0].trackObject.StartTimeInTicks;
            double baseTime = _main.TicksCurrentTime();

            foreach (var trackObjectData in sortedList)
            {
                if (trackObjectData is TrackObjectGroup group)
                {
                    ProcessGroupCopy(trackObjectData, group, result, baseTime, minTimeOuter, isChildCopy);
                }
                else
                {
                    ProcessSingleObjectCopy(trackObjectData, result, baseTime, minTimeOuter);
                }
            }

            return result;
        }

        /// <summary>
        /// Копирует одиночный объект трека
        /// </summary>
        internal TrackObjectData CopyObject(TrackObjectData trackObjectData, bool addToStorage)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();
            double baseTime = _main.TicksCurrentTime();

            // Создаем копии объектов
            TrackObjectSO trackObjectSo = trackObjectData.sceneObject.GetComponent<SceneTrackObject>().Copy();
            SceneTrackObject sceneTrackObject = CreateSceneTrackObject(trackObjectSo);

            // Копируем компоненты
            CopyComponents(trackObjectData.sceneObject, sceneTrackObject.gameObject);

            // Создаем трек-объект
            TrackObject trackObject = CreateTrackObject(
                new TrackObjectSO()
                {
                    name = trackObjectData.trackObject.Name,
                    startLiveTime = (float)trackObjectData.trackObject.BeatDuraction
                },
                _trackStorage.TrackLines[0],
                baseTime
            );

            // Копируем ветку и треки
            Branch branch = CopyBranchWithTracks(trackObjectData.branch, id, sceneTrackObject.gameObject, trackObject);

            if (addToStorage)
                return _trackObjectStorage.Add(sceneTrackObject.gameObject, trackObject, branch);
            else
                return new TrackObjectData(sceneTrackObject.gameObject, trackObject, branch);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Получает все компоненты, реализующие ICopyableComponent
        /// </summary>
        public static List<ICopyableComponent> GetAllCopyableComponents(GameObject gameObject)
        {
            var result = new List<ICopyableComponent>();
            var allComponents = gameObject.GetComponents<Component>();

            foreach (var component in allComponents)
            {
                if (component is ICopyableComponent copyable)
                {
                    result.Add(copyable);
                }
            }

            print($"ICopyableComponent {result.Count}");
            return result;
        }

        /// <summary>
        /// Создает сценный объект трека
        /// </summary>
        private SceneTrackObject CreateSceneTrackObject(TrackObjectSO trackObjectSO)
        {
            SceneTrackObject sceneTrackObject = _container
                .InstantiatePrefab(scenePrefab, root)
                .GetComponent<SceneTrackObject>();
            sceneTrackObject.Setup(trackObjectSO);
            return sceneTrackObject;
        }

        private GameObject CreateSceneTrackObject()
        {
            GameObject sceneTrackObject = _container.InstantiatePrefab(scenePrefab, root);
            return sceneTrackObject;
        }
        
        private GameObject CreateBaseSceneTrackObject()
        {
            GameObject sceneTrackObject = _container.InstantiatePrefab(sceneObjectBasePrefab, root);
            return sceneTrackObject;
        }

        /// <summary>
        /// Создает объект трека
        /// </summary>
        private TrackObject CreateTrackObject(TrackObjectSO trackObjectSO, TrackLine trackLine, double startTime = -1)
        {
            TrackObject trackObject = _container
                .InstantiatePrefab(trackPrefab, trackLine.RectTransform)
                .GetComponent<TrackObject>();

            double actualStartTime = startTime >= 0 ? startTime : _main.TicksCurrentTime();
            trackObject.Setup(trackObjectSO, trackLine, actualStartTime);

            return trackObject;
        }
        
        /// <summary>
        /// Создает объект трека
        /// </summary>
        private TrackObject CreateTrackObject(double ticksLifeTime, string name, TrackLine trackLine, double startTime = -1)
        {
            TrackObject trackObject = _container
                .InstantiatePrefab(trackPrefab, trackLine.RectTransform)
                .GetComponent<TrackObject>();

            double actualStartTime = startTime >= 0 ? startTime : _main.TicksCurrentTime();
            trackObject.Setup(ticksLifeTime, name, trackLine, actualStartTime);

            return trackObject;
        }

        /// <summary>
        /// Копирует компоненты с исходного объекта на целевой
        /// </summary>
        private void CopyComponents(GameObject source, GameObject target)
        {
            List<ICopyableComponent> sourceComponents = GetAllCopyableComponents(source);
            foreach (var component in sourceComponents)
            {
                component.Copy(target);
            }
        }

        /// <summary>
        /// Копирует ветку с треками
        /// </summary>
        private Branch CopyBranchWithTracks(Branch sourceBranch, string newId, GameObject sceneObject,
            TrackObject trackObject)
        {
            Branch branch = _branchCollection.CopyBranch(sourceBranch, newId);

            foreach (var node in sourceBranch.Nodes)
            {
                var track = _keyframeTrackStorage.GetTrack(node);
                string[] split = node.Path.Split('/');

                // TreeNode newNode = split.Length > 1
                //     ? branch.AddNode(split[0], split[1])
                //     : branch.AddNode("", split[0]);


                TreeNode newNode = null;
                if (split.Length > 1)
                    newNode = branch.AddNode(split[0], split[1]);

                if (track != null && newNode != null)
                    _keyframeTrackStorage.AddTrack(newNode, track.Copy(sceneObject), trackObject);
            }

            return branch;
        }

        #endregion

        #region Private Processing Methods

        /// <summary>
        /// Обрабатывает копирование группы объектов
        /// </summary>
        private void ProcessGroupCopy(TrackObjectData trackObjectData, TrackObjectGroup group,
            List<TrackObjectData> result, double baseTime, double minTimeOuter, bool isChildCopy)
        {
            // Рекурсивно копируем дочерние элементы
            List<TrackObjectData> childs = CopyObject(group.TrackObjectDatas, true);

            // Копируем родительский объект
            TrackObjectData parent = CopyObject(trackObjectData, false);

            // Вычисляем временные границы дочерних элементов
            double minTime = childs.Min(x => x.trackObject.StartTimeInTicks);
            double maxTime = childs.Max(x => x.trackObject.StartTimeInTicks + x.trackObject.TimeDuractionInTicks);
            double position = baseTime + (trackObjectData.trackObject.StartTimeInTicks - minTimeOuter);

            // Настраиваем группу
            parent.trackObject.Setup(maxTime - minTime, "Group", _trackStorage.TrackLines[0], position, true);

            // Обновляем позиции дочерних элементов
            foreach (var child in childs)
            {
                child.trackObject.GroupOffset(minTime);
                child.trackObject.Hide();
                child.sceneObject.transform.SetParent(parent.sceneObject.transform);

                // Обновляем родительские связи треков
                foreach (var node in child.branch.Nodes)
                {
                    foreach (var childNode in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(childNode)?.SetParent(parent.trackObject);
                    }
                }
            }

            // Добавляем группу в хранилище или создаем новую группу
            if (!isChildCopy)
            {
                _trackObjectStorage.AddGroup(parent.sceneObject.gameObject, parent.trackObject, parent.branch, childs);
                result.Add(parent);
            }
            else
            {
                var groupResult = new TrackObjectGroup(parent.sceneObject.gameObject, parent.trackObject, parent.branch,
                    childs);
                result.Add(groupResult);
            }
        }

        /// <summary>
        /// Обрабатывает копирование одиночного объекта
        /// </summary>
        private void ProcessSingleObjectCopy(TrackObjectData trackObjectData, List<TrackObjectData> result,
            double baseTime, double minTimeOuter)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Копируем данные сцены
            TrackObjectSO trackObjectSo = trackObjectData.sceneObject.GetComponent<SceneTrackObject>().Copy();
            SceneTrackObject sceneTrackObject = CreateSceneTrackObject(trackObjectSo);

            // Копируем компоненты
            CopyComponents(trackObjectData.sceneObject, sceneTrackObject.gameObject);

            // Создаем трек-объект с позицией относительно базового времени
            double position = baseTime + (trackObjectData.trackObject.StartTimeInTicks - minTimeOuter);
            TrackObject trackObject = CreateTrackObject(
                new TrackObjectSO()
                {
                    name = trackObjectData.trackObject.Name,
                    startLiveTime = (float)trackObjectData.trackObject.BeatDuraction
                },
                _trackStorage.TrackLines[0],
                position
            );

            // Копируем ветку и треки
            Branch branch = CopyBranchWithTracks(trackObjectData.branch, id, sceneTrackObject.gameObject, trackObject);

            result.Add(_trackObjectStorage.Add(sceneTrackObject.gameObject, trackObject, branch));
        }

        #endregion
    }
}