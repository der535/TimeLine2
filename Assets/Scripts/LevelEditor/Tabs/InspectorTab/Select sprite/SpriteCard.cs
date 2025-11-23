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

        internal void Setup(Sprite spriteCardSO, Action onClick)
        {
            image.sprite = spriteCardSO;
            text.text = spriteCardSO.name;
            if (onClick != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(onClick.Invoke);
            }
        }
    }
}
