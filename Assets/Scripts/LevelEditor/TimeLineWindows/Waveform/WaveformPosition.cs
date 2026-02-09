using System;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;
using OldPanEvent = TimeLine.EventBus.Events.Input.OldPanEvent;

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
        private Main _main;
        private TimeLineScroll _timeLineScroll;
        private GameEventBus _gameEventBus;
        private M_MusicData _musicData;

        private float _panOffset;
        private float _scrollOffset;

        [Inject]
        private void Construct(
            MainObjects mainObjects, 
            TimeLineSettings timeLineSettings,
            Main main,
            GameEventBus gameEventBus,
            TimeLineScroll timeLineScroll, M_MusicData musicData)
        {
            _mainObjects = mainObjects;
            _timeLineSettings = timeLineSettings;
            _main = main;
            _timeLineScroll = timeLineScroll;
            _gameEventBus = gameEventBus;
            _musicData = musicData;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref OldPanEvent data) => BuildWaveForm(), -2);
            _gameEventBus.SubscribeTo((ref TimeLineZoomEvent data) => BuildWaveForm(), -2);
            _gameEventBus.SubscribeTo((ref ScrollTimeLineEvent data) => BuildWaveForm(), -2);
            
            _gameEventBus.SubscribeTo(((ref MusicLoadedEvent data) =>
            {
                BuildWaveForm();
            }),-2);
            
            _gameEventBus.SubscribeTo((ref SetOffsetEvent data) =>
            {
                BuildWaveForm();
            });
            
            _gameEventBus.SubscribeTo((ref SetBPMEvent data) =>
            {
                BuildWaveForm();
            }, -2);
        }
        
        internal void BuildWaveForm()
        {
            if(_musicData.music == null) return;

            if (toggle)
            {
                // print((_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan) *
                //     _main.MusicData.music.length * _main.MusicData.bpm / 60);

                waveformRect.sizeDelta =
                    new Vector2(
                        (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom) *
                        _musicData.music.length * _musicData.bpm / 60,
                        waveformRect.rect.height);
            }

            else
            {
                waveformRect.sizeDelta =
                    new Vector2(
                        (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom) *
                        _musicData.music.length * (factor / acur), waveformRect.rect.height);
            }
            
            waveformRect.localPosition =
                new Vector2(
                    (waveformRect.sizeDelta.x / 2) + _mainObjects.ContentRectTransform.offsetMin.x -
                    TimeLineConverter.Instance.BeatPerSecondOffset()*(_timeLineSettings.DistanceBetweenBeatLines +_timeLineScroll.Zoom), 0);
        }
    }
}