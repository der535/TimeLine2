using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.Core.MusicLoader;
using TimeLine.LevelEditor.ECS.Components;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using TimeLine.LevelEditor.ValueEditor.Test;
using TimeLine.Parent;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Debug = UnityEngine.Debug;

namespace TimeLine
{
    public class TrackObjectStorage : MonoBehaviour
    {
        private readonly List<TrackObjectPacket> _trackObjects = new();
        private readonly List<TrackObjectGroup> _trackObjectGroups = new();

        [FormerlySerializedAs("_selectedObject")]
        public TrackObjectPacket selectedObject;

        private GameEventBus _gameEventBus;
        private SelectObjectController _selectObjectController;
        private SaveComposition _composition;
        private ActionMap _actionMap;
        private M_PlaybackState _playbackState;
        private EntityManager _entityManager;

        [Inject]
        private void Construct(GameEventBus gameEventBus, SelectObjectController selectObjectController,
            SaveComposition saveComposition, ActionMap actionMap, TrackObjectRemover trackObjectRemover,
            M_PlaybackState _playbackState)
        {
            _gameEventBus = gameEventBus;
            _selectObjectController = selectObjectController;
            _composition = saveComposition;
            _actionMap = actionMap;
            this._playbackState = _playbackState;
        }

        public List<TrackObjectPacket> TrackObjects => _trackObjects;
        public List<TrackObjectGroup> TrackObjectGroups => _trackObjectGroups;

