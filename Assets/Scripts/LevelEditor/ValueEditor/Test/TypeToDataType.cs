using System;
using UnityEngine;

namespace TimeLine.LevelEditor.ValueEditor.Test
{
    public static class TypeToDataType
    {
        public static DataType Convert(string typeName)
        {
            Type t = Type.GetType(typeName);

            // Если тип не найден, возвращаем значение по умолчанию или кидаем ошибку
            if (t == null) return DataType.Float; 

            return t switch
            {
                _ when t == typeof(float)  => DataType.Float,
                _ when t == typeof(int)    => DataType.Int,
                _ when t == typeof(string) => DataType.String,
                _ when t == typeof(Color) => DataType.Color,
                _                              => DataType.Float
            };
        }
        
        public static DataType Convert(Type typeName)
        {
            // Если тип не найден, возвращаем значение по умолчанию или кидаем ошибку
            if (typeName == null) return DataType.Float; 

            return typeName switch
            {
                _ when typeName == typeof(float)  => DataType.Float,
                _ when typeName == typeof(int)    => DataType.Int,
                _ when typeName == typeof(string) => DataType.String,
                _ when typeName == typeof(Color) => DataType.Color,
                _                              => DataType.Float
            };
        }
    }
}