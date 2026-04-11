using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Scale;
using UnityEngine;

namespace TimeLine.LevelEditor.ValueEditor.Test
{
    public static class TypeToDataType
    {
        public static DataType Convert(string typeName)
        {
            Type t = Type.GetType(typeName);

            // Если тип не найден, возвращаем значение по умолчанию или кидаем ошибку
            if (t == null)
            {
                Debug.Log($"Нихуя не найдено {typeName}");
                return DataType.Float;
            }

            return t switch
            {
                _ when t == typeof(float) => DataType.Float,
                _ when t == typeof(int) => DataType.Int,
                _ when t == typeof(string) => DataType.String,
                _ when t == typeof(Color) => DataType.Color,
                _ => DataType.Float
            };
        }

        public static DataType Convert(Type typeName)
        {
            // Если тип не найден, возвращаем значение по умолчанию или кидаем ошибку
            if (typeName == null) return DataType.Float;

            return typeName switch
            {
                _ when typeName == typeof(float) => DataType.Float,
                _ when typeName == typeof(int) => DataType.Int,
                _ when typeName == typeof(string) => DataType.String,
                _ when typeName == typeof(Color) => DataType.Color,
                _ => DataType.Float
            };
        }
        
        public static DataType Convert(JObject jObject)
        {
            List<EntityAnimationData> entityAnimationDatas = new List<EntityAnimationData>();
            entityAnimationDatas.Add(new EntityXPositionData(0));
            entityAnimationDatas.Add(new EntityYPositionData(0));
            entityAnimationDatas.Add(new EntityXRotationData(0));
            entityAnimationDatas.Add(new EntityYRotationData(0));
            entityAnimationDatas.Add(new EntityZRotationData(0));
            entityAnimationDatas.Add(new EntityXScaleData(0));
            entityAnimationDatas.Add(new EntityYScaleData(0));
            entityAnimationDatas.Add(new EntitySpriteRendererColor(Color.white));
            
            foreach (var typeName in entityAnimationDatas)
            {
                var type = typeName.GetValueType(jObject);
                if (type != null)
                {

                    return (DataType)type;
                }
            }
            Debug.Log("Нихуя не найдено");
            Debug.Log(jObject);
            return DataType.Float;
        }
    }
}