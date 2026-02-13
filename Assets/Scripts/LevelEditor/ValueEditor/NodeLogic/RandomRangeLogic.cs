using TimeLine.LevelEditor.ValueEditor;
using UnityEngine;


public class RandomRangeLogic : NodeLogic
{
    public RandomRangeLogic()
    {
        InputDefinitions = new()
        {
            ("Min", DataType.Float), // Индекс 0
            ("Max", DataType.Float) // Индекс 1
        };

        OutputDefinitions = new()
        {
            ("Result", DataType.Float) // Индекс 0
        };
    }

    public override object GetValue(int outputIndex = 0)
    {
        // 1. Пытаемся получить Min. Если в порту нет провода, берем ручной ввод.
        float min = (float)GetInputValue(0, 0);

        // 2. Пытаемся получить Max.
        float max = (float)GetInputValue(1, 0);


        // 3. Возвращаем случайное число в этом диапазоне
        return Random.Range(min, max);
    }
}