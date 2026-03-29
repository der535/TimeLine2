using Unity.Entities;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components
{
    public struct ShakeCameraData : IComponentData
    {
        public bool IsInitialized;
        public float StrengthX;
        public float StrengthY;
        public float Duration;
        public int Vibrato;
        public float Randomness;
        
    }
}