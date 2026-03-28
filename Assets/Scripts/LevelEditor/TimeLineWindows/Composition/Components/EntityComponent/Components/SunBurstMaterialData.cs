using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components
{
    public struct SunBurstMaterialData : IComponentData
    {
        public Color Color1;
        public Color Color2;
        public int LineCount;
        public float Offset;
        public float TwistFactor;
    }
}