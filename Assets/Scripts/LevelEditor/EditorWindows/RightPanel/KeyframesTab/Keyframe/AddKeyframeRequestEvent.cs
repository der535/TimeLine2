using EventBus;
using UnityEngine;

namespace TimeLine.Keyframe
{
    public struct AddKeyframeRequestEvent: IEvent
    {
        public GameObject TargetObject;
        public AnimationData Data;
    }
}