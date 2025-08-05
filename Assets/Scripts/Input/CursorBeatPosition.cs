using TimeLine.Installers;
using UnityEngine;
using Zenject;

namespace TimeLine.Input
{
    public class CursorBeatPosition : MonoBehaviour
    {
        [SerializeField] private TimeLineSettings timeLineSettings;

        private bool _isActive;
        
        private Main _main;
        private Scroll _scroll;
        private MainObjects _mainObjects;
        private TimeLineScroll _timeLineScroll;

        [Inject]
        private void Construct(Main main, Scroll scroll, MainObjects mainObjects, TimeLineScroll timeLineScroll)
        {
            _main = main;
            _scroll = scroll;
            _timeLineScroll = timeLineScroll;
            _mainObjects = mainObjects;
        }

        public void SetActive(bool isActive) => _isActive = isActive;

        private void Update()
        {
            if (UnityEngine.Input.GetKey(KeyCode.Mouse0) && _isActive)
                _main.SetTime(
                    (UnityEngine.Input.mousePosition.x - _mainObjects.ContentRectTransform.offsetMin.x -
                     _mainObjects.CanvasRectTransform.sizeDelta.x / 2) /
                    (timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan) *
                    (60 / _main.MusicDataSo.bpm) * (_main.MusicDataSo.bpm / 60));
        }
    }
}