using System;
using UnityEngine;

namespace TimeLine
{
    public class SaveEditorSettings : MonoBehaviour
    {
        private void Save()
        {
            
        }
    }

    [Serializable]
    class EditorSettings
    {
        public float sceneStep;
        public int timeLineStep;
    }
}
