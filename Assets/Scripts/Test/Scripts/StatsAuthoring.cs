using TimeLine.Test.Scripts;
using Unity.Entities;
using UnityEngine;

namespace TimeLine
{
    public class StatsAuthoring : MonoBehaviour
    {
        public float Health;
        public float Speed;
        public float Strength;

        class Baker : Baker<StatsAuthoring>
        {
            public override void Bake(StatsAuthoring authoring)
            {
               Entity entity = GetEntity(TransformUsageFlags.None);
               Stats component = new Stats
               {
                   Health = new FloatParameterECS()
                   {
                       Parameter = new ParameterECS()
                       {
                           ComponentName = "Stats",
                           ParameterName = "Health",
                       },
                       Value = 1f
                   },
                   Speed = new FloatParameterECS()
                   {
                       Parameter = new ParameterECS()
                       {
                           ComponentName = "Stats",
                           ParameterName = "Speed",
                       },
                       Value = 1f
                   },
                   Strength = new FloatParameterECS()
                   {
                       Parameter = new ParameterECS()
                       {
                           ComponentName = "Stats",
                           ParameterName = "Strength",
                       },
                       Value = 1f
                   },
               };
               AddComponent(entity,component);
            }
        }
    }
}
