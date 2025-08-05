using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TimeLine
{
    public class TabButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image image;

        internal void Setup(Action onClick)
        {
            button.onClick.AddListener(new UnityAction(onClick));
        }

        internal void Selected()
        {
            image.color = Color.grey;
        }

        internal void Deselected()
        {
            image.color = Color.black;
        }
    }
}
