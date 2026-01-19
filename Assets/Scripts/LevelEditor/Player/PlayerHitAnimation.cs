using System;
using System.Collections;
using UnityEngine;

namespace TimeLine
{
    public class PlayerHitAnimation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer playerSprite; // Спрайт игрока
        [SerializeField] private float countCycle; // Количество циклов мигания
        [SerializeField] private float transparent; // Прозрачность при мигании

        // Метод запуска анимации с колбэком завершения
        internal void Play(float duration, Action onFinish)
        {
            StartCoroutine(Animation(duration, onFinish));
        }

        // Корутина анимации мигания
        IEnumerator Animation(float duration, Action onFinish)
        {
            float durationHalfCycle = duration / countCycle / 2;

            for (int i = 0; i < countCycle; i++)
            {
                // Устанавливаем прозрачность
                playerSprite.color = new Color(playerSprite.color.r, playerSprite.color.g, playerSprite.color.b, transparent);
                yield return new WaitForSeconds(durationHalfCycle);

                // Возвращаем полную непрозрачность
                playerSprite.color = new Color(playerSprite.color.r, playerSprite.color.g, playerSprite.color.b, 1);
                yield return new WaitForSeconds(durationHalfCycle);
            }

            onFinish.Invoke(); // Вызываем колбэк завершения
        }
    }
}