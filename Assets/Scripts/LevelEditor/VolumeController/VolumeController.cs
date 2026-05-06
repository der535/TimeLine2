using System;
using System.Globalization;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine.LevelEditor.VolumeController
{
    public class VolumeController : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TextMeshProUGUI volumeText;
        [SerializeField] private AudioSource audioSource;

        private void Start()
        {
            volumeSlider.maxValue = 100;
            int volume = PlayerPrefs.GetInt("Volume", 100);
            SetVolume(volume);
            volumeSlider.onValueChanged.AddListener((arg0 =>
            {
                SetVolume(Mathf.RoundToInt(arg0));
            }));
        }

        private void SetVolume(int volume)
        {
            volumeSlider.value = volume;
            audioSource.volume = volume / 100f;
            volumeText.text = $"Volume: {Math.Round(volumeSlider.value).ToString(CultureInfo.InvariantCulture)}%";
            PlayerPrefs.SetInt("Volume", volume);
            Debug.Log(PlayerPrefs.GetInt("Volume", 100));
        }
    }
}