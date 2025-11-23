using Newtonsoft.Json.Linq;

namespace TimeLine.Keyframe
{
    using UnityEngine;

    [System.Serializable]
    public class RotationData : AnimationData
    {
        public Quaternion rotation;
    
        public RotationData(Quaternion rotation)
        {
            this.rotation = rotation;
        }
        
        public override AnimationData Clone()
        {
            return new RotationData(this.rotation);
        }

        public override object GetValue()
        {
            return rotation;
        }
        public override void SetValue(object value)
        {
            if(value is Quaternion f) this.rotation = f;
            else
            {
                Debug.LogWarning("[TimeLine.Keyframe] Cannot set XPositionData value to a Quaternion");
            }
        }
        
        public override string GetDataType()
        {
            return nameof(PositionData);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["x"] = rotation.x,
                ["y"] = rotation.y,
                ["z"] = rotation.z,
            };
        }

        public override void DeserializeData(JObject data)
        {
            float x = data["x"]?.ToObject<float>() ?? 0f;
            float y = data["y"]?.ToObject<float>() ?? 0f;
            float z = data["z"]?.ToObject<float>() ?? 0f;
            rotation = Quaternion.Euler(x, y, z);
        }
        
        public override AnimationData Interpolate(AnimationData other, double t, Keyframe current, Keyframe next)
        {
            RotationData otherRot = (RotationData)other;
            return new RotationData(Quaternion.Lerp(rotation, otherRot.rotation, (float)t));
        }
    
        public override void Apply(GameObject target)
        {
            target.transform.rotation = rotation;
        }
    }
}