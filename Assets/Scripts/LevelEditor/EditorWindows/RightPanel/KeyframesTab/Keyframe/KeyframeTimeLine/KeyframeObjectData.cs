using System;
using TimeLine.Keyframe;
using UnityEngine;

namespace TimeLine
{
    [Serializable]
    public class KeyframeObjectData : MonoBehaviour
    {
        [SerializeField] private KeyframeDrag keyframeDrag;
        [SerializeField] private KeyframeSelect keyframeSelect;
        [SerializeField] private RectTransform keyframeRect;

        public Keyframe.Keyframe Keyframe { get; set; }
        public Track Track { get; set; }

        public KeyframeDrag KeyframeDrag => keyframeDrag;
        public KeyframeSelect KeyframeSelect => keyframeSelect;
        public RectTransform RectTransform => keyframeRect;
    }
}