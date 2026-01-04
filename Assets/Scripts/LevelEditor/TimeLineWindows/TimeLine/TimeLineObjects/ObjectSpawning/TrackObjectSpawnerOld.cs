using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json;
using TimeLine.Components;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TrackObjectSpawnerOld : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [Space] [SerializeField] private GameObject scenePrefab;
        [SerializeField] private GameObject sceneObjectBasePrefab;
        [SerializeField] private GameObject trackPrefab;
        [SerializeField] private SaveComposition saveComposition;
        [SerializeField] private SaveLevel _saveLevel;
        [SerializeField] private WindowsFocus _windowsFocus;
        [SerializeField] private EditingCompositionController _controller;


        private KeyframeTrackStorage _keyframeTrackStorage;
        private TrackObjectStorage _trackObjectStorage;
        private BranchCollection _branchCollection;
        private TrackStorage _trackStorage;
        private SelectObjectController _selectObjectController;
        private Main _main;
        private DiContainer _container;
        private ActionMap _actionMap;


        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            BranchCollection branchCollection,
            Main main,
            TrackStorage trackStorage,
            DiContainer container,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController,
            ActionMap actionMap)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;
            _main = main;
            _trackStorage = trackStorage;
            _container = container;
            _keyframeTrackStorage = keyframeTrackStorage;
            _selectObjectController = selectObjectController;
            _actionMap = actionMap;
        }


        private void Start()
        {
            // _actionMap.Editor.Space.started += _ => CopyObjectNew(_selectObjectController.SelectObjects);
            _actionMap.Editor.C.started += _ =>
            {
                if (_actionMap.Editor.LeftCtrl.IsPressed() && _windowsFocus.IsFocused)
                    CopyNew(_selectObjectController.SelectObjects); //Todo потом сделать по нормальному
            };
            _actionMap.Editor.V.started += _ =>
            {
                if (_actionMap.Editor.LeftCtrl.IsPressed() && _windowsFocus.IsFocused)
                {
                    if (PasteValidCheck(_controller.EditionCompositionID))
                        PasteNew();
                }
            };
        }

        #region Public Methods

        internal void Spawn(Sprite sprite)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            GameObject sceneObject = CreateSceneTrackObject(sprite);

            sceneObject.name = sprite.name;

            // Создаем трек-объект
            TrackObject trackObject = CreateTrackObject(100, sprite.name, _trackStorage.GetTrackLineByIndex(0));

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, sprite.name);

            // Добавляем в хранилище
            _trackObjectStorage.Add(sceneObject, trackObject, branch, Guid.NewGuid().ToString());

            sceneObject.GetComponent<NameComponent>().Name.Value = sprite.name;
        }

        public (TrackObjectData, GameObject, Branch) LoadTrackObject(GameObjectSaveData data,
            bool addToStorage = true)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            GameObject sceneObject = CreateBaseSceneTrackObject();
            

            // Создаем трек-объект
            TrackObject trackObject;
            trackObject = CreateTrackObject(data.duractionTime, data.gameObjectName,
                _trackStorage.GetTrackLineByIndex(data.lineIndex), data.startTime);


            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, data.branch.Name);
            

            // Добавляем ноды в ветку
            foreach (var node in data.branch.Nodes)
            {
                print(node.Path);
                branch.AddNode(node.Path);
            }

            TrackObjectData trackObjectData;

            //Услови где мы добовляем в хранилище трекобжект или нет.
            //Не добовление может использоваться только в случае если мы сохдаём трек обжект который будет внутри группы
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

            //Добавляем необходимые компоненты
            foreach (var component in data.Components)
            {
                IParameterComponent parameterComponent =
                    (IParameterComponent)ComponentRules.GetOrAddComponentSafely(component.ComponentType, sceneObject,
                        _container);
                parameterComponent.SetParameterData(component.Parameters);
            }

            // Добавляем трек и ключевые кадры
            foreach (var track in data.tracks)
            {
                Track trackm = new Track(sceneObject, track.branchPath, track.animationColor);

                TreeNode node = branch.FindNode(track.branchPath);
                
                _keyframeTrackStorage.AddTrack(node, trackm, trackObjectData.trackObject);

                foreach (var saveData in track.keyframeSaveData)
                {
                    Keyframe.Keyframe keyframe = Keyframe.Keyframe.FromSaveData(saveData);
                    trackm.AddKeyframe(keyframe);
                }
            }
            
            sceneObject.GetComponent<NameComponent>().Name.Value = branch.Name;

            trackObjectData.trackObject._parentID = data.parentObjectID;

            return (trackObjectData, sceneObject, branch);
        }

        internal (TrackObjectData, GameObject, Branch) LoadGroupNew(GroupGameObjectSaveData data, string compositionID,
            GroupGameObjectSaveData compositionData = null, bool addToStorage = true, string lastEditID = null)
        {
            string sceneObjectID = UniqueIDGenerator.GenerateUniqueID();

            var (groupTrackObject, groupGameObject, groupBranch) = LoadTrackObject(data, false);


            _main.SetTimeInTicks(groupTrackObject.trackObject.StartTimeInTicks);


            List<TrackObjectData> trackObjectDatas = new List<TrackObjectData>();

            List<GameObjectSaveData> children;
            if (compositionData == null)
                children = data.children;
            else
            {
                children = compositionData.children;
            }

            foreach (var childData in children.ToList())
            {
                TrackObjectData childTrackObject = null;
                GameObject childSceneObject = null;
                Branch childBranch = null;

                if (childData is GroupGameObjectSaveData childGroupData)
                {
                   // print(childGroupData.gameObjectName); 
                   // if(lastEditID!=null)childGroupData.lastEditID = lastEditID;
                   // print(childGroupData.lastEditID); 
                    GroupGameObjectSaveData groupChildData = saveComposition.FindCompositionDataById(childGroupData
                        .compositionID);

                    if (groupChildData != null)
                    {
                        (childTrackObject, childSceneObject, childBranch) = LoadGroupNew(childGroupData,
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

                    Vector3 pos = childSceneObject.transform.localPosition;
                    Quaternion rot = childSceneObject.transform.localRotation;
                    Vector3 sca = childSceneObject.transform.localScale;
                    childSceneObject.transform.SetParent(groupGameObject.gameObject.transform);
                    childSceneObject.transform.localPosition = pos;
                    childSceneObject.transform.localRotation = rot;
                    childSceneObject.transform.localScale = sca;

                    foreach (var node in childBranch.Nodes)
                    {
                        foreach (var node2 in node.Children)
                        {
                            _keyframeTrackStorage.GetTrack(node2)?.SetParent(groupTrackObject.trackObject);
                        }
                    }
                }
            }

            if (compositionData == null)
            {
                groupTrackObject.trackObject.Setup((float)data.duractionTime, data.gameObjectName,
                    _trackStorage.GetTrackLineByIndex(data.lineIndex),
                    data.parentObjectID,data.startTime, true);
            }
            else
            {
                groupTrackObject.trackObject.Setup((float)data.duractionTime, compositionData.gameObjectName,
                    _trackStorage.GetTrackLineByIndex(data.lineIndex),
                    data.parentObjectID,data.startTime, true);
                groupTrackObject.trackObject.UpdateDuraction(compositionData.duractionTime);
            }

            TrackObjectGroup trackObjectGroup =
                _trackObjectStorage.AddGroup(groupGameObject.gameObject, groupTrackObject.trackObject,
                    groupTrackObject.branch, trackObjectDatas,
                    sceneObjectID, compositionID, data.lastEditID, addToStorage); // ???????

            groupGameObject.GetComponent<NameComponent>().Name.Value = groupTrackObject.branch.Name;

            
            var currentTime = _main.TicksCurrentTime();
            _main.SetTimeInTicks(currentTime);
            

            return (trackObjectGroup, groupGameObject, groupTrackObject.branch);
        }

        #endregion

        #region Copy Methods

        //Новая система копирования и вставки, сохраняем группу

        //-----------------------------------------------

        List<GameObjectSaveData> dataCopy = new List<GameObjectSaveData>();

        internal void CopyNew(List<TrackObjectData> selectedObjects)
        {
            dataCopy = new List<GameObjectSaveData>();
            foreach (var sObject in selectedObjects)
            {
                if (sObject is TrackObjectGroup group)
                {
                    print(group.lastEditID);
                    GroupGameObjectSaveData saveData = _saveLevel.FullSave(group);
                    print(JsonConvert.SerializeObject(saveData));
                    saveData.compositionID = group.compositionID;
                    dataCopy.Add(saveData);
                }
                else
                {
                    var data = _saveLevel.SaveGameObject(sObject, "");
                    dataCopy.Add(data);
                    print(JsonConvert.SerializeObject(data));
                }
            }
        }

        internal bool PasteValidCheck(string past)
        {
            foreach (var copyTrackObject in dataCopy)
            {
                if (copyTrackObject is GroupGameObjectSaveData copyGroup)
                {
                    if (copyGroup.compositionID == past) return false;
                    else if (PasteValidCheckGroup(copyGroup, past)) return false;
                }
            }

            return true;
        }

        internal bool PasteValidCheckGroup(GroupGameObjectSaveData copyGroup, string past)
        {
            foreach (var group in copyGroup.children)
            {
                if (group is GroupGameObjectSaveData groupGroup)
                {
                    if (groupGroup.compositionID == past) return true;
                    else if(PasteValidCheckGroup(groupGroup, past)) return true;
                }
            }

            return false;
        }

        internal double GetMinTime(List<GameObjectSaveData> list)
        {
            return list.Min(item => item.startTime);
        }

        internal void PasteNew()
        {
            if(dataCopy == null) return;
            var minTime = GetMinTime(dataCopy);
            foreach (var data in dataCopy)
            {
                data.startTime = data.startTime - minTime + _main.TicksCurrentTime();
                if (data is GroupGameObjectSaveData group)
                {
                    PasteGroup(group);
                }
                else
                {
                    LoadTrackObject(data);
                }
            }

            // dataCopy = new List<GameObjectSaveData>();
        }

        internal void PasteGroup(GroupGameObjectSaveData data, bool addToStorage = true)
        {
            foreach (var child in data.children)
            {
                if (child is GroupGameObjectSaveData childGroup)
                {
                    PasteGroup(childGroup, false);
                }
            }


            GroupGameObjectSaveData composition = saveComposition.FindCompositionDataById(data.compositionID);
            GroupGameObjectSaveData dataCopyComposition = data.DuplicateComposition();


            dataCopyComposition.startTime = 0;
            dataCopyComposition.branch.Nodes = new List<TreeNodeSaveData>();
            dataCopyComposition.tracks = new List<TrackSaveData>();
            dataCopyComposition.Components =
                new List<ComponentData>(); // В сохраняемую композицию запихать стандартные компонеты из префаба

            
            if (!string.IsNullOrEmpty(data.lastEditID) || !string.IsNullOrEmpty(composition.lastEditID))
            {
                if (data.lastEditID != composition.lastEditID)
                {
                    data.compositionID = Guid.NewGuid().ToString();
                    dataCopyComposition.compositionID = data.compositionID;
                    saveComposition.AddComposition(dataCopyComposition);
                }
            }
            


            if (addToStorage)
                LoadGroupNew(data, data.compositionID);
        }

        #endregion


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
            trackObject.Setup(ticksLifeTime, name, trackLine, string.Empty,actualStartTime);

            return trackObject;
        }
    }
}