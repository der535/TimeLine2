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
        [SerializeField] private Transform sceneObjectParent;
        [Space]
        [SerializeField] private RectTransform rightPanelRectTransform;
        [SerializeField] private RectTransform keyframeRootRectTransform;
        [SerializeField] private RectTransform keyframeScrollView;
        [SerializeField] private VerticalLayoutGroup keyframeVerticalLayoutGroup;
        [SerializeField] private Camera mainCamera;
        
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
        public Transform SceneObjectParent => sceneObjectParent;
        public VerticalLayoutGroup KeyframeVerticalLayoutGroup => keyframeVerticalLayoutGroup;
        
    }
}