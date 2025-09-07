namespace TimeLine.Keyframe
{
    using UnityEngine;

    [System.Serializable]
    public class PositionData : AnimationData
    {
        public Vector3 position;
    
        public PositionData(Vector3 position)
        {
            this.position = position;
        }

        public override AnimationData Clone()
        {
            return new PositionData(position);
        }

        public override object GetValue()
        {
            return position;
        }
        public override void SetValue(object value)
        {
            if(value is Vector3 f) this.position = f;
            else
            {
                Debug.LogWarning("[TimeLine.Keyframe] Cannot set XPositionData value to a Vector3");
            }
        }
        public override AnimationData Interpolate(AnimationData other, double t)
        {
            PositionData otherPos = (PositionData)other;
            return new PositionData(Vector3.Lerp(position, otherPos.position, (float)t));
        }
    
        public override void Apply(GameObject target)
        {
            target.transform.position = position;
        }
    }
}