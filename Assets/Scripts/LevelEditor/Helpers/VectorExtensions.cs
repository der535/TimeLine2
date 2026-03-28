using UnityEngine;

namespace TimeLine.LevelEditor.Helpers
{
    public static class VectorExtensions
    {
        public static Vector3 Divide(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
    }
}