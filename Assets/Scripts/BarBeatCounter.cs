using System;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class BarBeatCounter : MonoBehaviour
    {
        private Main _main;
        private MainObjects _mainObjects;
        private TimeLineSettings _timeLineSettings;
        private Scroll _scroll;
        private GameEventBus _gameEventBus;

        private const float SecondsInMunit = 60f;
        private double _oldBeat;

        [Inject]
        private void Construct(
            Main main,
            MainObjects mainObjects,
            TimeLineSettings timeLineSettings,
            Scroll scroll,
            GameEventBus gameEventBus)
        {
            _main = main;
            _mainObjects = mainObjects;
            _timeLineSettings = timeLineSettings;
            _scroll = scroll;
            _gameEventBus = gameEventBus;
        }

        public void Awake()
        {
            _gameEventBus.SubscribeTo<ExactTimeEvent>(OnTimeChangedUnSmooth);
        }

        public void OnTimeChangedUnSmooth(ref ExactTimeEvent timeEvent)
        {
            float currentTime = timeEvent.Time;
            float beatInSeconds = SecondsInMunit / _main.MusicDataSo.bpm;
            double currentBeat = Math.Ceiling((currentTime) / beatInSeconds);
            if (currentBeat != _oldBeat)
            {
                _oldBeat = currentBeat;
                _gameEventBus.Raise(new BeatEvent((int)currentBeat));
            }
        }

        // public float GetAnchorPositionFromTime(float time)
        // {
        //     return GetAnchorPositionFromBeatPosition(time / (60 / _main.MusicDataSo.bpm)) +
        //            _mainObjects.contentRectTransform.offsetMin.x;
        // }
        //
        // public float GetAnchorPositionFromBeatPosition(float time)
        // {
        //     return time * (_timeLineSettings.DistanceBetweenBeatLines + _scroll.pan);
        // }
    }
}