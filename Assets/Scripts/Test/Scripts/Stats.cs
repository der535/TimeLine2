using Unity.Entities;

namespace TimeLine.Test.Scripts
{
    public struct Stats : IComponentData
    {
        public FloatParameterECS Health;
        public FloatParameterECS Speed;
        public FloatParameterECS Strength;
    }
}