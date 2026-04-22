using System;
using System.Collections.Generic;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.GeneralServices;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Zenject;
using Unity.Transforms; // Обязательно!

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
        private ObjectFactory _factory;
        private Main _main;

        private DiContainer _container;
        private MainObjects _mainObjects;
        private EntityManager _entityManager;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            BranchCollection branchCollection,
            Main main,
            TrackStorage trackStorage,
            DiContainer container,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController,
            MainObjects mainObjects, ObjectFactory factory)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;
            _trackStorage = trackStorage;
            _container = container;
            _selectObjectController = selectObjectController;
            _keyframeTrackStorage = keyframeTrackStorage;
            _mainObjects = mainObjects;
            _main = main;
            _factory = factory;
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
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

            var minLine = int.MaxValue;
            foreach (var objectPacket in _selectObjectController.SelectObjects)
            {
                if(minLine > objectPacket.components.Data.TrackLineIndex) minLine = objectPacket.components.Data.TrackLineIndex;
            }
            
            GameObject trackObject = _container
                .InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[minLine].RectTransform); // Создём трекобжект группы

            TrackObjectComponents components = new TrackObjectComponents(); // Создаём класс компонентов группы
            _container.Inject(components);

            var (minTime, maxTime) =
                CalculateMinAndMaxTime(_selectObjectController
                    .SelectObjects); // Cчитаем стартовое время группы и конечное время группы
            
            TrackObjectData data = new TrackObjectData(maxTime - minTime, groupName, minLine, string.Empty, minTime,
                0, 0, true); //Создаём класс данных группы

            components.Setup(data, trackObject.GetComponent<TrackObjectView>(),
                trackObject.GetComponent<TrackObjectSelect>(), trackObject.GetComponent<TrackObjectVisual>(),
                trackObject
                    .GetComponent<
                        TrackObjectCustomizationController>()); // Заполняем класс данных компонентами и данными

            var currentTime = TimeLineConverter.Instance.TicksCurrentTime(); // Получаем текущее время
            _main.SetTimeInTicks(
                minTime); //Устанавливаем текущее время в начало группы что бы ничего не съхало при паренте

            // GameObject sceneObject = _container.InstantiatePrefab(scenePrefab, root);
            var entity = _factory.CreateSceneObject(groupName);

            _entityManager.AddComponent<CompositionPositionOffsetData>(entity);

            EntityName.SetupName(entity, groupName);
            // sceneObject.AddComponent<CompositionOffset>();

            foreach (var selectObject in _selectObjectController.SelectObjects)
            {
                // selectObject.components.Data.GroupOffset(minTime);
                selectObject.components.Data.GroupOffsetNew(minTime);
                selectObject.components.Data.GroupOffsetTrack(components);
                // print(string.IsNullOrEmpty(selectObject.components.Data.ParentID));

                if (string.IsNullOrEmpty(selectObject.components.Data.ParentID))
                {
                    // Debug.Log(entity.Index);
                    // Debug.Log(entity.Version);
// Для ребенка
                    _entityManager.AddComponentData(selectObject.entity, new Unity.Transforms.Parent { Value = entity });
// Если хочешь, чтобы он наследовал трансформации родителя:
                    if (!_entityManager.HasComponent<LocalToWorld>(selectObject.entity)) 
                        _entityManager.AddComponent<LocalToWorld>(selectObject.entity);

// Для родителя (обязательно!)
                    if (!_entityManager.HasComponent<LocalToWorld>(entity)) 
                        _entityManager.AddComponent<LocalToWorld>(entity);
                    
                    // var position = selectObject.sceneObject.transform.localPosition;
                    // var rotation = selectObject.sceneObject.transform.rotation;
                    // var scale = selectObject.sceneObject.transform.localScale;
                    // selectObject.sceneObject.transform.SetParent(sceneObject.transform);
                    // selectObject.sceneObject.transform.localPosition = position;
                    // selectObject.sceneObject.transform.localRotation = rotation;
                    // selectObject.sceneObject.transform.localScale = scale;
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

            _trackObjectStorage.AddGroup(null, entity, components, branch, _selectObjectController.SelectObjects,
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

            var  entity = _factory.CreateSceneObject(scenePrefab.name);


            // sceneObject.AddComponent<CompositionOffset>();

            foreach (var selectObject in trackObjects)
            {
                selectObject.components.Data.GroupOffsetNew(minTime);
                selectObject.components.Data.GroupOffsetTrack(components);

                // if (selectObject.sceneObject.transform.parent == null)
                if (!_entityManager.HasComponent<Unity.Transforms.Parent>(selectObject.entity))
                {
                    _entityManager.AddComponentData(selectObject.entity, new Unity.Transforms.Parent { Value = entity });
// Если хочешь, чтобы он наследовал трансформации родителя:
                    if (!_entityManager.HasComponent<LocalToWorld>(selectObject.entity)) 
                        _entityManager.AddComponent<LocalToWorld>(selectObject.entity);

// Для родителя (обязательно!)
                    if (!_entityManager.HasComponent<LocalToWorld>(entity)) 
                        _entityManager.AddComponent<LocalToWorld>(entity);

                    
                    // var position = selectObject.sceneObject.transform.localPosition;
                    // var rotation = selectObject.sceneObject.transform.rotation;
                    // var scale = selectObject.sceneObject.transform.localScale;
                    // selectObject.sceneObject.transform.SetParent(sceneObject.transform);
                    // selectObject.sceneObject.transform.localPosition = position;
                    // selectObject.sceneObject.transform.localRotation = rotation;
                    // selectObject.sceneObject.transform.localScale = scale;
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

            return _trackObjectStorage.AddGroup(null, entity, components, branch, trackObjects,
                UniqueIDGenerator.GenerateUniqueID(), compositionID, String.Empty); //?????????
        }
    }
}