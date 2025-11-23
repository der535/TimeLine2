using System;
using System.Collections;
using UnityEngine;

namespace TimeLine
{
    public class PlayerHitAnimation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer playerSprite;
        [SerializeField] private float countCycle;
        [SerializeField] private float transparent;

        internal void Play(float duraction, Action onFinish)
        {
            StartCoroutine(Animation(duraction, onFinish));
        }

        IEnumerator Animation(float duraction, Action onFinish)
        {
            float duractionHalfCycle = duraction / countCycle / 2;
            for (int i = 0; i < countCycle; i++)
            {
                playerSprite.color = new Color(playerSprite.color.r, playerSprite.color.g, playerSprite.color.b, transparent);
                yield return new WaitForSeconds(duractionHalfCycle);
                playerSprite.color = new Color(playerSprite.color.r, playerSprite.color.g, playerSprite.color.b, 1);
                yield return new WaitForSeconds(duractionHalfCycle);
            }
            onFinish.Invoke();
        }
    }
}
