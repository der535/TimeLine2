using System;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class SpriteCard : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMPro.TextMeshProUGUI text;
        [SerializeField] private Button button;

        internal void Setup(SpriteCardSO spriteCardSO, Action onClick)
        {
            image.sprite = spriteCardSO.sprite;
            text.text = spriteCardSO.name;
            if (onClick != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(onClick.Invoke);
            }
        }
    }
}
