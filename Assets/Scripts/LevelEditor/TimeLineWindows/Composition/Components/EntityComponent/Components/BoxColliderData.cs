using Unity.Entities;
using Unity.Mathematics;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components
{
    public struct BoxColliderData: IComponentData
    {
        public float3 boxSize;
        public float3 boxCenter;
        public bool isTrigger;
        public bool isDangerous;
    }
}