namespace TimeLine.Keyframe
{
    using UnityEngine;

    [System.Serializable]
    public class TintIntData : AnimationData
    {
        public int value;
    
        public TintIntData(int value)
        {
            this.value = value;
        }
    
        public override AnimationData Interpolate(AnimationData other, float t)
        {
            TintIntData otherPos = (TintIntData)other;
            return new TintIntData((int)Mathf.Lerp(value, otherPos.value, t));
        }
    
        public override void Apply(GameObject target)
        {
            // target.GetComponent<TintComponent>().Tint = value;
        }
    }
}