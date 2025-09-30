using System;
using EventBus;
using TimeLine.EventBus.Events.Misc;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine.Installers
{
    [Serializable]
    public class MainObjects
    {
        [SerializeField] private RectTransform canvasRectTransform;
        [SerializeField] private RectTransform toolRectTransform;
        [SerializeField] private RectTransform contentRectTransform;
        [Space]
        [SerializeField] private RectTransform rightPanelRectTransform;
        [SerializeField] private RectTransform keyframeRootRectTransform;
        [SerializeField] private RectTransform keyframeScrollView;
        [SerializeField] private VerticalLayoutGroup keyframeVerticalLayoutGroup;
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
        public RectTransform ToolRectTransform => toolRectTransform;
        public RectTransform RightPanelRectTransform => rightPanelRectTransform;
        public RectTransform KeyframeRootRectTransform => keyframeRootRectTransform;
        public RectTransform KeyframeScrollView => keyframeScrollView;
        public VerticalLayoutGroup KeyframeVerticalLayoutGroup => keyframeVerticalLayoutGroup;

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