        private void Awake()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _gameEventBus.SubscribeTo((ref TickSmoothTimeEvent x) => ActiveSceneObject(x.Time));
            _gameEventBus.SubscribeTo((ref LevelLoadedEvent _) =>
                ActiveSceneObject(_playbackState.SmoothTimeInTicks));
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent _) => DeselectAllObject());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                var trackObjectData = GetTrackObjectData(data.Tracks[^1].components.TrackObject);
                if (trackObjectData != null)
                {
                    InternalSelectObject(trackObjectData);
                }

                foreach (var track in data.Tracks)
                {
                    SelectObject(track.components.TrackObject);
                }
            });

            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => { DeselectObject(data.DeselectedObject); });
        }

        internal double GetMinTime()
        {
            var minFromObjects = _trackObjects.Select(f => f.components.Data.GetGlobalTicksPosition());
            var minFromGroups = _trackObjectGroups.Select(f => f.components.Data.GetGlobalTicksPosition());
            var allTimes = minFromObjects.Concat(minFromGroups);

            var enumerable = allTimes.ToList();
            return enumerable.Any() ? enumerable.Min() : 0.0;
        }

        internal List<TrackObjectPacket> GetAllActiveTrackData()
        {
            List<TrackObjectPacket> trackObjectData = new List<TrackObjectPacket>();
            foreach (var track in _trackObjects)
            {
                if (track.components.View.GetActive()) trackObjectData.Add(track);
            }

            foreach (var group in _trackObjectGroups)
            {
                if (group.components.View.GetActive()) trackObjectData.Add(group);
            }

            return trackObjectData;
        }

        internal List<TrackObjectPacket> GetAllActiveSceneObjects()
        {
            List<TrackObjectPacket> trackObjectData = new List<TrackObjectPacket>();
            foreach (var track in _trackObjects)
            {
                if (_entityManager.IsComponentEnabled<EntityActiveTag>(track.entity))
                {
                    trackObjectData.Add(track);
                }
            }

            foreach (var group in _trackObjectGroups)
            {
                if (_entityManager.IsComponentEnabled<EntityActiveTag>(group.entity))
                {
                    trackObjectData.Add(group);
                }
            }


            return trackObjectData;
        }


        private void ActiveSceneObject(double time)
        {
            for (int i = 0; i < _trackObjects.Count; i++)
            {
                CheckActiveTrackObjects(_trackObjects[i], time);
            }

            for (int i = 0; i < _trackObjectGroups.Count; i++)
            {
                CheckActiveGroup(_trackObjectGroups[i], time);
            }
        }

        internal TrackObjectPacket FindObjectByID(string id)
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


        /// <summary>
        /// Проверка одного конкретного трекобжекта
        /// </summary>
        /// <param name="trackObject"></param>
        internal void CheckActiveTrackSingle(TrackObjectPacket trackObject)
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

        public void ToggleEntity(Entity entity, bool active)
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (active)
            {
                if (manager.HasComponent<EntityActiveTag>(entity))
                {
                    bool isActive = manager.IsComponentEnabled<EntityActiveTag>(entity);
                    if (isActive == true) return;
                }

                manager.SetComponentEnabled<EntityActiveTag>(entity, true);
                manager.SetComponentData(entity, new EntityActiveTag() { IsActive = true });
                manager.AddComponent<ActivatingRequestTag>(entity);
            }
            else
            {
                if (manager.HasComponent<EntityActiveTag>(entity))
                {
                    bool isActive = manager.IsComponentEnabled<EntityActiveTag>(entity);
                    if (isActive == false) return;
                }

                manager.SetComponentEnabled<EntityActiveTag>(entity, false);
                manager.SetComponentData(entity, new EntityActiveTag() { IsActive = false });
                manager.AddComponent<DeactivatingRequestTag>(entity);
            }
        }

        private void CheckActiveTrackObjects(TrackObjectPacket trackObject, double time)
        {
            if (trackObject.components.View.GetActive())
            {
                bool shouldBeActive = trackObject.components.Data.GetGlobalTicksPosition() <= time &&
                                      trackObject.components.Data.TimeDurationInTicks +
                                      trackObject.components.Data.GetGlobalTicksPosition() > time;

                // trackObject.activeObjectController?.Turn(trackObject.components.Data.IsActive && shouldBeActive);
                ToggleEntity(trackObject.entity, shouldBeActive);
            }
            else
            {
                // trackObject.activeObjectController?.Turn(false);
                ToggleEntity(trackObject.entity, false);
            }
        }

        private void CheckActiveGroup(TrackObjectGroup group, double time, bool enchanted = false,
            bool activeGroup = true)
        {
            // Debug.Log($" {group.components.Data.Name} --------------------------------------");
            double groupStart = group.components.Data.GetGlobalTicksPosition();
            double groupEnd = groupStart + group.components.Data.TimeDurationInTicks;
            bool isGroupActive = time >= groupStart && time < groupEnd;

            if ((!enchanted && !group.components.View.GetActive()) || activeGroup == false)
            {
                ToggleEntity(group.entity, false);
                // group.activeObjectController.Turn(false);

                foreach (var trackObject in group.TrackObjectDatas)
                {
                    if (trackObject is TrackObjectGroup nestedGroup)
                    {
                        CheckActiveGroup(nestedGroup, time, true, false);
                        continue;
                    }

                    // trackObject.activeObjectController.Turn(false);
                    ToggleEntity(trackObject.entity, false);
                }

                return;
            }


            ToggleEntity(group.entity, isGroupActive);
            // group.activeObjectController.Turn(isGroupActive);

            foreach (var trackObject in group.TrackObjectDatas)
            {
                if (!isGroupActive)
                {
                    // trackObject.activeObjectController.Turn(false);
                    ToggleEntity(trackObject.entity, false);
                }

                if (trackObject is TrackObjectGroup nestedGroup)
                {
                    CheckActiveGroup(nestedGroup, time, true, isGroupActive);
                    continue;
                }

                // Debug.Log($" {trackObject.components.Data.Name} --------------------------------------");

                double objStart = trackObject.components.Data.GetGlobalTicksPosition();
                double objEnd = objStart + trackObject.components.Data.TimeDurationInTicks;
                bool isObjectActive = time >= objStart && time < objEnd;

                bool finalState = group.components.Data.IsActive && isObjectActive && isGroupActive;

                // trackObject.activeObjectController.Turn(finalState);
                ToggleEntity(trackObject.entity, finalState);
            }
        }

        internal TrackObjectPacket Add(GameObject sceneObject, Entity entity, TrackObjectComponents selectedObject,
            Branch branch,
            string id)
        {
            TrackObjectPacket trackObjectPacket =
                new TrackObjectPacket(sceneObject, entity, selectedObject, branch, id);
            _gameEventBus.Raise(new AddTrackObjectDataEvent(trackObjectPacket));
            _trackObjects.Add(trackObjectPacket);
            // sceneObject.GetComponent<SceneObjectLink>().trackObjectPacket = trackObjectPacket;

            //Debug.Log($"[Add] TrackObject '{selectedObject.Name}' added to storage.");
            return trackObjectPacket;
        }

        internal TrackObjectGroup AddGroup(GameObject sceneObject, Entity entity, TrackObjectComponents trackObject,
            Branch branch,
            List<TrackObjectPacket> trackObjectDatas, string sceneObjectID, string compositionID, string lastEditID,
            bool addToStorage = true)
        {
            var objectsForGroup = new List<TrackObjectPacket>(trackObjectDatas);

            foreach (var trackObjectData in objectsForGroup)
            {
                if (trackObjectData is TrackObjectGroup group2)
                    _trackObjectGroups.Remove(group2);
                else
                    _trackObjects.Remove(trackObjectData);

                trackObjectData.components.View.Hide();
            }

            if (compositionID == "")
                compositionID = Guid.NewGuid().ToString();
            TrackObjectGroup group =
                new TrackObjectGroup(sceneObject, entity, trackObject, branch, sceneObjectID, objectsForGroup,
                    compositionID,
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
                track.components.Data.GroupOffsetTrack(trackObject);
                trackObject.TrackObject.Rezise += (value) => { track.components.Data.GroupOffset(value); };
            }

            _composition.AddComposition(group);

            return group;
        }

        internal void HideAll()
        {
            foreach (var trackObject in _trackObjects)
            {
                trackObject.components.View.Hide();
            }

            foreach (var group in _trackObjectGroups)
            {
                group.components.View.Hide();
            }
            //Debug.Log("[HideAll] All track objects and groups hidden.");
        }

        internal void ShowAll()
        {
            foreach (var trackObject in _trackObjects)
            {
                trackObject.components.View.Show();
            }

            foreach (var group in _trackObjectGroups)
            {
                group.components.View.Show();
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

        internal void Remove(TrackObjectPacket trackObjectPacket)
        {
            _gameEventBus.Raise(new RemoveTrackObjectDataEvent(trackObjectPacket));

            if (trackObjectPacket is TrackObjectGroup group)
            {
                _trackObjectGroups.Remove(group);
                //Debug.Log($"[Remove] Group '{group.trackObject.Name}' removed.");
            }
            else
            {
                _trackObjects.Remove(trackObjectPacket);
                //Debug.Log($"[Remove] TrackObject '{trackObjectData.trackObject.Name}' removed.");
            }
        }

        internal TrackObjectPacket GetTrackObjectData(GameObject gObject)
        {
            TrackObjectPacket packet = _trackObjects.FirstOrDefault(trackObject => trackObject.sceneObject == gObject);
            if (packet != null) return packet;

            packet = _trackObjectGroups.FirstOrDefault(trackObject => trackObject.sceneObject == gObject);
            if (packet != null) return packet;

            foreach (var group in _trackObjectGroups)
            {
                packet = group.TrackObjectDatas.FirstOrDefault(trackObject => trackObject.sceneObject == gObject);
                if (packet != null)
                {
                    //Debug.Log($"[GetTrackObjectData] Found object '{gObject.name}' inside group '{group.trackObject.Name}'.");
                    return packet;
                }
            }

            //Debug.LogWarning($"[GetTrackObjectData] No TrackObjectData found for GameObject: {gObject.name}");
            return null;
        }

        internal TrackObjectPacket GetTrackObjectData(Entity gObject)
        {
            TrackObjectPacket packet = _trackObjects.FirstOrDefault(trackObject => trackObject.entity == gObject);
            if (packet != null) return packet;

            packet = _trackObjectGroups.FirstOrDefault(trackObject => trackObject.entity == gObject);
            if (packet != null) return packet;

            foreach (var group in _trackObjectGroups)
            {
                packet = group.TrackObjectDatas.FirstOrDefault(trackObject => trackObject.entity == gObject);
                if (packet != null)
                {
                    //Debug.Log($"[GetTrackObjectData] Found object '{gObject.name}' inside group '{group.trackObject.Name}'.");
                    return packet;
                }
            }

            //Debug.LogWarning($"[GetTrackObjectData] No TrackObjectData found for GameObject: {gObject.name}");
            return null;
        }

        /// <summary>
        /// Возвращает абсолютно все TrackObjectPacket
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public List<TrackObjectPacket> GetAllTrackObjectPacket(TrackObjectGroup group)
        {
            List<TrackObjectPacket> childs = new List<TrackObjectPacket>();

            childs.Add(group);
            foreach (var VARIABLE in group.TrackObjectDatas)
            {
                childs.Add(VARIABLE);
                if (VARIABLE is TrackObjectGroup trackObjectPacket)
                {
                    childs.AddRange(GetAllTrackObjectPacket(trackObjectPacket));
                }
            }
            return childs;
        }


        /// <summary>
        /// Возвращает TrackObjectData, соответствующий указанному GameObject.
        /// Если GameObject принадлежит дочернему объекту внутри группы — возвращается сама группа (TrackObjectGroup).
        /// Если GameObject — это сама группа — возвращается группа.
        /// </summary>
        /// <param name="sceneObject">Искомый GameObject.</param>
        /// <returns>TrackObjectData (обычный объект или группа), либо null, если не найден.</returns>
        public TrackObjectPacket GetTrackObjectDataOrParentGroupBySceneObject(GameObject sceneObject)
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

        internal TrackObjectPacket GetTrackObjectData(TrackObject trackObject)
        {
            TrackObjectPacket packet =
                _trackObjects.FirstOrDefault(trackObject2 => trackObject2.components.TrackObject == trackObject);
            if (packet != null) return packet;

            packet = _trackObjectGroups.FirstOrDefault(trackObject2 =>
                trackObject2.components.TrackObject == trackObject);
            if (packet != null) return packet;

            foreach (var group in _trackObjectGroups)
            {
                packet = group.TrackObjectDatas.FirstOrDefault(trackObject2 =>
                    trackObject2.components.TrackObject == trackObject);
                if (packet != null)
                {
                    //Debug.Log($"[GetTrackObjectData] Found TrackObject '{trackObject.Name}' inside group '{group.trackObject.Name}'.");
                    return packet;
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
                trackObject.components.Select.Deselect();
            }

            foreach (var group in _trackObjectGroups)
            {
                group.components.Select.Deselect();
            }
            //Debug.Log("[DeselectObject] All objects deselected.");
        }

        internal void DeselectObject(TrackObjectPacket deselectObject)
        {
            selectedObject = null;
            foreach (var trackObject in _trackObjects)
            {
                if (deselectObject == trackObject)
                {
                    trackObject.components.Select.Deselect();
                    return;
                }
            }

            foreach (var group in _trackObjectGroups)
            {
                if (deselectObject == group)
                {
                    group.components.Select.Deselect();
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
        public TrackObjectPacket GetTrackObjectDataBySceneObjectID(string sceneObjectID)
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
                objectData.components.Select.SelectColor();
            }
            //Debug.Log($"[SelectColor] Applied selection color to {_selectObjectController.SelectObjects.Count} objects.");
        }

        private void InternalSelectObject(TrackObjectPacket trackObjectPacket)
        {
            this.selectedObject = trackObjectPacket;
        }
    }

    [Serializable]
    public class TrackObjectPacket
    {
        public GameObject sceneObject;
        public Entity entity;
        public ActiveObjectControllerComponent activeObjectController;
        [FormerlySerializedAs("trackObject")] public TrackObjectComponents components;
        public Branch branch;

        public string sceneObjectID;

        public TrackObjectPacket(GameObject sceneObject, Entity entity, TrackObjectComponents components, Branch branch,
            string sceneObjectID)
        {
            this.sceneObject = sceneObject;
            this.entity = entity;
            this.components = components;
            this.branch = branch;
            this.sceneObjectID = sceneObjectID;
            // this.activeObjectController = sceneObject.GetComponent<ActiveObjectControllerComponent>();
            // if (!activeObjectController)
            // Debug.LogWarning("Не найдер ActiveObjectControllerComponent", sceneObject);
        }
    }

    [Serializable]
    public class TrackObjectGroup : TrackObjectPacket
    {
        public string compositionID;
        public string lastEditID;
        private List<TrackObjectPacket> _trackObjectDatas;

        public List<TrackObjectPacket> TrackObjectDatas
        {
            get { return _trackObjectDatas; }
            set { _trackObjectDatas = value; }
        }

        public TrackObjectGroup(GameObject sceneObject, Entity entity, TrackObjectComponents components, Branch branch,
            string sceneObjectID,
            List<TrackObjectPacket> trackObjectDatas, string compositionID, string lastEditID) : base(sceneObject,
            entity, components, branch,
            sceneObjectID)
        {
            this.entity = entity;
            this.compositionID = compositionID;
            this.lastEditID = lastEditID;
            this.sceneObject = sceneObject;
            this.components = components;
            this.branch = branch;
            this.TrackObjectDatas = trackObjectDatas;
            this.sceneObjectID = sceneObjectID;
        }

        /// <summary>
        /// Обновляет содержимое композиций в таймлайне
        /// </summary>
        /// <param name="newDuraction">Новая продолжительность композиции</param>
        /// <param name="trackObjectDatas">Обновляемые объекты</param>
        /// <param name="remover"></param>
        /// <param name="_mainObjects"></param>
        /// <param name="_keyframeTrackStorage"></param>
        /// <param name="lastEditID"></param>
        /// <param name="saveComposition"></param>
        /// <param name="compositionUpdateID">ID композиции который обновился</param>
        public void Update(double newDuraction, List<TrackObjectPacket> trackObjectDatas, TrackObjectRemover remover,
            MainObjects _mainObjects, KeyframeTrackStorage _keyframeTrackStorage, string lastEditID,
            SaveComposition saveComposition, string compositionUpdateID, bool updateSelf,
            LoadGraphLogic _loadGraphLogic)
        {
            this.lastEditID = lastEditID;

            components.Data.UpdateDuraction(newDuraction);

            List<TrackObjectPacket> updateTrackObjectDatas = new List<TrackObjectPacket>();

            //Если композиция не полностью обновляется
            if (updateSelf == false)
            {
                // Ищем композиции которые надо обновить и удаляем старые версии
                foreach (var VARIABLE in TrackObjectDatas.ToList())
                {
                    if (VARIABLE is TrackObjectGroup group)
                    {
                        if (group.compositionID == compositionUpdateID)
                        {
                            Debug.Log(group.compositionID == compositionUpdateID);
                            remover.ListRemove(group);
                            TrackObjectDatas.Remove(VARIABLE);
                        }
                    }
                }

                TrackObjectDatas.AddRange(trackObjectDatas);

                updateTrackObjectDatas = trackObjectDatas;
            }
            else
            {
                foreach (var data in TrackObjectDatas)
                {
                    if (data is TrackObjectGroup group)
                        remover.ListRemove(group);
                    else
                        remover.SingleRemoveNoStorage(data);
                }

                TrackObjectDatas = trackObjectDatas;
                updateTrackObjectDatas = TrackObjectDatas;
                //Обновляем ID У всех
            }


            UpdateLastEditID(updateTrackObjectDatas, saveComposition);


            foreach (var track in updateTrackObjectDatas)
            {
                track.components.Data.GroupOffsetTrack(components); ////
                // track.components.Data.StartTimeInTicks += components.Data.ReducedLeft;

                // components.TrackObject.Rezise += (value) => { track.components.Data.GroupOffset(value); };

                track.components.View.Hide();
            }

            var _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var selectObject in updateTrackObjectDatas)
            {
                // if (selectObject.sceneObject.transform.parent == null ||
                // selectObject.sceneObject.transform.parent.transform == _mainObjects.SceneObjectParent)
                if (!_entityManager.HasComponent<Unity.Transforms.Parent>(selectObject.entity))
                {
                    _entityManager.AddComponentData(selectObject.entity,
                        new Unity.Transforms.Parent { Value = entity });
                    // Если хочешь, чтобы он наследовал трансформации родителя:
                    if (!_entityManager.HasComponent<LocalToWorld>(selectObject.entity))
                        _entityManager.AddComponent<LocalToWorld>(selectObject.entity);

                    // Для родителя (обязательно!)
                    if (!_entityManager.HasComponent<LocalToWorld>(entity))
                        _entityManager.AddComponent<LocalToWorld>(entity);
                }


                foreach (var node in selectObject.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(components.Data);
                    }
                }
            }


            _loadGraphLogic.LoadGraph(TrackObjectDatas);

            ParentLinkRestorer.Restor(updateTrackObjectDatas);
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            GetChildrenExample(_trackObjectDatas.Select(x => x.entity).ToList(),
                entityManager.GetComponentData<CompositionPositionOffsetData>(entity).Offset);

            components.TrackObject.Rezise = null;

            foreach (var track in TrackObjectDatas)
            {
                components.TrackObject.Rezise += (value) => { track.components.Data.GroupOffset(value); };
            }
        }

        private void GetChildrenExample(List<Entity> children, float2 newOffset)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            // Проверяем, есть ли у сущности вообще дети

            foreach (var childEntity in children)
            {
               Debug.Log( entityManager.HasComponent(childEntity, typeof(ObjectPositionOffsetData)));
               Debug.Log( childEntity.Index);
               Debug.Log( childEntity.Version);
                ObjectPositionOffsetData
                    offsetData =
                        entityManager
                            .GetComponentData<ObjectPositionOffsetData>(childEntity); // Получаем компонент оффсета

                offsetData.Offset = newOffset; //Задаём новый оффсет

                entityManager.SetComponentData(childEntity, offsetData); //Применяем данные
                LocalTransform
                    localtransform =
                        entityManager.GetComponentData<LocalTransform>(childEntity); //Получаем компонент трансформа
                PositionData
                    positionData =
                        entityManager.GetComponentData<PositionData>(childEntity); //Получаем компонент трансформа
                localtransform.Position = new float3(positionData.Position.x + offsetData.Offset.x,
                    positionData.Position.y + offsetData.Offset.y, localtransform.Position.z);
                entityManager.SetComponentData(childEntity, localtransform);
            }
        }

        private void UpdateLastEditID(List<TrackObjectPacket> trackObjectDatas, SaveComposition saveComposition)
        {
            foreach (var expr in trackObjectDatas)
            {
                if (expr is TrackObjectGroup group)
                {
                    UpdateLastEditID(group.TrackObjectDatas, saveComposition);
                    group.lastEditID = saveComposition.FindCompositionDataById(group.compositionID).lastEditID;
                }
            }
        }
    }
}