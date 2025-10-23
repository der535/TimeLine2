using System;
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
        [SerializeField] private SaveComposition saveComposition;

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

        internal void Spawn(Sprite sprite)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            GameObject sceneObject = CreateSceneTrackObject(sprite);

            // Создаем трек-объект
            TrackObject trackObject = CreateTrackObject(100, "Object", _trackStorage.TrackLines[0]);

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, sceneObject.name);

            // Добавляем в хранилище
            _trackObjectStorage.Add(sceneObject, trackObject, branch, Guid.NewGuid().ToString());
        }

        internal (TrackObjectData, GameObject, Branch) LoadTrackObject(GameObjectSaveData data,
            bool addToStorage = true)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            GameObject sceneObject = CreateBaseSceneTrackObject();


            if (!string.IsNullOrEmpty(data.parentObjectID))
            {
                print(data.parentObjectID);
                print(_trackObjectStorage.GetTrackObjectDataBySceneObjectID(data.parentObjectID).sceneObject.transform);
                sceneObject.transform.parent = _trackObjectStorage
                    .GetTrackObjectDataBySceneObjectID(data.parentObjectID).sceneObject.transform;
            }

            // Создаем трек-объект
            TrackObject trackObject = CreateTrackObject(data.duractionTime, data.gameObjectName,
                _trackStorage.TrackLines[0], data.startTime);

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, data.gameObjectName);

            foreach (var node in data.branch.Nodes)
            {
                print($"Path:{node.Path} Name:{node.Name}");
                branch.AddNode(node.Path, node.Name);
            }

            TrackObjectData trackObjectData;

            if (addToStorage)
            {
                // Добавляем в хранилище
                trackObjectData =
                    _trackObjectStorage.Add(sceneObject.gameObject, trackObject, branch, data.sceneObjectID);
            }
            else
            {
                trackObjectData = new TrackObjectData(sceneObject.gameObject, trackObject, branch, data.sceneObjectID);
            }


            // print(trackObjectData);

            //Добавляем необходимые компоненты
            foreach (var component in data.Components)
            {
                // print(ComponentRules.GetComponentType(component.ComponentType));
                IParameterComponent parameterComponent =
                    (IParameterComponent)ComponentRules.GetOrAddComponentSafely(component.ComponentType, sceneObject,
                        _container);
                // print(parameterComponent);
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

        internal (TrackObjectData, GameObject, Branch) LoadGroup(GroupGameObjectSaveData data, string compositionID,
            GroupGameObjectSaveData compositionData = null, bool addToStorage = true)
        {
            string sceneObjectID = UniqueIDGenerator.GenerateUniqueID();

            TrackObject trackObject = _container
                .InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform).GetComponent<TrackObject>();

            GameObject sceneTrackObject =
                _container.InstantiatePrefab(scenePrefab, root);

            List<TrackObjectData> trackObjectDatas = new List<TrackObjectData>();

            // print(compositionData);
            List<GameObjectSaveData> children;
            if (compositionData == null)
                children = data.children;
            else
                children = compositionData.children;

            foreach (var childData in children.ToList())
            {
                TrackObjectData childTrackObject = null;
                GameObject childSceneObject = null;
                Branch childBranch = null;

                if (childData is GroupGameObjectSaveData childGroupData)
                {
                    GroupGameObjectSaveData groupChildData =  saveComposition.FindCompositionDataById(childGroupData
                        .compositionID);

                    if (groupChildData != null)
                    {
                        (childTrackObject, childSceneObject, childBranch) = LoadGroup(childGroupData,
                            childGroupData.compositionID,
                            groupChildData);
                    }
                    
                    
                }
                else
                {
                    (childTrackObject, childSceneObject, childBranch) = LoadTrackObject(childData);

                }

                if (childTrackObject != null)
                {
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

  
            }

            // print(compositionData);
            if (compositionData == null)
            {
                trackObject.Setup((float)data.duractionTime, data.gameObjectName, _trackStorage.TrackLines[0],
                    data.startTime, true);
            }
            else
            {
                trackObject.Setup((float)data.duractionTime, compositionData.gameObjectName, _trackStorage.TrackLines[0],
                    data.startTime, true);
                trackObject.UpdateDuraction(compositionData.duractionTime);
            }
                
//
            Branch branch;
//            
            // print(compositionData);
            // print(_branchCollection);
            // print(data.branch.ID);
            // print(data.branch.Name);
            
            if(compositionData == null)
                branch = _branchCollection.AddBranch(data.branch.ID, data.branch.Name);
            else
                branch = _branchCollection.AddBranch(compositionData.branch.ID, compositionData.branch.Name);

            TrackObjectGroup trackObjectGroup =
                _trackObjectStorage.AddGroup(sceneTrackObject.gameObject, trackObject, branch, trackObjectDatas,
                    sceneObjectID, compositionID, addToStorage); // todo тут убрать добовлении композиции
            return (trackObjectGroup, sceneTrackObject, branch);
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
                    ProcessGroupCopy(trackObjectData, group, result, baseTime, minTimeOuter, isChildCopy,
                        group.compositionID);
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
            GameObject sceneTrackObject = CreateSceneTrackObject();

            // Копируем компоненты
            CopyComponents(trackObjectData.sceneObject, sceneTrackObject.gameObject);

            // Создаем трек-объект
            TrackObject trackObject = CreateTrackObject(trackObjectData.trackObject.TimeDuractionInTicks,
                trackObjectData.sceneObject.name,
                _trackStorage.TrackLines[0],
                baseTime
            );

            // Копируем ветку и треки
            Branch branch = CopyBranchWithTracks(trackObjectData.branch, id, sceneTrackObject.gameObject, trackObject);

            if (addToStorage)
                return _trackObjectStorage.Add(sceneTrackObject.gameObject, trackObject, branch,
                    Guid.NewGuid().ToString());
            else
                return new TrackObjectData(sceneTrackObject.gameObject, trackObject, branch, Guid.NewGuid().ToString());
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

            return result;
        }

        /// <summary>
        /// Создает сценный объект трека
        /// </summary>
        private GameObject CreateSceneTrackObject(Sprite sprite = null)
        {
            GameObject sceneTrackObject = _container
                .InstantiatePrefab(scenePrefab, root);

            if (sprite != null)
            {
                SpriteRendererComponent spriteRendererComponent =
                    sceneTrackObject.AddComponent<SpriteRendererComponent>();
                _container.Inject(spriteRendererComponent);
                spriteRendererComponent.Sprite.Value = sprite;
            }

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
        private TrackObject CreateTrackObject(double ticksLifeTime, string name, TrackLine trackLine,
            double startTime = -1)
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
                print(node.Name);
                print(track?.TrackName);
                print(track?.Keyframes.Count);


                string[] split = $"{node.Path}/{node.Name}".Split('/');

                TreeNode newNode = null;
                if (split.Length > 1)
                    newNode = branch.AddNode(split[0], split[1]);

                print(track != null);
                print(track);
                print(newNode != null);
                print(newNode);
                if (track != null && newNode != null)
                {
                    print("AddTrack");
                    _keyframeTrackStorage.AddTrack(newNode, track.Copy(sceneObject), trackObject);
                }
            }

            return branch;
        }

        #endregion

        #region Private Processing Methods

        /// <summary>
        /// Обрабатывает копирование группы объектов
        /// </summary>
        private void ProcessGroupCopy(TrackObjectData trackObjectData, TrackObjectGroup group,
            List<TrackObjectData> result, double baseTime, double minTimeOuter, bool isChildCopy, string compositionID)
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

                Vector3 childPosition = child.sceneObject.transform.localPosition;
                Quaternion childRotation = child.sceneObject.transform.localRotation;
                Vector3 childScale = child.sceneObject.transform.localScale;

                child.sceneObject.transform.SetParent(parent.sceneObject.transform);

                child.sceneObject.transform.localPosition = childPosition;
                child.sceneObject.transform.localRotation = childRotation;
                child.sceneObject.transform.localScale = childScale;

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
                _trackObjectStorage.AddGroup(parent.sceneObject.gameObject, parent.trackObject, parent.branch, childs,
                    UniqueIDGenerator.GenerateUniqueID(), compositionID);
                result.Add(parent);
            }
            else
            {
                var groupResult = new TrackObjectGroup(parent.sceneObject.gameObject, parent.trackObject, parent.branch,
                    Guid.NewGuid().ToString(), childs, group.compositionID);
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
            // TrackObjectSO trackObjectSo = trackObjectData.sceneObject.GetComponent<SceneTrackObject>().Copy();
            GameObject sceneTrackObject = CreateSceneTrackObject();

            // Копируем компоненты
            CopyComponents(trackObjectData.sceneObject, sceneTrackObject.gameObject);

            // Создаем трек-объект с позицией относительно базового времени
            double position = baseTime + (trackObjectData.trackObject.StartTimeInTicks - minTimeOuter);
            TrackObject trackObject = CreateTrackObject(trackObjectData.trackObject.TimeDuractionInTicks,
                trackObjectData.trackObject.Name,
                _trackStorage.TrackLines[0],
                position
            );

            // Копируем ветку и треки
            Branch branch = CopyBranchWithTracks(trackObjectData.branch, id, sceneTrackObject.gameObject, trackObject);

            result.Add(_trackObjectStorage.Add(sceneTrackObject.gameObject, trackObject, branch,
                Guid.NewGuid().ToString()));
        }

        #endregion
    }
}