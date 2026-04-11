using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class VolumeController : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TextMeshProUGUI volumeText;
        [SerializeField] private AudioSource audioSource;

        private void Start()
        {
            volumeSlider.onValueChanged.AddListener(arg0 =>
            {
                audioSource.volume = volumeSlider.value;
                volumeText.text = Math.Round(volumeSlider.value * 100).ToString(CultureInfo.InvariantCulture);
            } );
        }
    }
}
