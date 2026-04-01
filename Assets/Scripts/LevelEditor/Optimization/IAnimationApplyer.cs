using Unity.Entities;
using Unity.Mathematics;

public interface IAnimationApplyer

{
    public void Apply(Entity target, float4 value);
}