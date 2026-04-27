using System.Collections.Generic;
using System.Linq;
using TimeLine.Keyframe;
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
        _Empty_ = 0,
        SpriteRenderer = 1,
        NameComponent = 2,
        Transform = 3,
        CompositionOffset = 4,
        BoxCollider = 5,
        CircleCollider= 6,
        PolygonCollider= 7,
        SunBurstMaterial= 8,
        ShakeCamera= 9,
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
                    typeof(BoxColliderData),
                    typeof(ColliderTag)
                }
            },
            {
                ComponentNames.CircleCollider, new ComponentType[]
                {
                    typeof(PhysicsCollider),
                    typeof(CircleColliderData),
                    typeof(ColliderTag)
                }
            },
            {
                ComponentNames.PolygonCollider, new ComponentType[]
                {
                    typeof(PhysicsCollider),
                    typeof(PolygonColliderData),
                    typeof(ColliderTag)
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

        // Принимаем списки интерфейсов вместо 15 конкретных классов
        EntityComponentController(
            List<IComponentInstaller> installers,
            List<IEntityComponentSave> savers) 
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _componentInstallers = installers.ToArray();
            _entityComponentSaves = savers.ToArray();
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
        /// Сохранение конкретного компонента
        /// </summary>
        /// <param name="entity"></param>
        public Dictionary<string, object> Save(Entity entity, ComponentNames componentName)
        {
            foreach (var componentSaves in _entityComponentSaves)
            {
                if (componentSaves.Check() == componentName)
                {
                    var (_, save) = componentSaves.Save(entity);
                    if (save != null)  return save;
                }
            }

            return null;
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
                // 1. Проверяем, нет ли уже такого же компонента
                bool alreadyHas = CheckIfComponentTypeInList.Check(types.ToList(), component.Value.ToList());
        
                // 2. Проверяем на конфликты (например, ColliderTag)
                bool hasConflict = IsConflict(component.Key, types);

                if (!alreadyHas && !hasConflict)
                {
                    componentNames.Add(component.Key);
                }
            }

            return componentNames;
        }
        
        // Пример логики исключения
        private bool IsConflict(ComponentNames componentToAdd, NativeArray<ComponentType> existingTypes)
        {
            // Список всех типов коллайдеров
            var colliderTypes = new List<ComponentNames> { 
                ComponentNames.BoxCollider, 
                ComponentNames.PolygonCollider, 
                ComponentNames.CircleCollider 
            };

            // Если мы пытаемся добавить коллайдер
            if (colliderTypes.Contains(componentToAdd))
            {
                // Проверяем, есть ли уже на сущности какой-либо коллайдер или ColliderTag
                foreach (var type in existingTypes)
                {
                    string typeName = type.GetManagedType().Name;
                    // Если в списке компонентов есть ColliderTag или любой из коллайдеров
                    if (typeName.Contains("Collider")) return true; 
                }
            }
            return false;
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
        /// <param name="keyframeTrackStorage">Необходимый клайсс для удаления ключевых кадров связанные с этим компонентом</param>
        /// <param name="entity">Сущность в котором компонент будет удалён</param>
        internal void RemoveComponent(ComponentNames componentNames, KeyframeTrackStorage keyframeTrackStorage,  Entity entity)
        {
            foreach (var track in keyframeTrackStorage.GetTracks().ToList())
            {
                if(track.Track.ComponentNames == componentNames)
                    keyframeTrackStorage.RemoveTrackWithNode(track.Track);
            }
            
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