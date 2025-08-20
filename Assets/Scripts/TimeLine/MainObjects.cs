using System;
using EventBus;
using TimeLine.EventBus.Events.Misc;
using UnityEngine;

namespace TimeLine.Installers
{
    [Serializable]
    public class MainObjects
    {
        [SerializeField] private RectTransform canvasRectTransform;
        [SerializeField] private RectTransform contentRectTransform;
        [SerializeField] private Camera mainCamera;

        private GameEventBus _gameEventBus;

        public void Init(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        public RectTransform CanvasRectTransform
        {
            get => canvasRectTransform;
            set => canvasRectTransform = value;
        }
        
        public Camera MainCamera => mainCamera;

        public RectTransform ContentRectTransform => contentRectTransform;

        public void NotifyContentRectChanged()
        {
            if (_gameEventBus != null)
            {
                _gameEventBus.Raise(new ContentRectTransformChangedEvent(contentRectTransform));
            }
            else
            {
                Debug.LogWarning("EventBus is not initialized! ContentRectTransform change not notified.");
            }
        }
    }
}