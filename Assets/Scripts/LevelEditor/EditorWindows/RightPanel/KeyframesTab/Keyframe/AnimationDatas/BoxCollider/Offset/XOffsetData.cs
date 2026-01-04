using Newtonsoft.Json.Linq;
using TimeLine.TimeLine;
using UnityEngine;

namespace TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset
{
    [System.Serializable]
    public class XOffsetData : AnimationData
    {
        public float value;

        public XOffsetData(float value)
        {
            this.value = value;
        }

        public override AnimationData Clone()
        {
            return new XOffsetData(value);
        }

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object value)
        {
            if(value is float f) this.value = f;
            else
            {
                Debug.LogWarning("[TimeLine.Keyframe] Cannot set XPositionData value to a float");
            }
        }

        public override string GetDataType()
        {
           return nameof(XOffsetData);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["boxCollider-offset-x"] = JToken.FromObject(value)
            };
        }

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("boxCollider-offset-x", out JToken token))
            {
                value = token.ToObject<float>();
            }
        }

        public override AnimationData Interpolate(
            AnimationData other, 
            double t, 
            Keyframe current, 
            Keyframe next, Keyframe.InterpolationType interpolationType)
        {
            if (other is not XOffsetData otherPos)
                throw new System.ArgumentException($"Interpolation requires another {GetType()}.");

            float localT = (float)t;
            float interpolatedValue = TimeLineConverter.Instance.Interpolate(
                value,
                otherPos.value,
                current,
                next,
                localT,
                interpolationType
            );

            return new XOffsetData(interpolatedValue);
        }
        
        
        public override void Apply(GameObject target)
        {
            BoxCollider2DComponent transformComponent = target.GetComponent<BoxCollider2DComponent>();
            transformComponent.OffsetX.Value = value;
        }
    }
}