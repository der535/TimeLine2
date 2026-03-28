using System;
using System.Collections.Generic;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Scale;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using Unity.Entities;

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

        // private AnimationData animationData;
        private EntityAnimationData entityAnimationData;

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
                
                if (entityAnimationData is { initializedNodes: not null })
                {
                    foreach (var variable in entityAnimationData.initializedNodes)
                    {
                        variable.Initialized();
                    }
                }

       
            };
        }
        
        public void AddData(EntityAnimationData data)
        {
            entityAnimationData = data;
        }

        public void Apply(Entity target)
        {
            entityAnimationData.Apply(target, entityAnimationData.GetValue());
        }

        public EntityAnimationData GetEntityData() => entityAnimationData;

        public Keyframe Clone()
        {
            Keyframe clone = new Keyframe(Ticks, Interpolation, OutTangent, InTangent, InWeight, OutWeight);
            clone.AddData(entityAnimationData.Clone());
            return clone;
        }

        public void Interpolate(Keyframe next, Entity target, double t)
        {
            EntityAnimationData currentData = entityAnimationData;
            EntityAnimationData nextData = next.entityAnimationData;

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
                DataType = entityAnimationData?.GetDataType(),
                Data = entityAnimationData?.SerializeData(),
                Graph = entityAnimationData?.Graph
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
                EntityAnimationData data = CreateEntityAnimationData(saveData.DataType);
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
        
        public static EntityAnimationData CreateEntityAnimationData(string typeName)
        {
            return typeName switch
            {
                // nameof(PositionData) => new PositionData(Vector3.zero),
                nameof(EntityXPositionData) => new EntityXPositionData(0),
                nameof(EntityYPositionData) => new EntityYPositionData(0),
                nameof(EntityXRotationData) => new EntityXRotationData(0),
                nameof(EntityYRotationData) => new EntityYRotationData(0),
                nameof(EntityZRotationData) => new EntityZRotationData(0),
                nameof(EntityXScaleData) => new EntityXScaleData(0),
                nameof(EntityYScaleData) => new EntityYScaleData(0),

                _ => null
            };
        }
    }
}