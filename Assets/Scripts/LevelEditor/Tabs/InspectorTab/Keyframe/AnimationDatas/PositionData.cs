// using Newtonsoft.Json.Linq;
//
// namespace TimeLine.Keyframe
// {
//     using UnityEngine;
//
//     [System.Serializable]
//     public class PositionData : AnimationData
//     {
//         public Vector3 position;
//     
//         public PositionData(Vector3 position)
//         {
//             this.position = position;
//         }
//
//         public override AnimationData Clone()
//         {
//             return new PositionData(position);
//         }
//
//         public override object GetValue()
//         {
//             return position;
//         }
//         public override void SetValue(object value)
//         {
//             if(value is Vector3 f) this.position = f;
//             else
//             {
//                 Debug.LogWarning("[TimeLine.Keyframe] Cannot set XPositionData value to a Vector3");
//             }
//         }
//         public override string GetDataType()
//         {
//             return nameof(PositionData);
//         }
//
//         // ✅ ИСПРАВЛЕНО: ручная сериализация Vector3
//         public override JObject SerializeData()
//         {
//             return new JObject
//             {
//                 ["x"] = position.x,
//                 ["y"] = position.y,
//                 ["z"] = position.z
//             };
//         }
//
//         // ✅ ИСПРАВЛЕНО: ручная десериализация Vector3
//         public override void DeserializeData(JObject data)
//         {
//             float x = data["x"]?.ToObject<float>() ?? 0f;
//             float y = data["y"]?.ToObject<float>() ?? 0f;
//             float z = data["z"]?.ToObject<float>() ?? 0f;
//             position = new Vector3(x, y, z);
//         }
//         
//         public override AnimationData Interpolate(AnimationData other, double t, Keyframe current, Keyframe next, global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType)
//         {
//             PositionData otherPos = (PositionData)other;
//             return new PositionData(Vector3.Lerp(position, otherPos.position, (float)t));
//         }
//     
//         public override void Apply(GameObject target)
//         {
//             target.transform.position = position;
//         }
//     }
// }