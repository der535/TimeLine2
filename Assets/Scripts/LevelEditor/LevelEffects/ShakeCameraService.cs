using DG.Tweening;
using UnityEngine;

namespace TimeLine.LevelEditor.LevelEffects
{
    public static class ShakeCameraService
    {
        public static void Shake(ShakeCameraData shakeCameraData, Transform cameraTransform, Vector2 shakeStrength, float duration, int vibrato, float randomness)
        {
            // Останавливаем предыдущую тряску, если она была
            shakeCameraData.ShakeTween.Kill();
            shakeCameraData.CurrentOffset = Vector3.zero;

            // Создаем "виртуальную" тряску вектора смещения
            // Мы трясем Vector3, но фактически используем только X и Y
            shakeCameraData.ShakeTween = DOTween.Shake(() => shakeCameraData.CurrentOffset, x => shakeCameraData.CurrentOffset = x, duration, 
                    new Vector3(shakeStrength.x, shakeStrength.y, 0), vibrato, randomness)
                .OnUpdate(() =>
                {
                    // Применяем смещение к оригинальной позиции, игнорируя Z из тряски
                    cameraTransform.position = new Vector3(
                        shakeCameraData.OriginalPos.x + shakeCameraData.CurrentOffset.x,
                        shakeCameraData.OriginalPos.y + shakeCameraData.CurrentOffset.y,
                        shakeCameraData.OriginalPos.z // Z всегда остается константой
                    );
                })
                .OnComplete(() =>
                {
                    // Возвращаем в точный ноль по окончании
                    cameraTransform.position = shakeCameraData.OriginalPos;
                });
        }
    }
}