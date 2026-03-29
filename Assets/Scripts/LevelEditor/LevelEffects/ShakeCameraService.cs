using DG.Tweening;
using UnityEngine;

namespace TimeLine.LevelEditor.LevelEffects
{
    public static class ShakeCameraService
    {
        public static void Shake(ShakeCameraDataOLD shakeCameraDataOld, Transform cameraTransform, Vector2 shakeStrength, float duration, int vibrato, float randomness)
        {
            // Останавливаем предыдущую тряску, если она была
            shakeCameraDataOld.ShakeTween.Kill();
            shakeCameraDataOld.CurrentOffset = Vector3.zero;

            // Создаем "виртуальную" тряску вектора смещения
            // Мы трясем Vector3, но фактически используем только X и Y
            shakeCameraDataOld.ShakeTween = DOTween.Shake(() => shakeCameraDataOld.CurrentOffset, x => shakeCameraDataOld.CurrentOffset = x, duration, 
                    new Vector3(shakeStrength.x, shakeStrength.y, 0), vibrato, randomness)
                .OnUpdate(() =>
                {
                    // Применяем смещение к оригинальной позиции, игнорируя Z из тряски
                    cameraTransform.position = new Vector3(
                        shakeCameraDataOld.OriginalPos.x + shakeCameraDataOld.CurrentOffset.x,
                        shakeCameraDataOld.OriginalPos.y + shakeCameraDataOld.CurrentOffset.y,
                        shakeCameraDataOld.OriginalPos.z // Z всегда остается константой
                    );
                })
                .OnComplete(() =>
                {
                    // Возвращаем в точный ноль по окончании
                    cameraTransform.position = shakeCameraDataOld.OriginalPos;
                });
        }
    }
}