using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
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
    }

    public class EntityComponentRules
    {
        private IComponentInstaller[] _componentInstallers;
        private EntityManager _entityManager;

        private readonly Dictionary<ComponentNames, ComponentType[]> _componentTypes = new()
        {
            {
                ComponentNames.SpriteRenderer, new ComponentType[]
                {
                    typeof(RenderMeshArray),
                    typeof(MaterialMeshInfo),
                    typeof(RenderFilterSettings),
                    typeof(WorldRenderBounds)
                }
            },
            {
                ComponentNames.Transform, new ComponentType[]
                {
                    typeof(LocalTransform)
                }
            }
        };

        EntityComponentRules(SpriteRendererInstaller spriteRendererInstaller)
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _componentInstallers = new IComponentInstaller[] { spriteRendererInstaller };
        }


        public void Save(Entity entity)
        {
            Dictionary<ComponentNames, Dictionary<string, object>> data = new Dictionary<ComponentNames, Dictionary<string, object>>();
            var (componentNames, save) = SaveTransform.Save(entity);
            var (componentNames1, save1) = SaveSpriteRenderer.Save(entity);
            data.Add(componentNames, save);
            data.Add(componentNames1, save1);
            Debug.Log(JsonConvert.SerializeObject(data));
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