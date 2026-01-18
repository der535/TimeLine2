using System;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
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
        private void Constructor(GameEventBus gameEventBus, Main main, M_MusicOffsetData musicOffsetService)
        {
            _gameEventBus = gameEventBus;
            _main = main;
            _mMusicOffsetService = musicOffsetService;
            
            _gameEventBus.SubscribeTo((ref OpenEditorEvent data) =>
            {
                _mMusicOffsetService.Value = data.LevelInfo.offset;
                _main.SetTimeInTicks(TimeLineConverter.Instance.TicksCurrentTime());
            });
            
            _gameEventBus.SubscribeTo((ref SetOffsetEvent data) =>
            {
                _mMusicOffsetService.Value = data.Offset;
                _main.SetTimeInTicks(TimeLineConverter.Instance.TicksCurrentTime());
            });
        }
    }
}