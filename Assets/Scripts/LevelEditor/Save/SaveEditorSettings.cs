using System;
using EventBus;
using EventBus.Events.Settings;
using TimeLine.LevelEditor.Tabs.SettingTab.Current_time_type;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.LevelEditor.Save
{
    public class SaveEditorSettings : MonoBehaviour
    {
        [FormerlySerializedAs("_editorSettings")] [SerializeField] private EditorSettings editorSettings;
        [FormerlySerializedAs("_gridScene")] [SerializeField] private GridScene gridScene;
        [FormerlySerializedAs("_gridDropDown")] [SerializeField] private GridDropDown gridDropDown;
        [FormerlySerializedAs("_settingDisplayCurrentTime")] [SerializeField] private SettingDisplayCurrentTime settingDisplayCurrentTime;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        void Start()
        {
            _gameEventBus.SubscribeTo((ref ChangeEditorSettingsEvent _) =>
            {
                Save();
            });
        }
        
        internal void Save()
        {
            var (gridSize, gridRotateSize) = gridScene.GetGridSize();
            editorSettings.sceneGrid = gridSize;
            editorSettings.sceneRotate = gridRotateSize;
            editorSettings.timeLineStep = gridDropDown.GetGridSize();
            editorSettings.settingDisplayCurrentTime = settingDisplayCurrentTime.GetSettingDisplayCurrentTime();
            PlayerPrefs.SetString("Editor settings", JsonUtility.ToJson(editorSettings));
        }
        
        internal void Load()
        {
            if(!PlayerPrefs.HasKey("Editor settings")) return;
            
            editorSettings = JsonUtility.FromJson<EditorSettings>(PlayerPrefs.GetString("Editor settings"));
            settingDisplayCurrentTime.SetSettingDisplayCurrentTime(editorSettings.settingDisplayCurrentTime);
            gridScene.SetGridSize(editorSettings.sceneGrid, editorSettings.sceneRotate);
            gridDropDown.SetGridSize(editorSettings.timeLineStep);
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
