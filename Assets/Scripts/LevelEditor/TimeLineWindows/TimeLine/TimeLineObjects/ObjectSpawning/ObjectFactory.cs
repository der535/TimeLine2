using System;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.ECS.Components;
using TimeLine.LevelEditor.GeneralServices;
using TimeLine.LevelEditor.MaxObjectIndex.Controller;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using TimeLine.LevelEditor.TrackObjectSize.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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

        private EntityManager _entityManager;

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
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        /// <summary>
        /// Создаёт объект на сцене с компонентам спрайта
        /// </summary>
        internal void CreateSceneObjectAndAddSprite(Sprite sprite)
        {
            // Создаем сценный объект
            var entity = CreateSceneObject(sprite.name);
            
            _entityManager.AddComponent<SpriteRendererTag>(entity);


            string name = $"{sprite.name} {_maxObjectIndexDataReading.GetNextIndex()}";

            _addAnEntitySprite.SetupSpriteRender(entity, sprite); // сетапаем спрайт
            

            // Создаем трек-объект
            TrackObjectComponents trackObject = CreateTrackObject(_trackObjectSizeReader.GetSize(), name,
                0, TimeLineConverter.Instance.TicksCurrentTime());

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(UniqueIDGenerator.GenerateUniqueID(), name);

            _trackObjectStorage.Add(null, entity, trackObject, branch, UniqueIDGenerator.GenerateUniqueID());
        }

        // internal TrackObjectPacket CreateFullObject(string name)
        // {
        //     string id = UniqueIDGenerator.GenerateUniqueID();
        //
        //     // Создаем сценный объект
        //     var entity = CreateSceneObject();
        //
        //     // Создаем трек-объект
        //     TrackObjectComponents trackObject = CreateTrackObject(_trackObjectSizeReader.GetSize(), name,
        //         0, TimeLineConverter.Instance.TicksCurrentTime());
        //
        //     // Создаем ветку
        //     Branch branch = _branchCollection.AddBranch(id, name);
        //
        //     // Добавляем в хранилище
        //     var TrackObjectData =
        //         _trackObjectStorage.Add(null, entity, trackObject, branch, UniqueIDGenerator.GenerateUniqueID());
        //
        //     // sceneObject.GetComponent<NameComponent>().Name.Value = name;
        //
        //     return TrackObjectData;
        // }

        /// <summary>
        /// Создаёт объект на сцене
        /// </summary>
        /// <returns></returns>
        internal Entity CreateSceneObject(string name)
        {
            //Старая система создание объекта
            // GameObject sceneTrackObject = _container.InstantiatePrefab(_sceneObjectPrefab, _rootSceneObject);

            //Н
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = entityManager.CreateEntity();
            _entityManager.AddComponent<EntityActiveTag>(entity);
            _entityManager.AddComponent<LocalTransform>(entity);
            _entityManager.AddComponent<ObjectPositionOffsetData>(entity);
            _entityManager.AddComponent<PostTransformMatrix>(entity);
            _entityManager.AddComponent<PositionData>(entity);
            _entityManager.AddComponent<RotationData>(entity);
            
            EntityName.SetupName(entity, name); // сетапаем имя

            var transform = LocalTransform.Identity;
            // Вместо пустой матрицы ставим единичную
            _entityManager.SetComponentData(entity, new PostTransformMatrix
            {
                Value = float4x4.identity
            });

            _entityManager.SetComponentData(entity, transform);

            return (entity);
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