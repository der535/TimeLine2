using System;
using System.Collections.Generic;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.GeneralServices;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class GroupCreater : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [Space] [SerializeField] private GameObject scenePrefab;
        [SerializeField] private GameObject trackPrefab;

        private SelectObjectController _selectObjectController;
        private TrackObjectStorage _trackObjectStorage;
        private BranchCollection _branchCollection;
        private TrackStorage _trackStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private Main _main;

        private DiContainer _container;
        private MainObjects _mainObjects;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            BranchCollection branchCollection,
            Main main,
            TrackStorage trackStorage,
            DiContainer container,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController,
            MainObjects mainObjects)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;
            _trackStorage = trackStorage;
            _container = container;
            _selectObjectController = selectObjectController;
            _keyframeTrackStorage = keyframeTrackStorage;
            _mainObjects = mainObjects;
            _main = main;
        }

        public static (double minTime, double maxTime) CalculateMinAndMaxTime(List<TrackObjectPacket> trackObjectData)
        {
            double minTime = double.MaxValue;
            double maxTime = double.MinValue;

            foreach (var selectObject in trackObjectData)
            {
                double startTime = selectObject.components.Data.StartTimeInTicks;
                double endTime = startTime + selectObject.components.Data.TimeDurationInTicks;

                if (startTime < minTime)
                    minTime = startTime;

                if (endTime > maxTime)
                    maxTime = endTime;
            }

            return (minTime, maxTime);
        }

        public static (double minTime, double maxTime) CalculateMinAndMaxTime(List<GameObjectSaveData> trackObjectData)
        {
            double minTime = double.MaxValue;
            double maxTime = double.MinValue;

            foreach (var selectObject in trackObjectData)
            {
                double startTime = selectObject.startTime;
                double endTime = startTime + selectObject.duractionTime;

                if (startTime < minTime)
                    minTime = startTime;

                if (endTime > maxTime)
                    maxTime = endTime;
            }

            return (minTime, maxTime);
        }

        public void Create(string groupName)
        {
            if (_selectObjectController.SelectObjects.Count <= 0)
                return; // Если не выбран не один объект, то группу не создаём

            GameObject trackObject = _container
                .InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform); // Создём трекобжект группы

            TrackObjectComponents components = new TrackObjectComponents(); // Создаём класс компонентов группы
            _container.Inject(components);

            var (minTime, maxTime) =
                CalculateMinAndMaxTime(_selectObjectController
                    .SelectObjects); // Cчитаем стартовое время группы и конечное время группы

            TrackObjectData data = new TrackObjectData(maxTime - minTime, "Group", 0, string.Empty, minTime,
                0, 0, true); //Создаём класс данных группы

            components.Setup(data, trackObject.GetComponent<TrackObjectView>(),
                trackObject.GetComponent<TrackObjectSelect>(), trackObject.GetComponent<TrackObjectVisual>(),
                trackObject
                    .GetComponent<
                        TrackObjectCustomizationController>()); // Заполняем класс данных компонентами и данными

            var currentTime = TimeLineConverter.Instance.TicksCurrentTime(); // Получаем текущее время
            _main.SetTimeInTicks(
                minTime); //Устанавливаем текущее время в начало группы что бы ничего не съхало при паренте

            GameObject sceneObject = _container.InstantiatePrefab(scenePrefab, root);

            sceneObject.GetComponent<NameComponent>().Name.Value = groupName;
            sceneObject.AddComponent<CompositionOffset>();

            foreach (var selectObject in _selectObjectController.SelectObjects)
            {
                selectObject.components.Data.GroupOffset(minTime);
                selectObject.components.Data.GroupOffsetTrack(components);

                if (string.IsNullOrEmpty(selectObject.components.Data.ParentID))
                {
                    var position = selectObject.sceneObject.transform.localPosition;
                    var rotation = selectObject.sceneObject.transform.rotation;
                    var scale = selectObject.sceneObject.transform.localScale;
                    selectObject.sceneObject.transform.SetParent(sceneObject.transform);
                    selectObject.sceneObject.transform.localPosition = position;
                    selectObject.sceneObject.transform.localRotation = rotation;
                    selectObject.sceneObject.transform.localScale = scale;
                }

                foreach (var node in selectObject.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(data);
                    }
                }
            }

            Branch branch = _branchCollection.AddBranch(UniqueIDGenerator.GenerateUniqueID(), groupName);

            _trackObjectStorage.AddGroup(sceneObject, components, branch, _selectObjectController.SelectObjects,
                UniqueIDGenerator.GenerateUniqueID(), UniqueIDGenerator.GenerateUniqueID(), String.Empty);

            _main.SetTimeInTicks(currentTime);
        }

        public TrackObjectGroup Create(List<TrackObjectPacket> trackObjects, string compositionID = null)
        {
            GameObject trackObject = _container
                .InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform);

            TrackObjectComponents components = new TrackObjectComponents();
            _container.Inject(components);

            var (minTime, maxTime) = CalculateMinAndMaxTime(trackObjects);

            TrackObjectData data = new TrackObjectData(maxTime - minTime, "Group", 0, string.Empty, minTime,
                0, 0, true);

            components.Setup(data, trackObject.GetComponent<TrackObjectView>(),
                trackObject.GetComponent<TrackObjectSelect>(),
                trackObject.GetComponent<TrackObjectVisual>(),
                trackObject.GetComponent<TrackObjectCustomizationController>());


            _main.SetTimeInTicks(minTime);

            var sceneObject = _container.InstantiatePrefab(scenePrefab, root);

            sceneObject.AddComponent<CompositionOffset>();

            foreach (var selectObject in trackObjects)
            {
                selectObject.components.Data.GroupOffset(minTime);
                selectObject.components.Data.GroupOffsetTrack(components);

                if (selectObject.sceneObject.transform.parent == null ||
                    selectObject.sceneObject.transform.parent.transform == _mainObjects.SceneObjectParent)
                {
                    var position = selectObject.sceneObject.transform.localPosition;
                    var rotation = selectObject.sceneObject.transform.rotation;
                    var scale = selectObject.sceneObject.transform.localScale;
                    selectObject.sceneObject.transform.SetParent(sceneObject.transform);
                    selectObject.sceneObject.transform.localPosition = position;
                    selectObject.sceneObject.transform.localRotation = rotation;
                    selectObject.sceneObject.transform.localScale = scale;
                }

                foreach (var node in selectObject.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(data);
                    }
                }
            }


            string id = UniqueIDGenerator.GenerateUniqueID();
            Branch branch = _branchCollection.AddBranch(id, scenePrefab.name);

            if (string.IsNullOrEmpty(compositionID))
                compositionID = UniqueIDGenerator.GenerateUniqueID();

            return _trackObjectStorage.AddGroup(sceneObject, components, branch, trackObjects,
                UniqueIDGenerator.GenerateUniqueID(), compositionID, String.Empty); //?????????
        }
    }
}