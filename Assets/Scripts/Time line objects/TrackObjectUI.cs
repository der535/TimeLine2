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

        internal void Setup(TrackObjectSO trackObject, Action onClick)
        {
            text.text = trackObject.name;
            image.sprite = trackObject.sprite;
            button.onClick.AddListener(new UnityAction(onClick));
        }
    }
}
