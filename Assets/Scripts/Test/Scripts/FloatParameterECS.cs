using Unity.Entities;

namespace TimeLine.Test.Scripts
{
    public struct FloatParameterECS : IComponentData
    {
        public ParameterECS Parameter;
        public float Value;
    }
}