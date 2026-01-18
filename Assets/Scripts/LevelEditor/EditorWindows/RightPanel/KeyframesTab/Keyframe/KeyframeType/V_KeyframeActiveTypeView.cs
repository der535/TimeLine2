using UnityEngine;
using UnityEngine.UI;
using System;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeType
{
    public class KeyframeActiveTypeView : MonoBehaviour
    {
        [SerializeField] private Button keyframeButton;
        [SerializeField] private Button bezierButton;

        // События, на которые подпишется Контроллер
        public event Action OnKeyframeSelected;
        public event Action OnBezierSelected;

        private void Awake()
        {
            keyframeButton.onClick.AddListener(() => OnKeyframeSelected?.Invoke());
            bezierButton.onClick.AddListener(() => OnBezierSelected?.Invoke());
        }
    }
}