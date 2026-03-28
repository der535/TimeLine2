using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using EventBus;
using Newtonsoft.Json;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.EventBus.Events.LevelFinishedEvent;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using TimeLine.TimeLine;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class FinishLevelController : MonoBehaviour
    {
        [SerializeField] private GameObject finishScreen;
        [SerializeField] private PlayModeController playModeController;
        [Space] 
        [SerializeField] private TMP_InputField inputField;
        
        private Main _main;
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        private TimeLineMarkersController _timeLineMarkersController;
        private TimeLineConverter _timeLineConverter;
        private FloatInputValidator _inputValidator;
        private SaveLevel _saveLevel;
        private M_MusicData _musicData;

        private TimeLineMarker _timeLineMarker;

        private bool _levelFinished = false;
        private double _finishTime;

        [Inject]
        private void Constructor(
            GameEventBus gameEventBus, 
            ActionMap actionMap, 
            Main main,
            TimeLineMarkersController timeLineMarkersController, 
            TimeLineConverter timeLineConverter,
            SaveLevel saveLevel, M_MusicData musicData)
        {
            _gameEventBus = gameEventBus;
            _main = main;
            _actionMap = actionMap;
            _timeLineMarkersController = timeLineMarkersController;
            _timeLineConverter = timeLineConverter;
            _saveLevel = saveLevel;
            _musicData = musicData;
            
        }

        internal void SetTime(double value)
        {
            if(_timeLineMarker == null)
                _timeLineMarker = _timeLineMarkersController.AddMarker(0, Color.green);
            
            _finishTime = value;
            
            _timeLineMarker.UpdateTime((float)_finishTime);
            _timeLineMarker.UpdatePosition();
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref MusicLoadedEvent data) =>
            {
                _actionMap.LevelFinished.Disable();
                
                _actionMap.LevelFinished.CloseFinishScreen.started += _ =>
                {
                    PlayerInvulnerable.SetActive(false);
                    finishScreen.SetActive(false);

                    _actionMap.Player.Enable();
                    _actionMap.Editor.Enable();
                    _actionMap.LevelFinished.Disable();

                    playModeController.ExitPlayMode();
                };

                if (_finishTime == 0)
                {
                    SetTime(_timeLineConverter.SecondsToTicks(_musicData.music.length));
                    inputField.text = _finishTime.ToString(CultureInfo.InvariantCulture);
                }

                _inputValidator = new FloatInputValidator(inputField, f => SetTime(f), minValue: 200);
            });
            
            _gameEventBus.SubscribeTo((ref TurnToPlayModeEvent data) => { _levelFinished = false; });
            
            _gameEventBus.SubscribeTo((ref TickExactTimeEvent data) =>
            {
                if (data.Time >= _finishTime && _levelFinished == false)
                {
                    if (!playModeController.IsPlaying) return;
                    _levelFinished = true;
                    FinishLevel();
                }
            });
        }

        internal void Load()
        {
            string path =
                $"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}/FinishData.json";
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                SetTime( JsonConvert.DeserializeObject<float>(json));
            }
           
        }

        internal void Save()
        {
            string path =
                $"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}/FinishData.json";
            string json = JsonConvert.SerializeObject(_finishTime, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private void FinishLevel()
        {
            _gameEventBus.Raise(new LevelFinishedEvent());
            PlayerInvulnerable.SetActive(true);
            finishScreen.SetActive(true);

            _actionMap.Player.Disable();
            _actionMap.Editor.Disable();
            _actionMap.LevelFinished.Enable();
        }
    }
}