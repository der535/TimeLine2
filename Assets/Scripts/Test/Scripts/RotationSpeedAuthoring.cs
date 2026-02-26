using TimeLine.Test.Scripts;
using Unity.Entities;
using UnityEngine;

namespace TimeLine
{
    public class RotationSpeedAuthoring : MonoBehaviour
    {
        public float RotationSpeed;

        class Baker : Baker<RotationSpeedAuthoring>
        {
            public override void Bake(RotationSpeedAuthoring authoring)
            {
               Entity entity = GetEntity(TransformUsageFlags.Dynamic);
               Test.Scripts.RotationSpeed component = new Test.Scripts.RotationSpeed
               {
                   Value = authoring.RotationSpeed
               };
               AddComponent(entity,component);
            }
        }
    }
}
