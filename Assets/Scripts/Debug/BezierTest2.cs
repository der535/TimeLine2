using NaughtyAttributes;
using UnityEngine;

namespace TimeLine
{
    public class BezierTest2 : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _acnimationCurves;

        [Button]
        private void OnValidate()
        {
            foreach (var keyframe in _acnimationCurves.keys)
            {
                print(
                    $"Value: {keyframe.value}, Time: {keyframe.time}, inTangent {keyframe.inTangent}, outTangent {keyframe.outTangent}, inWeight {keyframe.inWeight}, outWeight {keyframe.outWeight}");
            }
        }
    }
}