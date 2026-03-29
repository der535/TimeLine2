using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller
{
    public class ShakeCameraInstaller : IComponentInstaller
    {
        public ComponentNames GetComponentName()
        {
            return ComponentNames.ShakeCamera;
        }

        public void Install(Entity entity)
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            entityManager.AddComponent<ShakeCameraData>(entity);
            entityManager.AddComponentData<ShakeCameraData>(entity, new ShakeCameraData()
            {
                StrengthX = 0.1f,
                StrengthY = 0.1f,
                Duration = 0.1f,
                Vibrato = 5,
                Randomness = 1f,
            });
        }

        public void Remove(Entity entity)
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            entityManager.RemoveComponent<ShakeCameraData>(entity);
        }
    }
}