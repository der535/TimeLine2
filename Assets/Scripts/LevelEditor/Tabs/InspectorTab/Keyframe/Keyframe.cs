using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.Keyframe.AnimationDatas.BoxCollider.Scale;
using TimeLine.Keyframe.AnimationDatas.TransformComponent;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Rotation;
using TimeLine.LevelEditor.Tabs.InspectorTab.Keyframe.AnimationDatas.TransformComponent.Position;

namespace TimeLine.Keyframe
{
    using UnityEngine;

    public class Keyframe
    {
        public double Ticks { get; set; }
        
        public double OutTangent { get; set; }
        public double InTangent { get; set; }
        public double InWeight { get; set; }
        public double OutWeight { get; set; }
        public InterpolationType Interpolation  { get; set; }

        public enum InterpolationType
        {
            Linear,
            Bezier,
            Hold
        }
        
        private AnimationData animationData;

        public Keyframe(double ticks, double outTangent = 0, double inTangent = 0, double inWeight = 0.5f, double outWeight = 0.5f)
        {
            this.Ticks = Mathf.Round((float)ticks);
            
            Interpolation = InterpolationType.Linear;

            OutTangent = outTangent;
            InTangent = inTangent;
            InWeight = inWeight;
            OutWeight = outWeight;
        }

        public void AddData(AnimationData data)
        {
            animationData = data;
        }

        public void Apply(GameObject target)
        {
            // Debug.Log(target);
            // Debug.Log(animationData);
            animationData.Apply(target);
        }
        
        public AnimationData GetData() => animationData;

        public Keyframe Clone()
        {
            Keyframe clone = new Keyframe(Ticks, OutTangent, InTangent, InWeight, OutWeight);
            clone.AddData(animationData.Clone());
            return clone;
        }

        public void Interpolate(Keyframe next, GameObject target, double t)
        {
            AnimationData currentData = animationData;
            AnimationData nextData = next.animationData;

            if (currentData != null && nextData != null)
            {
                currentData.Interpolate(nextData, t, this,next, Interpolation).Apply(target);
            }
            else if (currentData != null)
            {
                currentData.Apply(target);
            }
            else if (nextData != null)
            {
                nextData.Apply(target);
            }
        }
        
        public KeyframeSaveData ToSaveData()
        {
            return new KeyframeSaveData
            {
                Ticks = Ticks,
                OutTangent = OutTangent,
                InTangent = InTangent,
                InWeight = InWeight,
                OutWeight = OutWeight,
                DataType = animationData?.GetDataType(),
                Data = animationData?.SerializeData()
            };
        }
        
        public static Keyframe FromSaveData(KeyframeSaveData saveData)
        {
            if (saveData == null) return null;

            var keyframe = new Keyframe(
                saveData.Ticks,
                saveData.OutTangent,
                saveData.InTangent,
                saveData.InWeight,
                saveData.OutWeight
            );

            if (!string.IsNullOrEmpty(saveData.DataType) && saveData.Data != null)
            {
                AnimationData data = CreateAnimationData(saveData.DataType);
                if (data != null)
                {
                    data.DeserializeData(saveData.Data);
                    keyframe.AddData(data);
                }
                else
                {
                    Debug.LogWarning($"Unknown AnimationData type: {saveData.DataType}");
                }
            }

            return keyframe;
        }
        
        // 🔑 Фабрика для создания AnimationData по имени типа
        private static AnimationData CreateAnimationData(string typeName)
        {
            return typeName switch
            {
                // nameof(PositionData) => new PositionData(Vector3.zero),
                nameof(XPositionData) => new XPositionData(0),
                nameof(YPositionData) => new YPositionData(0),
                nameof(XRotationData) => new XRotationData(0),
                nameof(YRotationData) => new YRotationData(0),
                nameof(ZRotationData) => new ZRotationData(0),
                nameof(XScaleData) => new XScaleData(0),
                nameof(YScaleData) => new YScaleData(0),
                
                nameof(XSizeData) => new XSizeData(0),
                nameof(YSizeData) => new YSizeData(0),
                nameof(XOffsetData) => new XOffsetData(0),
                nameof(YOffsetData) => new YOffsetData(0),
                _ => null
            };
        }
    }
}