using Unity.Collections;
using Unity.Entities;

namespace TimeLine.LevelEditor.ECS
{
    public struct NameComponent : IComponentData
    {
        public FixedString64Bytes Value;
    }
}