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
        
        public ObjectFactory(
            Main main,
            DiContainer container,
            Transform rootSceneObject,
            TrackStorage trackStorage,
            GameObject sceneObjectPrefab,
            GameObject trackObjectPrefab,
            BranchCollection branchCollection,
            TrackObjectStorage trackObjectStorage)
        {
            _main = main;
            _container = container;
            _rootSceneObject = rootSceneObject;
            _sceneObjectPrefab = sceneObjectPrefab;
            _trackObjectPrefab = trackObjectPrefab;
            _trackStorage = trackStorage;
            _branchCollection = branchCollection;
            _trackObjectStorage = trackObjectStorage;
        }
        
        /// <summary>
        /// Создаёт объект на сцене с компонентам спрайта
        /// </summary>
        internal void CreateSceneObjectAndAddSprite(Sprite sprite)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            // Создаем сценный объект
            GameObject sceneObject = CreateSceneObject();
            
            var component = sceneObject.AddComponent<SpriteRendererComponent>();
            _container.Inject(component);
            component.Sprite.Value = sprite;
            
            sceneObject.name = sprite.name;

            // Создаем трек-объект
            TrackObject trackObject = CreateTrackObject(100, sprite.name, _trackStorage.GetTrackLineByIndex(0));

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, sprite.name);

            // Добавляем в хранилище
            _trackObjectStorage.Add(sceneObject, trackObject, branch, UniqueIDGenerator.GenerateUniqueID());

            sceneObject.GetComponent<NameComponent>().Name.Value = sprite.name;
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
            double startTime = -1)
        {
            TrackObject trackObject = _container
                .InstantiatePrefab(_trackObjectPrefab, trackLine.RectTransform)
                .GetComponent<TrackObject>();

            double actualStartTime = startTime >= 0 ? startTime : _main.TicksCurrentTime();
            trackObject.Setup(ticksLifeTime, name, trackLine, string.Empty,actualStartTime);

            return trackObject;
        }
    }
}