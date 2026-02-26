using Unity.Collections;
using Unity.Entities;

namespace TimeLine.Test.Scripts
{
    public struct ParameterECS : IComponentData
    {
        public FixedString64Bytes ComponentName;
        public FixedString64Bytes ParameterName;
        
    }
}