using UnityEngine;

namespace TimeLine
{
    public static class CalculatePixel
    {
        public static float Calculate(float pixelCount, Transform transform, Camera camera)
        {
            // Получаем расстояние от камеры до объекта (для ортографической камеры это не важно, но для перспективы — критично)
            float distance = Vector3.Distance(transform.position, camera.transform.position);

            // Для ортографической камеры:
            if (camera.orthographic)
            {
                // В ортографической камере 1 юнит = orthographicSize * 2 / Screen.height пикселей по вертикали
                float pixelsPerUnit = Screen.height / (camera.orthographicSize * 2f);
                return pixelCount / pixelsPerUnit;
            }
            else
            {
                // Для перспективной камеры используем ScreenPointToRay и расстояние
                Vector3 upper = camera.ViewportToWorldPoint(new Vector3(0, 0.5f + pixelCount / (2f * Screen.height), distance));
                Vector3 lower = camera.ViewportToWorldPoint(new Vector3(0, 0.5f - pixelCount / (2f * Screen.height), distance));
                return Vector3.Distance(upper, lower);
            }
        }
    }
}
