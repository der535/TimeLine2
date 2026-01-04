using Newtonsoft.Json.Linq;
using TimeLine.TimeLine;
using UnityEngine;

namespace TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset
{
    [System.Serializable]
    public class ColorData : AnimationData
    {
        public Color value;

        public ColorData(Color value)
        {
            this.value = value;
        }

        public override AnimationData Clone()
        {
            return new ColorData(value);
        }

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object value)
        {
            if(value is Color f) this.value = f;
            else
            {
                Debug.LogWarning("[TimeLine.Keyframe] Cannot set XPositionData value to a float");
            }
        }

        public override string GetDataType()
        {
           return nameof(ColorData);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["sprite-renderer-color"] = JToken.FromObject(value)
            };
        }

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("sprite-renderer-color", out JToken token))
            {
                value = token.ToObject<Color>();
            }
        }

        public override AnimationData Interpolate(
            AnimationData other, 
            double t, 
            Keyframe current, 
            Keyframe next,
            Keyframe.InterpolationType interpolationType)
        {
            if (other is not ColorData otherPos)
                throw new System.ArgumentException($"Interpolation requires another {GetType()}.");

            float localT = (float)t;
            
            float interpolatedAlpha = TimeLineConverter.Instance.Interpolate(
                value.a,
                otherPos.value.a,
                current,
                next,
                localT,
                interpolationType
            );
            float interpolatedGreen = TimeLineConverter.Instance.Interpolate(
                value.g,
                otherPos.value.g,
                current,
                next,
                localT,
                interpolationType
            );
            float interpolatedBlue = TimeLineConverter.Instance.Interpolate(
                value.b,
                otherPos.value.b,
                current,
                next,
                localT,
                interpolationType
            );
            float interpolatedRed = TimeLineConverter.Instance.Interpolate(
                value.r,
                otherPos.value.r,
                current,
                next,
                localT,
                interpolationType
            );
            
            Color result = new Color(interpolatedRed, interpolatedGreen, interpolatedBlue, interpolatedAlpha);

            return new ColorData(result);
        }
        
        
        public override void Apply(GameObject target)
        {
            SpriteRendererComponent component = target.GetComponent<SpriteRendererComponent>();
            // Debug.Log(target);
            // Debug.Log(component);
            component.SpriteColor.Value = value;
        }
    }
}