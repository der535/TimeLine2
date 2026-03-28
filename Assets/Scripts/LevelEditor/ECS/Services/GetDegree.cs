using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.LevelEditor.ECS
{
    public static class GetDegree
    {
        internal static Vector3 FromQuaternion(quaternion rotation)
        {
            // Явно указываем, что используем UnityEngine.Quaternion
            // Это превратит данные из ECS в формат, который понимает Transform
            UnityEngine.Quaternion unityRot = rotation;

            // Если IDE не видит .eulerAngles, значит есть проблема с ссылками на UnityEngine.CoreModule
            // Но это свойство ЕСТЬ у каждого UnityEngine.Quaternion с 2005 года.
            return unityRot.eulerAngles;
        }

        internal static quaternion FromEuler(Vector3 euler)
        {
            // 1. Конвертируем Vector3 (градусы) в float3 (радианы)
            float3 radians = math.radians(new float3(euler.x, euler.y, euler.z));

            // 2. Вызываем метод именно у структуры quaternion
            // Используйте EulerZXY, так как это стандарт для Unity ECS
            return quaternion.EulerZXY(radians); 
        }
    }
}