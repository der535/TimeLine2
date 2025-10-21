using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using NaughtyAttributes;
using Newtonsoft.Json;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
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

        [Inject]
        private void Construct(GameEventBus gameEventBus, SelectObjectController selectObjectController, SaveComposition saveComposition)
        {
            _gameEventBus = gameEventBus;
            _selectObjectController = selectObjectController;
            _composition = saveComposition;
        }

        public List<TrackObjectData> TrackObjects => _trackObjects;
        public List<TrackObjectGroup> TrackObjectGroups => _trackObjectGroups;

        private void Awake()
        {
            _gameEventBus.SubscribeTo<TickSmoothTimeEvent>(ActiveSceneObject);
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => DeselectObject());

            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                var trackObjectData = GetTrackObjectData(data.Tracks[^1].trackObject);
                if (trackObjectData != null)
                {
                    InternalSelectObject(trackObjectData);
                }
            });
        }

        [Button]
        private void checkGroupIntrackObjects()
        {
            foreach (var objectData in _trackObjects)
            {
                print(objectData);
            }
        }

        private void ActiveSceneObject(ref TickSmoothTimeEvent smoothTimeEvent)
        {
            foreach (var trackObject in _trackObjects)
            {
                CheckActiveTrackObjects(trackObject, smoothTimeEvent.Time);
            }

            foreach (var group in _trackObjectGroups)
            {
                CheckActiveGroup(group, smoothTimeEvent.Time);
            }
        }

        private void CheckActiveTrackObjects(TrackObjectData trackObject, double time)
        {
            if (trackObject.trackObject.gameObject.activeSelf)
            {
                bool shouldBeActive = trackObject.trackObject.StartTimeInTicks <= time &&
                                      trackObject.trackObject.TimeDuractionInTicks +
                                      trackObject.trackObject.StartTimeInTicks > time;

                trackObject.sceneObject.SetActive(shouldBeActive);
                // //Debug.Log($"[TrackObject] {trackObject.trackObject.Name} | Active: {shouldBeActive} | Time: {time}");
            }
            else
            {
                trackObject.sceneObject.SetActive(false);
            }
        }

        private void CheckActiveGroup(TrackObjectGroup group, double time, bool enchanted = false)
        {
            if (!enchanted && !group.trackObject.gameObject.activeSelf)
            {
                group.sceneObject.SetActive(false);
                foreach (var trackObject in group.TrackObjectDatas)
                {
                    trackObject.sceneObject.SetActive(false);
                }

                return;
            }
            else
            {
                group.sceneObject.SetActive(true);
            }

            double groupStart = group.trackObject.StartTimeInTicks;
            double groupEnd = groupStart + group.trackObject.TimeDuractionInTicks;
            bool isGroupActive = time >= groupStart && time < groupEnd;

            //Debug.Log($"[Group] {group.branch.ID} | Active: {isGroupActive} | Time: {time} | Range: [{groupStart}, {groupEnd})");

            foreach (var trackObject in group.TrackObjectDatas)
            {
                if (!isGroupActive)
                {
                    trackObject.sceneObject.SetActive(false);
                    continue;
                }

                if (trackObject is TrackObjectGroup nestedGroup)
                {
                    //Debug.Log($"    → Entering nested group: {nestedGroup.branch.ID}");
                    CheckActiveGroup(nestedGroup, time - groupStart, true); // Исправлено: передаём time, а не смещение!
                    continue;
                }

                // if (!enchanted)
                // {
                double objStart = trackObject.trackObject.StartTimeInTicks + groupStart;
                double objEnd = objStart + trackObject.trackObject.TimeDuractionInTicks;
                var isObjectActive = time >= objStart && time < objEnd;
                //Debug.Log($"  [Child] {trackObject.branch.ID} | Active: {isObjectActive} | Time: {time} | Range: [{objStart}, {objEnd})");
                //
                // }
                // else
                // {
                //     isObjectActive = time >= group.trackObject.StartTimeInTicks && time < groupStart + group.trackObject.TimeDuractionInTicks;
                // }

                trackObject.sceneObject.SetActive(isObjectActive);
            }
        }

        internal TrackObjectData Add(GameObject sceneObject, TrackObject selectedObject, Branch branch, string id)
        {
            TrackObjectData trackObjectData = new TrackObjectData(sceneObject, selectedObject, branch, id);
            _gameEventBus.Raise(new AddTrackObjectDataEvent(trackObjectData));
            _trackObjects.Add(trackObjectData);
            //Debug.Log($"[Add] TrackObject '{selectedObject.Name}' added to storage.");
            return trackObjectData;
        }

        internal TrackObjectGroup AddGroup(GameObject sceneObject, TrackObject trackObject, Branch branch,
            List<TrackObjectData> trackObjectDatas, string sceneObjectID, string compositionID)
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

            if(compositionID == "")
                compositionID = Guid.NewGuid().ToString();
            TrackObjectGroup group =
                new TrackObjectGroup(sceneObject, trackObject, branch, sceneObjectID, objectsForGroup, compositionID)
                {
                    compositionID = compositionID
                };
            print(_composition);
            _trackObjectGroups.Add(group);

            // Подписка на изменение размера
            foreach (var track in trackObjectDatas)
            {
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
            _gameEventBus.Raise(new DragTrackObjectEvent(selectedObject));
        }

        private void DeselectObject()
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

        public void SelectObject(TrackObject trackObjectToSelect)
        {
            var targetData = GetTrackObjectData(trackObjectToSelect);
            if (targetData == null)
            {
                //Debug.LogWarning($"[SelectObject] Cannot select: TrackObject '{trackObjectToSelect.Name}' not found in storage.");
                return;
            }

            if (!UnityEngine.Input.GetKey(KeyCode.LeftShift))
                DeselectObject();

            InternalSelectObject(targetData);
            _selectObjectController.Select(targetData, UnityEngine.Input.GetKey(KeyCode.LeftShift));
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
        public TrackObject trackObject;
        public Branch branch;

        public string sceneObjectID;

        public TrackObjectData(GameObject sceneObject, TrackObject trackObject, Branch branch, string sceneObjectID)
        {
            this.sceneObject = sceneObject;
            this.trackObject = trackObject;
            this.branch = branch;
            this.sceneObjectID = sceneObjectID;
        }
    }

    [Serializable]
    public class TrackObjectGroup : TrackObjectData
    {
        public string compositionID;
        private List<TrackObjectData> _trackObjectDatas;

        public List<TrackObjectData> TrackObjectDatas
        {
            get { return _trackObjectDatas; }
            set { _trackObjectDatas = value; }
        }

        public TrackObjectGroup(GameObject sceneObject, TrackObject trackObject, Branch branch, string sceneObjectID,
            List<TrackObjectData> trackObjectDatas, string compositionID) : base(sceneObject, trackObject, branch,
            sceneObjectID)
        {
            this.compositionID = compositionID;
            this.sceneObject = sceneObject;
            this.trackObject = trackObject;
            this.branch = branch;
            this.TrackObjectDatas = trackObjectDatas;
            this.sceneObjectID = sceneObjectID;
        }
    }
}