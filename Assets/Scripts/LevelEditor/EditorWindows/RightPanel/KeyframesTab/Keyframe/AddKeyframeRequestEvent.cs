using EventBus;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using UnityEngine;

namespace TimeLine.Keyframe
{
    public struct AddKeyframeRequestEvent: IEvent
    {
        public GameObject TargetObject;
        public AnimationData Data;
    }
}