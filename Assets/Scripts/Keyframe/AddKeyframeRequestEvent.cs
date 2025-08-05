using EventBus;
using UnityEngine;

namespace TimeLine.Keyframe
{
    public struct AddKeyframeRequestEvent: IEvent
    {
        public GameObject targetObject;
        public AnimationData data;
    }
}