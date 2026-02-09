using System;
using TimeLine.LevelEditor.GeneralServices;
using TimeLine.LevelEditor.MaxObjectIndex.Controller;
using TimeLine.LevelEditor.TrackObjectSize.Data;
using TimeLine.TimeLine;
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
            IMaxObjectIndexDataReading maxObjectIndexDataReading)
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
        }
        
        /// <summary>
        /// Создаёт объект на сцене с компонентам спрайта
        /// </summary>
        internal TrackObjectData CreateSceneObjectAndAddSprite(Sprite sprite)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            GameObject sceneObject = CreateSceneObject();
            
            var component = sceneObject.AddComponent<SpriteRendererComponent>();
            _container.Inject(component);
            component.Sprite.Value = sprite;

            string name = $"{sprite.name} {_maxObjectIndexDataReading.GetNextIndex()}";
            
            sceneObject.name = name;

            // Создаем трек-объект
            TrackObject trackObject = CreateTrackObject(_trackObjectSizeReader.GetSize(), name, _trackStorage.GetTrackLineByIndex(0), TimeLineConverter.Instance.TicksCurrentTime());

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, name);

            // Добавляем в хранилище
            var TrackObjectData = _trackObjectStorage.Add(sceneObject, trackObject, branch, UniqueIDGenerator.GenerateUniqueID());


            sceneObject.GetComponent<NameComponent>().Name.Value = name;

            return TrackObjectData;
        }
        
        internal TrackObjectData CreateFullObject(string name)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            GameObject sceneObject = CreateSceneObject();

            // Создаем трек-объект
            TrackObject trackObject = CreateTrackObject(_trackObjectSizeReader.GetSize(), name, _trackStorage.GetTrackLineByIndex(0), TimeLineConverter.Instance.TicksCurrentTime());

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, name);

            // Добавляем в хранилище
            var TrackObjectData = _trackObjectStorage.Add(sceneObject, trackObject, branch, UniqueIDGenerator.GenerateUniqueID());
            
            sceneObject.GetComponent<NameComponent>().Name.Value = name;

            return TrackObjectData;
        }
        
        /// <summary>
        /// Создаёт объект на сцене
        /// </summary>
        /// <returns></returns>
        internal GameObject CreateSceneObject()
        {
            GameObject sceneTrackObject = _container.InstantiatePrefab(_sceneObjectPrefab, _rootSceneObject);
            return sceneTrackObject;
        }
        
        /// <summary>
        /// Создает объект трека
        /// </summary>
        /// <param name="ticksLifeTime">Продолжительность трекобжекта</param>
        /// <param name="name">Имя трекобжекта</param>
        /// <param name="trackLine">Номер линии в тамйлане на которов будет создан трекобжект</param>
        /// <param name="startTime">Время начала</param>
        /// <returns></returns>
        internal TrackObject CreateTrackObject(double ticksLifeTime, string name, TrackLine trackLine,
            double startTime)
        {
            TrackObject trackObject = _container
                .InstantiatePrefab(_trackObjectPrefab, trackLine.RectTransform)
                .GetComponent<TrackObject>();

            double actualStartTime = startTime;
            trackObject.Setup(ticksLifeTime, name, trackLine, string.Empty,actualStartTime,0,0);

            return trackObject;
        }
    }
}