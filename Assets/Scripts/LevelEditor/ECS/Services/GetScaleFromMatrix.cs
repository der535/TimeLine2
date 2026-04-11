using Unity.Mathematics;

namespace TimeLine.LevelEditor.ECS.Services
{
    public struct GetScaleFromMatrix
    {
        public static float3 Get(float4x4 matrix)
        {
            return new float3(
                math.length(matrix.c0.xyz),
                math.length(matrix.c1.xyz),
                math.length(matrix.c2.xyz)
            );
        }
    }
}