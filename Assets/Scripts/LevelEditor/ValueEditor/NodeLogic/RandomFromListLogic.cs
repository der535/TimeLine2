using System.Collections.Generic;
using TimeLine.LevelEditor.ValueEditor;
using UnityEngine;

public class RandomFromListLogic : NodeLogic
{
    public RandomFromListLogic()
    {
        // По умолчанию создаем один вход
        InputDefinitions = new()
        {
            ("Input 0", DataType.Float)
        };
        OutputDefinitions = new()
        {
            ("Result", DataType.Float)
        };
    }
    

    public override object GetValue(int outputIndex = 0)
    {
        List<float> pool = new List<float>();

        for (int i = 0; i < InputDefinitions.Count; i++)
        {
            // 1. Передаем 0f (float) вместо 0 (int)
            object rawValue = GetInputValue(i, 0f); 
        
            // 2. Используем безопасную конвертацию
            float val = System.Convert.ToSingle(rawValue);
        
            pool.Add(val);
        }

        if (pool.Count == 0) return 0f;

        int randomIndex = Random.Range(0, pool.Count);
        return pool[randomIndex];
    }
}