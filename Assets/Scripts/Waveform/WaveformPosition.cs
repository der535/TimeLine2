using System;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.Misc;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

namespace TimeLine.Waveform
{
    public class WaveformPosition : MonoBehaviour
    {
        [SerializeField] private RectTransform waveformRect;
        [SerializeField] private WaveformRenderer waveformRenderer;

        [SerializeField] private float factor;
        [SerializeField] private float acur;
        [SerializeField] private bool toggle;
        
        private MainObjects _mainObjects;
        private TimeLineSettings _timeLineSettings;
        private Scroll _scroll;
        private Main _main;
        private TimeLineScroll _timeLineScroll;

        private float _panOffset;
        private float _scrollOffset;

        [Inject]
        private void Construct(
            MainObjects mainObjects, 
            TimeLineSettings timeLineSettings,
            Scroll scroll, 
            Main main,
            GameEventBus gameEventBus,
            TimeLineScroll timeLineScroll)
        {
            _mainObjects = mainObjects;
            _timeLineSettings = timeLineSettings;
            _scroll = scroll;
            _main = main;
            _timeLineScroll = timeLineScroll;
        }

        private void Update()
        {
            BuildWaveForm();
        }
        
        private void BuildWaveForm()
        {
            waveformRect.localPosition =
                new Vector2(
                    (waveformRect.sizeDelta.x / 2) + _mainObjects.ContentRectTransform.offsetMin.x +
                    _main.Offset(), 0);
            if (toggle)
                waveformRect.sizeDelta =
                    new Vector2(
                        (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan) *
                        _main.MusicDataSo.music.length * _main.MusicDataSo.bpm / 60,
                        waveformRect.rect.height);
            else
            {
                waveformRect.sizeDelta =
                    new Vector2(
                        (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan) *
                        _main.MusicDataSo.music.length * (factor / acur), waveformRect.rect.height);
            }
        }
    }
}