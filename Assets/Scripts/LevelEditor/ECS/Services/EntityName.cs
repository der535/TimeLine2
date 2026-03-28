using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using Unity.Entities;

namespace TimeLine.LevelEditor.ECS
{
    public static class EntityName
    {
        public static void SetupName(Entity entity, string name)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;
            
            manager.AddComponentData(entity, new NameComponent { Value = name });
        }
    }
}