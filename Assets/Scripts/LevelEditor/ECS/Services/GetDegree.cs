using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.LevelEditor.ECS
{
    public static class GetDegree
    {
        internal static Vector3 FromQuaternion(quaternion rotation)
        {
            // Используем математику Unity.Mathematics для точности
            // math.Euler возвращает радианы, переводим в градусы
            float3 angles = math.degrees(math.Euler(rotation));
            return new Vector3(angles.x, angles.y, angles.z);
        }

        internal static quaternion FromEuler(Vector3 euler)
        {
            // Чтобы корректно задать поворот из ЛЮБЫХ градусов (даже -600 или 1000),
            // переводим их в радианы и создаем кватернион.
            // Математически он будет верным, хотя при чтении позже покажет 120.
            float3 radians = math.radians(new float3(euler.x, euler.y, euler.z));
            
            // Используем Order ZXY, так как это стандарт для Unity
            return quaternion.Euler(radians);
        }
    }
}