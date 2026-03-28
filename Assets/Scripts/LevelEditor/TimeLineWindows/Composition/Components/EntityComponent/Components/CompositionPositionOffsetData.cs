using Unity.Entities;
using Unity.Mathematics;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components
{
    public struct CompositionPositionOffsetData : IComponentData
    {
        public float2 Offset;
    }
}