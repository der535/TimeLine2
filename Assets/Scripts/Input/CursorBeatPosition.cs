using System;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.Input
{
    public class CursorBeatPosition : MonoBehaviour
    {
        // [Range(0, 1000)] [SerializeField] private int gridSize;
        [SerializeField] private TimeLineSettings timeLineSettings;
        [FormerlySerializedAs("gridSystem")] [SerializeField] private GridUI gridUI;

        private bool _isActive;

        private Main _main;
        private MainObjects _mainObjects;
        private TimeLineScroll _timeLineScroll;

        [Inject]
        private void Construct(Main main, MainObjects mainObjects, TimeLineScroll timeLineScroll)
        {
            _main = main;
            _timeLineScroll = timeLineScroll;
            _mainObjects = mainObjects;
        }

        public void SetActive(bool isActive) => _isActive = isActive;

        public Vector2 GetCursorPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainObjects.CanvasRectTransform,
                UnityEngine.Input.mousePosition, _mainObjects.MainCamera, out var vector2);

            return vector2;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKey(KeyCode.Mouse0) && _isActive)
            {
               float time = (GetCursorPosition().x - _mainObjects.ContentRectTransform.offsetMin.x) /
                      (timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan) *
                      (60 / _main.MusicDataSo.bpm) * (_main.MusicDataSo.bpm / 60);

               time = gridUI.RoundBeatPositionToGrid(time);
               
               _main.SetTime(time);
            }
        }
    }
}