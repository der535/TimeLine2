// using System;
// using Newtonsoft.Json.Linq;
// using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
// using TimeLine.LevelEditor.ValueEditor;
// using TimeLine.LevelEditor.ValueEditor.Test;
// using TimeLine.TimeLine;
// using Unity.Mathematics;
// using UnityEngine;
//
// namespace TimeLine.Keyframe.AnimationDatas.TransformComponent.Rotation
// {
//     [System.Serializable]
//     public class ZRotationData : AnimationData
//     {
//         public ZRotationData(float value)
//         {
//             Logic = new OutputLogic();
//             Logic.Initialize(DataType.Float);
//             Logic.ManualValues[0] = value;
//             Graph = SaveGraph.ToJson(Logic);
//         }
//
//         public override float4 PackDataToFloat4()
//         {
//             return new float4((float)Logic.GetValue(),0,0,0);
//         }
//
//         public override AnimationData Clone()
//         {
//             return new ZRotationData((float)Logic.GetValue());
//             
//             // todo не всё копируется
//         }
//         
//
//         public override object GetValue()
//         {
//             return (float)Logic.GetValue();
//         }
//         public override void SetValue(object value)
//         {
//             if(value is float f) Logic.ManualValues[0] = f;
//             else
//             {
//                 Debug.LogWarning("[TimeLine.Keyframe] Cannot set XPositionData value to a float");
//             }
//         }
//         
//         public override string GetDataType()
//         {
//             return nameof(ZRotationData);
//         }
//
//         public override JObject SerializeData()
//         {
//             return new JObject
//             {
//                 ["transform-rotation-z"] =  JToken.FromObject((float)Logic.GetValue())
//             };
//         }
//
//         public override void DeserializeData(JObject data)
//         {
//             if (data.TryGetValue("transform-rotation-z", out JToken token))
//             {
//                 Logic.Initialize(DataType.Float);
//                 Logic.ManualValues[0] = token.ToObject<float>();
//                 Graph = SaveGraph.ToJson(Logic);
//             }
//         }
//
//         public override void Apply(Component target, float4 value)
//         {
//             if(target is global::TimeLine.TransformComponent component)
//                 component.ZRotation.Value = value.x;
//         }
//
//         public override void Interpolate(AnimationData other, double t, Keyframe current, Keyframe next, global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType, Component target)
//         {
//             if (other is not ZRotationData otherPos)
//                 throw new System.ArgumentException("Interpolation requires another XPositionData.");
//
//             float localT = (float)t;
//             float interpolatedValue = TimeLineConverter.Instance.Interpolate(
//                 (float)Logic.GetValue(),
//                 (float)otherPos.Logic.GetValue(),
//                 current,
//                 next,
//                 localT,
//                 interpolationType
//             );
//             
//             Apply(target, interpolatedValue);
//         }
//
//         public override Type GetComponentType()
//         {
//             return typeof(global::TimeLine.TransformComponent);
//         }
//         public override void Apply(Component target, object o)
//         {
//             if(target is global::TimeLine.TransformComponent component)
//                 component.ZRotation.Value = (float)o;
//         }
//     }
// }