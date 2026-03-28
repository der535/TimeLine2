using System.Globalization;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.InspectorTab.Components
{
    public class CompositionOffset : MonoBehaviour
    {
        [SerializeField] private TMP_InputField x;
        [SerializeField] private TMP_InputField y;

        private Entity _selectedEntity;

        private GameEventBus _gameEventBus;

        private bool update = true;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                if (data.Tracks[^1] is TrackObjectGroup group)
                {
                    EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    _selectedEntity = data.Tracks[^1].entity;

                    if (entityManager.HasBuffer<Child>(_selectedEntity))
                    {
                        // Получаем буфер (массив) детей
                        DynamicBuffer<Child> children = entityManager.GetBuffer<Child>(_selectedEntity);

                        ObjectPositionOffsetData offsetData =
                            entityManager
                                .GetComponentData<ObjectPositionOffsetData>(children[0].Value); // Получаем компонент оффсета

                        update = false;
                        x.text = offsetData.Offset.x.ToString(CultureInfo.InvariantCulture);
                        y.text = offsetData.Offset.y.ToString(CultureInfo.InvariantCulture);
                        update = true;
                    }
                }
            });

            FloatInputValidator floatInputValidator = new FloatInputValidator(x, (value) =>
            {
                if (update == false) return;
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                
                CompositionPositionOffsetData compositionPositionOffsetData = entityManager.GetComponentData<CompositionPositionOffsetData>(_selectedEntity);
                compositionPositionOffsetData.Offset.x = value;
                entityManager.SetComponentData(_selectedEntity, compositionPositionOffsetData);
                
                if (entityManager.HasBuffer<Child>(_selectedEntity))
                {
                    // Получаем буфер (массив) детей
                    DynamicBuffer<Child> children = entityManager.GetBuffer<Child>(_selectedEntity);

                    ObjectPositionOffsetData offsetData =
                        entityManager
                            .GetComponentData<ObjectPositionOffsetData>(children[0].Value); // Получаем компонент оффсета


                    offsetData.Offset.x = value;
                    GetChildrenExample(_selectedEntity, offsetData.Offset);
                }
            });

            FloatInputValidator floatInputValidator2 = new FloatInputValidator(y, (value) =>
            {
                if (update == false) return;
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                CompositionPositionOffsetData compositionPositionOffsetData = entityManager.GetComponentData<CompositionPositionOffsetData>(_selectedEntity);
                compositionPositionOffsetData.Offset.x = value;
                entityManager.SetComponentData(_selectedEntity, compositionPositionOffsetData);

                if (entityManager.HasBuffer<Child>(_selectedEntity))
                {
                    // Получаем буфер (массив) детей
                    DynamicBuffer<Child> children = entityManager.GetBuffer<Child>(_selectedEntity);
                    Debug.Log(children.Length);

                    ObjectPositionOffsetData offsetData =
                        entityManager
                            .GetComponentData<ObjectPositionOffsetData>(children[0].Value); // Получаем компонент оффсета


                    offsetData.Offset.y = value;
                    GetChildrenExample(_selectedEntity, offsetData.Offset);
                }
            });
        }

        private void GetChildrenExample(Entity parentEntity, float2 newOffset)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            // Проверяем, есть ли у сущности вообще дети
            if (entityManager.HasBuffer<Child>(parentEntity))
            {
                // Получаем буфер (массив) детей
                DynamicBuffer<Child> children = entityManager.GetBuffer<Child>(parentEntity);

                for (int i = 0; i < children.Length; i++)
                {
                    Entity childEntity = children[i].Value; //Получаем существо

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
        }
    }
}