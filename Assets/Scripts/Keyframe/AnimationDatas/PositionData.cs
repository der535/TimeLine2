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