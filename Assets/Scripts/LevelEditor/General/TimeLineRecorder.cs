using System;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine.LevelEditor.GeneralEditor
{
    public class TimeLineRecorder : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;
        [Space]
        [SerializeField] private Color activeColor;
        [SerializeField] private Color inactiveColor;

        private bool _isRecording;
        private bool _isRecordingTemp;

        public void TemporaryChangeRecording(bool isRecording)
        {
            _isRecordingTemp = isRecording;
        }

        public bool IsRecording() => _isRecordingTemp && _isRecording;
        
        private void Start()
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (_isRecording)
                {
                    _isRecording = false;
                    buttonImage.color = inactiveColor;
                }
                else
                {
                    _isRecording = true;
                    buttonImage.color = activeColor;
                }
            });
        }
    }
}