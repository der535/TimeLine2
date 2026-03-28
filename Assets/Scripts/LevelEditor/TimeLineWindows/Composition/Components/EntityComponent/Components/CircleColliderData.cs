using Unity.Entities;
using Unity.Mathematics;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components
{
    public struct CircleColliderData : IComponentData
    {
        public float radius;
        public float3 center;
        public bool isTrigger;
        public bool isDangerous;
    }
}