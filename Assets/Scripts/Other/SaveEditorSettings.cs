using System;
using UnityEngine;

namespace TimeLine
{
    public class SaveEditorSettings : MonoBehaviour
    {
        [SerializeField] private EditorSettings _editorSettings;
        [SerializeField] private GridScene _gridScene;
        [SerializeField] private GridDropDown _gridDropDown;
        
        internal void Save()
        {
            var (gridSize, gridRotateSize) = _gridScene.GetGridSize();
            _editorSettings.sceneGrid = gridSize;
            _editorSettings.sceneRotate = gridRotateSize;
            _editorSettings.timeLineStep = _gridDropDown.GetGridSize();
            PlayerPrefs.SetString("Editor settings", JsonUtility.ToJson(_editorSettings));
        }
        
        internal void Load()
        {
            if(!PlayerPrefs.HasKey("Editor settings")) return;
            
            _editorSettings = JsonUtility.FromJson<EditorSettings>(PlayerPrefs.GetString("Editor settings"));
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
    }
}
