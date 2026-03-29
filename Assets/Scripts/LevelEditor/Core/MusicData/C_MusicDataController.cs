using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Core.MusicData
{
    public class C_MusicDataController : MonoBehaviour
    {
        private M_MusicData _mMusicData;
        
        private GameEventBus _gameEventBus;
        private Main _main;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, Main main, M_MusicData mMusicDataService)
        {
            _mMusicData = mMusicDataService;
            _gameEventBus = gameEventBus;
            _main = main;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SetBPMEvent data) =>
            {
                _mMusicData.bpm = data.BPM;
                _main.SetTimeInTicks(TimeLineConverter.Instance.TicksCurrentTime());
            });

            _gameEventBus.SubscribeTo((ref OpenEditorEvent data) =>
            {
                _mMusicData.bpm = data.LevelInfo.bpm;
            });
            
            _gameEventBus.SubscribeTo((ref MusicLoadedEvent data) =>
            {
                _mMusicData.music = data.audioClip;
            });
        }
    }
}