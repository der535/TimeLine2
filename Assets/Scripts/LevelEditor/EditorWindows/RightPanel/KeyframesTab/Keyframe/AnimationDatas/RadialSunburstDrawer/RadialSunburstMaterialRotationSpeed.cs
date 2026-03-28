// using System;
// using Newtonsoft.Json.Linq;
// using TimeLine.Keyframe;
// using TimeLine.TimeLine;
// using TimeLine;
// using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
// using Unity.Mathematics;
// using UnityEngine;
//
// namespace TimeLine.LevelEditor.Tabs.InspectorTab.Keyframe.AnimationDatas.TransformComponent.Position
// {
//     [System.Serializable]
//     public class RadialSunburstMaterialRotationSpeed : AnimationData
//     {
//         public float value;
//
//         public RadialSunburstMaterialRotationSpeed(float value)
//         {
//             this.value = value;
//         }
//
//         public override Type GetComponentType()
//         {
//             return typeof(RadialSunburstMaterial);
//         }
//
//         public override float4 PackDataToFloat4()
//         {
//            return new float4(value,0,0,0);
//         }
//
//         public override AnimationData Clone()
//         {
//             return new RadialSunburstMaterialRotationSpeed(value);
//         }
//
//         public override object GetValue()
//         {
//             return value;
//         }
//
//         public override void SetValue(object value)
//         {
//             if(value is float f) this.value = f;
//             else
//             {
//                 Debug.LogWarning("[TimeLine.Keyframe] Cannot set RadialSunburstMaterialRotationSpeed value to a float");
//             }
//         }
//
//         public override string GetDataType()
//         {
//            return nameof(RadialSunburstMaterialRotationSpeed);
//         }
//
//         public override JObject SerializeData()
//         {
//             return new JObject
//             {
//                 ["RadialSunburstMaterial-rotationspeed"] = JToken.FromObject(value)
//             };
//         }
//
//         public override void DeserializeData(JObject data)
//         {
//             if (data.TryGetValue("RadialSunburstMaterial-rotationspeed", out JToken token))
//             {
//                 value = token.ToObject<float>();
//             }
//         }
//
//         public override void Apply(Component target, float4 value)
//         {
//             if (target is RadialSunburstMaterial component)
//             {
//                 component.RotationSpeed.Value = value.x;
//             }
//         }
//
//         public override void Interpolate(
//             AnimationData other, 
//             double t, 
//             global::TimeLine.Keyframe.Keyframe current, 
//             global::TimeLine.Keyframe.Keyframe next,
//             global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType, Component target)
//         {
//             if (other is not RadialSunburstMaterialRotationSpeed otherPos)
//                 throw new System.ArgumentException("Interpolation requires another RadialSunburstMaterialRotationSpeed.");
//
//             float localT = (float)t;
//             float interpolatedValue = TimeLineConverter.Instance.Interpolate(
//                 value,
//                 otherPos.value,
//                 current,
//                 next,
//                 localT,
//                 interpolationType
//             );
//
//             Apply(target, interpolatedValue);
//         }
//
//         public override void Apply(Component target, object o)
//         {
//             if (target is RadialSunburstMaterial component)
//             {
//                 component.RotationSpeed.Value = (float)o;
//             }
//         }
//     }
// }