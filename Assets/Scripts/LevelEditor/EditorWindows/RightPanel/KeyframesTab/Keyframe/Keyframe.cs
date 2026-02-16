using System;
using System.Collections.Generic;
using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.Keyframe.AnimationDatas.BoxCollider.Scale;
using TimeLine.Keyframe.AnimationDatas.TransformComponent;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Rotation;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.RadialSunburstDrawer;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Scale;
using TimeLine.LevelEditor.Tabs.InspectorTab.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;

namespace TimeLine.Keyframe
{
    using UnityEngine;

    public class Keyframe
    {
        private bool _isInitialized; // Это реальное место в памяти

        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                // Добавляем лог при изменении
                // UnityEngine.Debug.Log($"[PROPERTY] Смена флага с {Ticks} {_isInitialized} на {value}");
        
                _isInitialized = value; // Записываем в поле, а не в свойство!
            }
        }
        public Action Initialize {  get; set; }
        public double Ticks { get; set; }

        public double OutTangent { get; set; }
        public double InTangent { get; set; }
        public double InWeight { get; set; }
        public double OutWeight { get; set; }
        public InterpolationType Interpolation { get; set; }

        public enum InterpolationType : int
        {
            Linear = 0,
            Bezier = 1,
            Hold = 2
        }

        private AnimationData animationData;

        public Keyframe(double ticks, InterpolationType interpolation,  double outTangent = 0, double inTangent = 0,
            double inWeight = 0.5f, double outWeight = 0.5f)
        {
            Ticks = Mathf.Round((float)ticks);

            Interpolation = interpolation;

            OutTangent = outTangent;
            InTangent = inTangent;
            InWeight = inWeight;
            OutWeight = outWeight;

            Initialize += () =>
            {
                
                if (animationData.initializedNodes != null)
                {
                    foreach (var VARIABLE in animationData.initializedNodes)
                    {
                        VARIABLE.Initialized();
                    }
                }

       
            };
        }

        public void AddData(AnimationData data)
        {
            // Debug.Log(data);
            animationData = data;
        }

        public void Apply(Component target)
        {
            animationData.Apply(target, animationData.GetValue());
        }

        public AnimationData GetData() => animationData;

        public Keyframe Clone()
        {
            Keyframe clone = new Keyframe(Ticks, Interpolation, OutTangent, InTangent, InWeight, OutWeight);
            clone.AddData(animationData.Clone());
            return clone;
        }

        public void Interpolate(Keyframe next, Component target, double t)
        {
            AnimationData currentData = animationData;
            AnimationData nextData = next.animationData;

            if (currentData != null && nextData != null)
            {
                currentData.Interpolate(nextData, t, this, next, Interpolation, target);
            }
            else if (currentData != null)
            {
                currentData.Apply(target, currentData.GetValue());
            }
            else if (nextData != null)
            {
                nextData.Apply(target, nextData.GetValue());
            }
        }

        public KeyframeSaveData ToSaveData()
        {
            return new KeyframeSaveData
            {
                Ticks = Ticks,
                InterpolationType = Interpolation,
                OutTangent = OutTangent,
                InTangent = InTangent,
                InWeight = InWeight,
                OutWeight = OutWeight,
                DataType = animationData?.GetDataType(),
                Data = animationData?.SerializeData(),
                Graph = animationData?.Graph
            };
        }

        public static Keyframe FromSaveData(KeyframeSaveData saveData, OutputLogic logic, List<IInitializedNode> initializedNodes)
        {
            if (saveData == null) return null;

            // Debug.Log($"to save {saveData.InterpolationType}");

            var keyframe = new Keyframe(
                saveData.Ticks,
                saveData.InterpolationType,
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
                    
                    
                    if (saveData.Graph != null)
                    {
                        data.Graph = saveData.Graph;
                        data.Logic = logic;
                        data.initializedNodes = initializedNodes;
                    }

                }
                else
                {
                    Debug.LogWarning($"Unknown AnimationData type: {saveData.DataType}");
                }
            }

            return keyframe;
        }

        // 🔑 Фабрика для создания AnimationData по имени типа
        public static AnimationData CreateAnimationData(string typeName)
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
                nameof(ColorData) => new ColorData(Color.black),

                nameof(XSizeData) => new XSizeData(0),
                nameof(YSizeData) => new YSizeData(0),
                nameof(XOffsetData) => new XOffsetData(0),
                nameof(YOffsetData) => new YOffsetData(0),
                nameof(RadialSunburstMaterial) => new RadialSunburstMaterialColor1(Color.black),
                nameof(RadialSunburstMaterialColor1) => new RadialSunburstMaterialColor1(Color.black),
                nameof(RadialSunburstMaterialColor2) => new RadialSunburstMaterialColor2(Color.black),
                nameof(RadialSunburstMaterialRotationSpeed) => new RadialSunburstMaterialRotationSpeed(0),
                _ => null
            };
        }
    }
}