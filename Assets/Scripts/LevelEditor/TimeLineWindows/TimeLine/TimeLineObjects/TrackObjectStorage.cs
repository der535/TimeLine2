using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class TrackObjectStorage : MonoBehaviour
    {
        private List<TrackObjectData> _trackObjects = new();
        private List<TrackObjectGroup> _trackObjectGroups = new();

        [FormerlySerializedAs("_selectedObject")]
        public TrackObjectData selectedObject;

        private GameEventBus _gameEventBus;
        private SelectObjectController _selectObjectController;
        private SaveComposition _composition;
        private ActionMap _actionMap;


        [Inject]
        private void Construct(GameEventBus gameEventBus, SelectObjectController selectObjectController,
            SaveComposition saveComposition, ActionMap actionMap, TrackObjectRemover trackObjectRemover, Main main)
        {
            _gameEventBus = gameEventBus;
            _selectObjectController = selectObjectController;
            _composition = saveComposition;
            _actionMap = actionMap;
        }

        public List<TrackObjectData> TrackObjects => _trackObjects;
        public List<TrackObjectGroup> TrackObjectGroups => _trackObjectGroups;

        private void Awake()
        {
            _gameEventBus.SubscribeTo<TickSmoothTimeEvent>(ActiveSceneObject);
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => DeselectAllObject());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                var trackObjectData = GetTrackObjectData(data.Tracks[^1].trackObject);
                if (trackObjectData != null)
                {
                    InternalSelectObject(trackObjectData);
                }

                foreach (var track in data.Tracks)
                {
                    SelectObject(track.trackObject);
                }
            });

            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => { DeselectObject(data.DeselectedObject); });
        }

        internal double GetMinTime()
        {
            var minFromObjects = _trackObjects.Select(f => f.trackObject.StartTimeInTicks);
            var minFromGroups = _trackObjectGroups.Select(f => f.trackObject.StartTimeInTicks);
            var allTimes = minFromObjects.Concat(minFromGroups);

            var enumerable = allTimes.ToList();
            return enumerable.Any() ? enumerable.Min() : 0.0;
        }

        internal List<TrackObjectData> GetAllActiveTrackData()
        {
            List<TrackObjectData> trackObjectData = new List<TrackObjectData>();
            foreach (var track in _trackObjects)
            {
                if (track.trackObject.GetActive()) trackObjectData.Add(track);
            }

            foreach (var group in _trackObjectGroups)
            {
                if (group.trackObject.GetActive()) trackObjectData.Add(group);
            }

            return trackObjectData;
        }

        internal List<TrackObjectData> GetAllActiveSceneObjects()
        {
            List<TrackObjectData> trackObjectData = new List<TrackObjectData>();
            foreach (var track in _trackObjects)
            {
                if (track.sceneObject.activeSelf) trackObjectData.Add(track);
            }

            foreach (var group in _trackObjectGroups)
            {
                if (group.sceneObject.activeSelf) trackObjectData.Add(group);
            }

            return trackObjectData;
        }


        private void ActiveSceneObject(ref TickSmoothTimeEvent smoothTimeEvent)
        {
            foreach (var trackObject in _trackObjects.ToList())
            {
                CheckActiveTrackObjects(trackObject, smoothTimeEvent.Time);
            }

            foreach (var group in _trackObjectGroups.ToList())
            {
                CheckActiveGroup(group, smoothTimeEvent.Time);
            }
        }

        internal TrackObjectData FindObjectByID(string id)
        {
            foreach (var objectData in _trackObjects)
            {
                if (objectData.sceneObjectID == id)
                {
                    return objectData;
                }
            }

            foreach (var objectData in _trackObjectGroups)
            {
                if (objectData.sceneObjectID == id)
                {
                    return objectData;
                }
            }

            Debug.LogWarning($"Не найден объект {id}");
            return null;
        }

        internal void CheckActiveTrackSingle(TrackObjectData trackObject)
        {
            if (trackObject is TrackObjectGroup group)
            {
                CheckActiveGroup(group, TimeLineConverter.Instance.TicksCurrentTime());
            }
            else
            {
                CheckActiveTrackObjects(trackObject, TimeLineConverter.Instance.TicksCurrentTime());
            }
        }

        private void CheckActiveTrackObjects(TrackObjectData trackObject, double time)
        {
            if (trackObject.trackObject.gameObject.activeSelf)
            {
                bool shouldBeActive = trackObject.trackObject.StartTimeInTicks <= time &&
                                      trackObject.trackObject.TimeDuractionInTicks +
                                      trackObject.trackObject.StartTimeInTicks > time;

                trackObject.activeObjectController?.Turn(trackObject.trackObject.isActive && shouldBeActive);
            }
            else
            {
                trackObject.activeObjectController?.Turn(false);
            }


            //REMOVE TEMP OBJECTS
            if (trackObject.trackObject.isTemp &&
                time > trackObject.trackObject.TimeDuractionInTicks +
                trackObject.trackObject.StartTimeInTicks)
            {
                trackObject.trackObject.isActive = false;
            }
        }

        private void CheckActiveGroup(TrackObjectGroup group, double time, bool enchanted = false,
            bool activeGroup = true)
        {
            if ((!enchanted && !group.trackObject.gameObject.activeSelf) || activeGroup == false)
            {
                group.activeObjectController?.Turn(false);

                foreach (var trackObject in group.TrackObjectDatas)
                {
                    trackObject.activeObjectController?.Turn(false);
                }

                return;
            }

            bool targetState = group.trackObject.isActive;
            group.activeObjectController?.Turn(targetState);
            


            double groupStart = group.trackObject.StartTimeInTicks;
            double groupEnd = groupStart + group.trackObject.TimeDuractionInTicks;
            bool isGroupActive = time >= groupStart && time < groupEnd;
            print(isGroupActive);


            foreach (var trackObject in group.TrackObjectDatas)
            {
                if (!isGroupActive)
                {
                    trackObject.activeObjectController.Turn(false);
                }

                if (trackObject is TrackObjectGroup nestedGroup)
                {
                    CheckActiveGroup(nestedGroup, time - groupStart, true, isGroupActive);
                    continue;
                }

                double objStart = trackObject.trackObject.StartTimeInTicks + groupStart;
                double objEnd = objStart + trackObject.trackObject.TimeDuractionInTicks;
                bool isObjectActive = time >= objStart && time < objEnd;

                print(time);
                print(trackObject.trackObject.StartTimeInTicks);
                print(trackObject.trackObject.StartTimeInTicks + groupStart);
                print(objStart);
                print(objEnd);

                bool finalState = group.trackObject.isActive && isObjectActive && isGroupActive; 

                trackObject.activeObjectController?.Turn(finalState);
            }

            // REMOVE TEMP OBJECTS
            if (group.trackObject.isTemp &&
                time > group.trackObject.StartTimeInTicks + group.trackObject.TimeDuractionInTicks)
            {
                group.trackObject.isActive = false;
            }
        }

        internal TrackObjectData Add(GameObject sceneObject, TrackObject selectedObject, Branch branch, string id)
        {
            TrackObjectData trackObjectData = new TrackObjectData(sceneObject, selectedObject, branch, id);
            _gameEventBus.Raise(new AddTrackObjectDataEvent(trackObjectData));
            _trackObjects.Add(trackObjectData);
            sceneObject.GetComponent<SceneObjectLink>().trackObjectData = trackObjectData;

            //Debug.Log($"[Add] TrackObject '{selectedObject.Name}' added to storage.");
            return trackObjectData;
        }

        internal TrackObjectGroup AddGroup(GameObject sceneObject, TrackObject trackObject, Branch branch,
            List<TrackObjectData> trackObjectDatas, string sceneObjectID, string compositionID, string lastEditID,
            bool addToStorage = true)
        {
            var objectsForGroup = new List<TrackObjectData>(trackObjectDatas);

            foreach (var trackObjectData in objectsForGroup)
            {
                if (trackObjectData is TrackObjectGroup group2)
                    _trackObjectGroups.Remove(group2);
                else
                    _trackObjects.Remove(trackObjectData);

                trackObjectData.trackObject.Hide();
            }

            if (compositionID == "")
                compositionID = Guid.NewGuid().ToString();
            TrackObjectGroup group =
                new TrackObjectGroup(sceneObject, trackObject, branch, sceneObjectID, objectsForGroup, compositionID,
                    lastEditID)
                {
                    compositionID = compositionID
                };

            // print(_composition);
            if (addToStorage)
                _trackObjectGroups.Add(group);

            // Подписка на изменение размера
            foreach (var track in trackObjectDatas)
            {
                track.trackObject.GroupOffsetTrack(trackObject);
                trackObject.Rezise += (value) => { track.trackObject.GroupOffset(value); };
            }

            _composition.AddComposition(group);

            return group;
        }

        internal void HideAll()
        {
            foreach (var trackObject in _trackObjects)
            {
                trackObject.trackObject.Hide();
            }

            foreach (var group in _trackObjectGroups)
            {
                group.trackObject.Hide();
            }
            //Debug.Log("[HideAll] All track objects and groups hidden.");
        }

        internal void ShowAll()
        {
            foreach (var trackObject in _trackObjects)
            {
                trackObject.trackObject.Show();
            }

            foreach (var group in _trackObjectGroups)
            {
                group.trackObject.Show();
            }
            //Debug.Log("[ShowAll] All track objects and groups shown.");
        }

        internal void SeparetaGroup(TrackObjectGroup group)
        {
            foreach (var trackData in group.TrackObjectDatas)
            {
                if (trackData is TrackObjectGroup nestedGroup)
                    _trackObjectGroups.Add(nestedGroup);
                else
                    _trackObjects.Add(trackData);
            }
            //print(group.branch.ID);
            //print(_trackObjectGroups.Remove(group));
            //print(_trackObjectGroups.Count);
            //Debug.Log($"[SeparetaGroup] Group '{group.trackObject.Name}' separated into {group.TrackObjectDatas.Count} individual objects.");
        }

        internal void Remove(TrackObjectData trackObjectData)
        {
            _gameEventBus.Raise(new RemoveTrackObjectDataEvent(trackObjectData));

            if (trackObjectData is TrackObjectGroup group)
            {
                _trackObjectGroups.Remove(group);
                //Debug.Log($"[Remove] Group '{group.trackObject.Name}' removed.");
            }
            else
            {
                _trackObjects.Remove(trackObjectData);
                //Debug.Log($"[Remove] TrackObject '{trackObjectData.trackObject.Name}' removed.");
            }
        }

        internal TrackObjectData GetTrackObjectData(GameObject gObject)
        {
            TrackObjectData data = _trackObjects.FirstOrDefault(trackObject => trackObject.sceneObject == gObject);
            if (data != null) return data;

            data = _trackObjectGroups.FirstOrDefault(trackObject => trackObject.sceneObject == gObject);
            if (data != null) return data;

            foreach (var group in _trackObjectGroups)
            {
                data = group.TrackObjectDatas.FirstOrDefault(trackObject => trackObject.sceneObject == gObject);
                if (data != null)
                {
                    //Debug.Log($"[GetTrackObjectData] Found object '{gObject.name}' inside group '{group.trackObject.Name}'.");
                    return data;
                }
            }

            //Debug.LogWarning($"[GetTrackObjectData] No TrackObjectData found for GameObject: {gObject.name}");
            return null;
        }

        internal TrackObjectGroup DeepSearchGroup(GameObject gObject)
        {
            foreach (var group in _trackObjectGroups)
            {
                return DeepSearchGroup(group, gObject);
            }

            return null;
        }

        private TrackObjectGroup DeepSearchGroup(TrackObjectGroup group, GameObject gObject)
        {
            foreach (var child in group.TrackObjectDatas)
            {
                if (child is TrackObjectGroup childGroup)
                {
                    if (child.sceneObject == gObject) return childGroup;
                    DeepSearchGroup(childGroup, gObject);
                }
            }

            return null;
        }

        /// <summary>
        /// Возвращает TrackObjectData, соответствующий указанному GameObject.
        /// Если GameObject принадлежит дочернему объекту внутри группы — возвращается сама группа (TrackObjectGroup).
        /// Если GameObject — это сама группа — возвращается группа.
        /// </summary>
        /// <param name="sceneObject">Искомый GameObject.</param>
        /// <returns>TrackObjectData (обычный объект или группа), либо null, если не найден.</returns>
        public TrackObjectData GetTrackObjectDataOrParentGroupBySceneObject(GameObject sceneObject)
        {
            if (sceneObject == null)
                return null;

            // 1. Проверяем, не является ли GameObject самой группой
            var directGroup = _trackObjectGroups.FirstOrDefault(g => g.sceneObject == sceneObject);
            if (directGroup != null)
                return directGroup;

            // 2. Проверяем, не является ли GameObject обычным (не вложенным) объектом
            var directObject = _trackObjects.FirstOrDefault(o => o.sceneObject == sceneObject);
            if (directObject != null)
                return directObject;

            // 3. Проверяем, находится ли GameObject внутри какой-либо группы как дочерний элемент
            foreach (var group in _trackObjectGroups)
            {
                var child = group.TrackObjectDatas.FirstOrDefault(o => o.sceneObject == sceneObject);
                if (child != null)
                {
                    // Возвращаем группу, а не дочерний объект
                    return group;
                }
            }

            // Не найдено
            // Debug.LogWarning($"[GetTrackObjectDataOrParentGroupBySceneObject] GameObject '{sceneObject.name}' not found in storage or any group.");
            return null;
        }

        internal TrackObjectData GetTrackObjectData(TrackObject trackObject)
        {
            TrackObjectData data =
                _trackObjects.FirstOrDefault(trackObject2 => trackObject2.trackObject == trackObject);
            if (data != null) return data;

            data = _trackObjectGroups.FirstOrDefault(trackObject2 => trackObject2.trackObject == trackObject);
            if (data != null) return data;

            foreach (var group in _trackObjectGroups)
            {
                data = group.TrackObjectDatas.FirstOrDefault(trackObject2 => trackObject2.trackObject == trackObject);
                if (data != null)
                {
                    //Debug.Log($"[GetTrackObjectData] Found TrackObject '{trackObject.Name}' inside group '{group.trackObject.Name}'.");
                    return data;
                }
            }

            //Debug.LogWarning($"[GetTrackObjectData] No TrackObjectData found for TrackObject: {trackObject.Name}");
            return null;
        }

        public void UpdatePositionSelectedTrackObject()
        {
            if (selectedObject != null)
                _gameEventBus.Raise(new DragTrackObjectEvent(selectedObject));
        }

        private void DeselectAllObject()
        {
            selectedObject = null;
            foreach (var trackObject in _trackObjects)
            {
                trackObject.trackObject.Deselect();
            }

            foreach (var group in _trackObjectGroups)
            {
                group.trackObject.Deselect();
            }
            //Debug.Log("[DeselectObject] All objects deselected.");
        }

        internal void DeselectObject(TrackObjectData deselectObject)
        {
            selectedObject = null;
            foreach (var trackObject in _trackObjects)
            {
                if (deselectObject == trackObject)
                {
                    trackObject.trackObject.Deselect();
                    return;
                }
            }

            foreach (var group in _trackObjectGroups)
            {
                if (deselectObject == group)
                {
                    group.trackObject.Deselect();
                    return;
                }
            }
        }


        public void SelectObject(TrackObject trackObjectToSelect)
        {
            var targetData = GetTrackObjectData(trackObjectToSelect);
            if (targetData == null)
            {
                //Debug.LogWarning($"[SelectObject] Cannot select: TrackObject '{trackObjectToSelect.Name}' not found in storage.");
                return;
            }

            if (!_actionMap.Editor.LeftShift.IsPressed())
                DeselectAllObject();

            InternalSelectObject(targetData);
            // _selectObjectController.Select(targetData);
            SelectColor();
        }

        public void SelectObjectTrackObject(TrackObject trackObjectToSelect)
        {
            var targetData = GetTrackObjectData(trackObjectToSelect);
            if (targetData == null)
            {
                //Debug.LogWarning($"[SelectObject] Cannot select: TrackObject '{trackObjectToSelect.Name}' not found in storage.");
                return;
            }

            if (!_actionMap.Editor.LeftShift.IsPressed())
                DeselectAllObject();

            InternalSelectObject(targetData);
            _selectObjectController.SelectMultiple(targetData);
            SelectColor();
        }


        /// <summary>
        /// Ищет TrackObjectData (включая TrackObjectGroup) по уникальному идентификатору sceneObjectID.
        /// Поиск выполняется сначала среди отдельных объектов, затем среди групп и их вложенных объектов.
        /// </summary>
        /// <param name="sceneObjectID">Уникальный идентификатор объекта.</param>
        /// <returns>Найденный TrackObjectData или null, если не найден.</returns>
        public TrackObjectData GetTrackObjectDataBySceneObjectID(string sceneObjectID)
        {
            if (string.IsNullOrEmpty(sceneObjectID))
                return null;

            // Поиск среди обычных TrackObjectData
            var found = _trackObjects.FirstOrDefault(data => data.sceneObjectID == sceneObjectID);
            if (found != null)
                return found;

            // Поиск среди групп (включая сами группы)
            found = _trackObjectGroups.FirstOrDefault(group => group.sceneObjectID == sceneObjectID);
            if (found != null)
                return found;

            // Поиск внутри вложенных объектов групп
            foreach (var group in _trackObjectGroups)
            {
                found = group.TrackObjectDatas.FirstOrDefault(data => data.sceneObjectID == sceneObjectID);
                if (found != null)
                    return found;
            }

            // Если ничего не найдено
            //Debug.LogWarning($"[GetTrackObjectDataBySceneObjectID] No TrackObjectData found with sceneObjectID: {sceneObjectID}");
            return null;
        }

        private void SelectColor()
        {
            foreach (var objectData in _selectObjectController.SelectObjects)
            {
                objectData.trackObject.SelectColor();
            }
            //Debug.Log($"[SelectColor] Applied selection color to {_selectObjectController.SelectObjects.Count} objects.");
        }

        private void InternalSelectObject(TrackObjectData trackObjectData)
        {
            this.selectedObject = trackObjectData;
        }
    }

    [Serializable]
    public class TrackObjectData
    {
        public GameObject sceneObject;
        public ActiveObjectControllerComponent activeObjectController;
        public TrackObject trackObject;
        public Branch branch;

        public string sceneObjectID;

        public TrackObjectData(GameObject sceneObject, TrackObject trackObject, Branch branch, string sceneObjectID)
        {
            this.sceneObject = sceneObject;
            this.trackObject = trackObject;
            this.branch = branch;
            this.sceneObjectID = sceneObjectID;
            this.activeObjectController = sceneObject.GetComponent<ActiveObjectControllerComponent>();
            if (!activeObjectController)
                Debug.LogWarning("Не найдер ActiveObjectControllerComponent", sceneObject);
        }
    }

    [Serializable]
    public class TrackObjectGroup : TrackObjectData
    {
        public string compositionID;
        public string lastEditID;
        private List<TrackObjectData> _trackObjectDatas;

        public List<TrackObjectData> TrackObjectDatas
        {
            get { return _trackObjectDatas; }
            set { _trackObjectDatas = value; }
        }

        public TrackObjectGroup(GameObject sceneObject, TrackObject trackObject, Branch branch, string sceneObjectID,
            List<TrackObjectData> trackObjectDatas, string compositionID, string lastEditID) : base(sceneObject,
            trackObject, branch,
            sceneObjectID)
        {
            this.compositionID = compositionID;
            this.lastEditID = lastEditID;
            this.sceneObject = sceneObject;
            this.trackObject = trackObject;
            this.branch = branch;
            this.TrackObjectDatas = trackObjectDatas;
            this.sceneObjectID = sceneObjectID;
        }

        public void Update(double newDuraction, List<TrackObjectData> trackObjectDatas, TrackObjectRemover remover,
            MainObjects _mainObjects, KeyframeTrackStorage _keyframeTrackStorage, string lastEditID,
            SaveComposition saveComposition)
        {
            Debug.Log(this.lastEditID);
            this.lastEditID = lastEditID;
            Debug.Log(this.lastEditID);

            trackObject.UpdateDuraction(newDuraction);
            foreach (var data in TrackObjectDatas)
            {
                remover.SingleRemoveNoStorage(data);
            }

            TrackObjectDatas = trackObjectDatas;

            UpdateLastEditID(TrackObjectDatas, saveComposition);

            foreach (var track in TrackObjectDatas)
            {
                track.trackObject.GroupOffsetTrack(trackObject);////
                track.trackObject.SetTime(track.trackObject.StartTimeInTicks + trackObject._reducedLeft);

                trackObject.Rezise += (value) => { track.trackObject.GroupOffset(value); };

                track.trackObject.Hide();
            }

            foreach (var selectObject in TrackObjectDatas)
            {
                if (selectObject.sceneObject.transform.parent == null ||
                    selectObject.sceneObject.transform.parent.transform == _mainObjects.SceneObjectParent)
                {
                    Vector3 pos = selectObject.sceneObject.transform.localPosition;
                    Quaternion rot = selectObject.sceneObject.transform.localRotation;
                    Vector3 scale = selectObject.sceneObject.transform.localScale;
                    selectObject.sceneObject.transform.SetParent(sceneObject.transform);
                    selectObject.sceneObject.transform.localPosition = pos;
                    selectObject.sceneObject.transform.localRotation = rot;
                    selectObject.sceneObject.transform.localScale = scale;
                }


                foreach (var node in selectObject.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(trackObject);
                    }
                }
            }
        }

        private void UpdateLastEditID(List<TrackObjectData> trackObjectDatas, SaveComposition saveComposition)
        {
            foreach (var expr in trackObjectDatas)
            {
                if (expr is TrackObjectGroup group)
                {
                    UpdateLastEditID(group.TrackObjectDatas, saveComposition);
                    Debug.Log("GROUP");
                    Debug.Log(group.lastEditID);
                    group.lastEditID = saveComposition.FindCompositionDataById(group.compositionID).lastEditID;
                }
            }
        }
    }
}