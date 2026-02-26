using System;
using TimeLine.LevelEditor.GeneralServices;
using TimeLine.LevelEditor.MaxObjectIndex.Controller;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using TimeLine.LevelEditor.TrackObjectSize.Data;
using TimeLine.TimeLine;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning
{
    public class ObjectFactory
    {
        private Transform _rootSceneObject;
        private GameObject _sceneObjectPrefab;
        private GameObject _trackObjectPrefab;

        private Main _main;
        private DiContainer _container;
        private TrackStorage _trackStorage;
        private BranchCollection _branchCollection;
        private TrackObjectStorage _trackObjectStorage;
        private ITrackObjectSizeReader _trackObjectSizeReader;
        private IMaxObjectIndexDataReading _maxObjectIndexDataReading;
        private AddAnEntitySprite _addAnEntitySprite;

        public ObjectFactory(
            Main main,
            DiContainer container,
            Transform rootSceneObject,
            TrackStorage trackStorage,
            GameObject sceneObjectPrefab,
            GameObject trackObjectPrefab,
            BranchCollection branchCollection,
            TrackObjectStorage trackObjectStorage,
            ITrackObjectSizeReader trackObjectSizeReader,
            IMaxObjectIndexDataReading maxObjectIndexDataReading,
            AddAnEntitySprite addAnEntitySprite)
        {
            _main = main;
            _container = container;
            _rootSceneObject = rootSceneObject;
            _sceneObjectPrefab = sceneObjectPrefab;
            _trackObjectPrefab = trackObjectPrefab;
            _trackStorage = trackStorage;
            _branchCollection = branchCollection;
            _trackObjectStorage = trackObjectStorage;
            _trackObjectSizeReader = trackObjectSizeReader;
            _maxObjectIndexDataReading = maxObjectIndexDataReading;
            _addAnEntitySprite = addAnEntitySprite;
        }

        /// <summary>
        /// Создаёт объект на сцене с компонентам спрайта
        /// </summary>
        internal TrackObjectPacket CreateSceneObjectAndAddSprite(Sprite sprite)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            var (sceneObject, entity) = CreateSceneObject();
            
            _addAnEntitySprite.SetupSpriteRender(entity, sprite);

            var component = sceneObject.AddComponent<SpriteRendererComponent>();
            _container.Inject(component);
            component.Sprite.Value = sprite;

            string name = $"{sprite.name} {_maxObjectIndexDataReading.GetNextIndex()}";

            sceneObject.name = name;

            // Создаем трек-объект
            TrackObjectComponents trackObject = CreateTrackObject(_trackObjectSizeReader.GetSize(), name,
                0, TimeLineConverter.Instance.TicksCurrentTime());

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, name);

            // Добавляем в хранилище
            var TrackObjectData =
                _trackObjectStorage.Add(sceneObject, trackObject, branch, UniqueIDGenerator.GenerateUniqueID());


            sceneObject.GetComponent<NameComponent>().Name.Value = name;

            return TrackObjectData;
        }

        internal TrackObjectPacket CreateFullObject(string name)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            GameObject sceneObject = CreateSceneObject().Item1;

            // Создаем трек-объект
            TrackObjectComponents trackObject = CreateTrackObject(_trackObjectSizeReader.GetSize(), name,
                0, TimeLineConverter.Instance.TicksCurrentTime());

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, name);

            // Добавляем в хранилище
            var TrackObjectData =
                _trackObjectStorage.Add(sceneObject, trackObject, branch, UniqueIDGenerator.GenerateUniqueID());

            sceneObject.GetComponent<NameComponent>().Name.Value = name;

            return TrackObjectData;
        }

        /// <summary>
        /// Создаёт объект на сцене
        /// </summary>
        /// <returns></returns>
        internal (GameObject, Entity) CreateSceneObject()
        {
            //Старая система создание объекта
            GameObject sceneTrackObject = _container.InstantiatePrefab(_sceneObjectPrefab, _rootSceneObject);

            //Н
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = entityManager.CreateEntity();

            return (sceneTrackObject, entity);
        }

        /// <summary>
        /// Создает объект трека
        /// </summary>
        /// <param name="ticksLifeTime">Продолжительность трекобжекта</param>
        /// <param name="name">Имя трекобжекта</param>
        /// <param name="trackLine">Номер линии в тамйлане на которов будет создан трекобжект</param>
        /// <param name="startTime">Время начала</param>
        /// <returns></returns>
        internal TrackObjectComponents CreateTrackObject(double ticksLifeTime, string name, int index,
            double startTime, bool createTrackObject = true)
        {
            TrackObjectComponents components = new TrackObjectComponents();
            _container.Inject(components);

            TrackObjectData data = new TrackObjectData(ticksLifeTime, name, index, string.Empty, startTime, 0, 0);

            if (!createTrackObject)
                components.Setup(data);
            else
            {
                GameObject trackObject = _container.InstantiatePrefab(_trackObjectPrefab,
                    _trackStorage.GetTrackLineByIndex(index).RectTransform);
                components.Setup(data, trackObject.GetComponent<TrackObjectView>(),
                    trackObject.GetComponent<TrackObjectSelect>(), trackObject.GetComponent<TrackObjectVisual>(),
                    trackObject.GetComponent<TrackObjectCustomizationController>());
            }


            return components;
        }
    }
}