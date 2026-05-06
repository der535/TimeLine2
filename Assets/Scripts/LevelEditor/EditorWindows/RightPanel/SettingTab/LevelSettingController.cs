using System.Globalization;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.LevelEditor.Core.MusicOffset;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.SettingTab
{
    public class LevelSettingController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _offset;
        [SerializeField] private TMP_InputField _bpm;
        
        private GameEventBus _gameEventBus;
        private M_MusicOffsetData _musicOffsetData;
        private M_MusicData _musicData;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, M_MusicOffsetData musicOffsetData, M_MusicData musicData)
        {
            _gameEventBus = gameEventBus;
            _musicOffsetData = musicOffsetData;
            _musicData = musicData;
        }
        

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref MusicLoadedEvent _) =>
            {
                _offset.text = _musicOffsetData.Value.ToString(CultureInfo.InvariantCulture);
                _bpm.text = _musicData.bpm.ToString(CultureInfo.InvariantCulture);

                FloatInputValidator floatInputValidator1 = new FloatInputValidator(_offset, (value) =>
                {
                    _gameEventBus.Raise(new SetOffsetEvent(value));
                }, 0);
                FloatInputValidator floatInputValidator2 = new FloatInputValidator(_bpm, (value) =>
                {
                    _gameEventBus.Raise(new SetBPMEvent(value));
                });
            });

        }
    }
}