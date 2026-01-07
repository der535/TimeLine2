using System;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Core.MusicOffset
{
    public class C_MusicOffsetController : MonoBehaviour
    {
        private M_MusicOffsetData _mMusicOffsetService;
        
        private GameEventBus _gameEventBus;
        private Main _main;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, Main main)
        {
            _gameEventBus = gameEventBus;
            _main = main;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SetOffsetEvent data) =>
            {
                _mMusicOffsetService.Value = data.Offset;
                _main.SetTimeInTicks(TimeLineConverter.Instance.TicksCurrentTime());
            });
        }
    }
}