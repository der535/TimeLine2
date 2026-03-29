using System.Collections.Generic;
using System.Linq;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent
{
    public enum ComponentNames
    {
        SpriteRenderer,
        NameComponent,
        Transform,
        CompositionOffset,
        BoxCollider,
        CircleCollider,
        PolygonCollider,
        SunBurstMaterial,
        ShakeCamera,
    }

    public class EntityComponentController
    {
        private IComponentInstaller[] _componentInstallers;
        private IEntityComponentSave[] _entityComponentSaves;
        private EntityManager _entityManager;

        /// <summary>
        /// Словарь со списком структур которые есть у определённых компонентов
        /// </summary>
        private readonly Dictionary<ComponentNames, ComponentType[]> _componentTypes = new()
        {
            {
                ComponentNames.SpriteRenderer, new ComponentType[]
                {
                    typeof(RenderMeshArray),
                    typeof(MaterialMeshInfo),
                    typeof(RenderFilterSettings),
                    typeof(WorldRenderBounds),
                    typeof(SpriteRendererTag)
                }
            },
            {
                ComponentNames.Transform, new ComponentType[]
                {
                    typeof(LocalTransform)
                }
            },
            {
                ComponentNames.BoxCollider, new ComponentType[]
                {
                    typeof(PhysicsCollider),
                    typeof(BoxColliderData)
                }
            },
            {
                ComponentNames.CircleCollider, new ComponentType[]
                {
                    typeof(PhysicsCollider),
                    typeof(CircleColliderData)
                }
            },
            {
                ComponentNames.PolygonCollider, new ComponentType[]
                {
                    typeof(PhysicsCollider),
                    typeof(PolygonColliderData)
                }
            },
            {
                ComponentNames.SunBurstMaterial, new ComponentType[]
                {
                    typeof(SunBurstMaterialData)
                }
            },
            {
                ComponentNames.ShakeCamera, new ComponentType[]
                {
                    typeof(ShakeCameraData)
                }
            }
            
        };

        EntityComponentController(
            SpriteRendererInstaller spriteRendererInstaller,
            SunBurstMaterialInstaller sunBurstMaterialInstaller,
            BoxColliderInstaller boxColliderInstaller,
            CircleColliderInstaller circleColliderInstaller,
            PolygoneColliderInstaller polygoneColliderInstaller,
            SaveSpriteRenderer saveSpriteRenderer,
            SaveBoxCollider saveBoxCollider,
            SaveCircleCollider saveCircleCollider,
            SavePolygonCollider savePolygonCollider,
            SaveCompositionOffset saveCompositionOffset,
            SaveSunBurstMaterial saveSunBurstMaterial,
            SaveShakeCamera saveShakeCamera,
            ShakeCameraInstaller shakeCameraInstaller,
            SaveTransform saveTransform) 
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _componentInstallers = new IComponentInstaller[]
            {
                spriteRendererInstaller, 
                boxColliderInstaller, 
                circleColliderInstaller, 
                polygoneColliderInstaller,
                sunBurstMaterialInstaller,
                shakeCameraInstaller
            };
            _entityComponentSaves = new IEntityComponentSave[]
            {
                saveSpriteRenderer, 
                saveTransform, 
                saveBoxCollider, 
                saveCircleCollider, 
                savePolygonCollider,
                saveCompositionOffset,
                saveSunBurstMaterial,
                saveShakeCamera
            };
        }

        /// <summary>
        /// Сохранение данных
        /// </summary>
        /// <param name="entity"></param>
        public Dictionary<ComponentNames, Dictionary<string, object>> Save(Entity entity)
        {
            Dictionary<ComponentNames, Dictionary<string, object>> data =
                new Dictionary<ComponentNames, Dictionary<string, object>>();

            foreach (var componentSaves in _entityComponentSaves)
            {
                var (componentNames, save) = componentSaves.Save(entity);
                if (save != null) data.Add(componentNames, save);
            }

            return data;
        }

        /// <summary>
        /// Загрузка
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dataList"></param>
        public void Load(Entity entity, Dictionary<ComponentNames, Dictionary<string, object>> dataList)
        {
            foreach (var componentSaves in _entityComponentSaves)
            {
                // Debug.Log(dataList.ContainsKey(componentSaves.Check()));
                // Debug.Log(componentSaves.Check());
                if (dataList.ContainsKey(componentSaves.Check()))
                    componentSaves.Load(dataList[componentSaves.Check()], entity);
            }
        }

        /// <summary>
        /// Получить все компоненты которые можно добавить на существо
        /// </summary>
        /// <param name="entity">Проверяемое существо</param>
        /// <returns>Все компоненты которые можно добавить</returns>
        internal List<ComponentNames> GetAllTheComponentsThatCanBeAdded(Entity entity)
        {
            List<ComponentNames> componentNames = new List<ComponentNames>();
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            using NativeArray<ComponentType> types = entityManager.GetComponentTypes(entity);
            foreach (var component in _componentTypes)
            {
                if (!CheckIfComponentTypeInList.Check(types.ToList(), component.Value.ToList()))
                {
                    componentNames.Add(component.Key);
                }
            }

            return componentNames;
        }

        internal bool CheckComponentAvailability(Entity entity, ComponentNames componentNames)
        {
            ComponentType type = _componentTypes[componentNames][0];

            return _entityManager.HasComponent(entity, type);
        }

        internal void AddComponentSafely(ComponentNames componentNames, Entity entity)
        {
            foreach (var componentInstaller in _componentInstallers)
            {
                if (componentInstaller.GetComponentName() == componentNames)
                {
                    componentInstaller.Install(entity);
                    return;
                }
            }
        }

        /// <summary>
        /// Удаляет компонент
        /// </summary>
        /// <param name="componentNames">Название компонента</param>
        /// <param name="entity">Сущность в котором компонент будет удалён</param>
        internal void RemoveComponent(ComponentNames componentNames, Entity entity)
        {
            foreach (var componentInstaller in _componentInstallers)
            {
                if (componentInstaller.GetComponentName() == componentNames)
                {
                    componentInstaller.Remove(entity);
                    return;
                }
            }
        }
    }
}