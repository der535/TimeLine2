using System;
using EventBus;
using EventBus.Events.Settings;
using TimeLine.LevelEditor.Tabs.SettingTab.Current_time_type;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class SaveEditorSettings : MonoBehaviour
    {
        [SerializeField] private EditorSettings _editorSettings;
        [SerializeField] private GridScene _gridScene;
        [SerializeField] private GridDropDown _gridDropDown;
        [SerializeField] private SettingDisplayCurrentTime _settingDisplayCurrentTime;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        void Start()
        {
            _gameEventBus.SubscribeTo<ChangeEditorSettingsEvent>((ref ChangeEditorSettingsEvent data) =>
            {
                Save();
            });
        }
        

        
        internal void Save()
        {
            var (gridSize, gridRotateSize) = _gridScene.GetGridSize();
            _editorSettings.sceneGrid = gridSize;
            _editorSettings.sceneRotate = gridRotateSize;
            _editorSettings.timeLineStep = _gridDropDown.GetGridSize();
            _editorSettings.settingDisplayCurrentTime = _settingDisplayCurrentTime.GetSettingDisplayCurrentTime();
            PlayerPrefs.SetString("Editor settings", JsonUtility.ToJson(_editorSettings));
        }
        
        internal void Load()
        {
            if(!PlayerPrefs.HasKey("Editor settings")) return;
            
            _editorSettings = JsonUtility.FromJson<EditorSettings>(PlayerPrefs.GetString("Editor settings"));
            _settingDisplayCurrentTime.SetSettingDisplayCurrentTime(_editorSettings.settingDisplayCurrentTime);
            _gridScene.SetGridSize(_editorSettings.sceneGrid, _editorSettings.sceneRotate);
            _gridDropDown.SetGridSize(_editorSettings.timeLineStep);
        }
    }

    [Serializable]
    class EditorSettings
    {
        public float sceneGrid;
        public float sceneRotate;
        public int timeLineStep;

        public string settingDisplayCurrentTime;
    }
}
