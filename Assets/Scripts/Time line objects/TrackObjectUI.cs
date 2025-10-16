using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TimeLine
{
    public class TrackObjectUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image image;
        [SerializeField] private Button button;

        internal void Setup(Sprite sprite, Action onClick)
        {
            text.text = sprite.name;
            image.sprite = sprite;
            button.onClick.AddListener(new UnityAction(onClick));
        }
    }
}
