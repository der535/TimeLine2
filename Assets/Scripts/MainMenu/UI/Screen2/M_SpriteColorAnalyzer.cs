using UnityEngine;
using System.Collections.Generic;

public static class M_SpriteColorAnalyzer
{
    public static Color GetDominantColor(Sprite sprite)
    {
        Texture2D texture = sprite.texture;
        Rect rect = sprite.textureRect;

        // Получаем пиксели только из области спрайта (важно, если на одной текстуре много спрайтов/атлас)
        Color[] pixels = texture.GetPixels(
            (int)rect.x, 
            (int)rect.y, 
            (int)rect.width, 
            (int)rect.height
        );

        Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

        foreach (Color color in pixels)
        {
            // Пропускаем полностью прозрачные пиксели
            if (color.a < 0.1f) continue;

            // Округляем цвет для более точной группировки (необязательно, но полезно)
            Color roundedColor = RoundColor(color);

            if (colorCounts.ContainsKey(roundedColor))
                colorCounts[roundedColor]++;
            else
                colorCounts[roundedColor] = 1;
        }

        // Находим цвет с максимальным количеством упоминаний
        Color dominant = Color.white;
        int maxCount = 0;

        foreach (var pair in colorCounts)
        {
            if (pair.Value > maxCount)
            {
                maxCount = pair.Value;
                dominant = pair.Key;
            }
        }

        return dominant;
    }

    // Вспомогательный метод для группировки похожих оттенков
    private static Color RoundColor(Color c)
    {
        return new Color(
            Mathf.Round(c.r * 10f) / 10f,
            Mathf.Round(c.g * 10f) / 10f,
            Mathf.Round(c.b * 10f) / 10f,
            1f
        );
    }
    
    public static Color GetCustomizedColor(Color originalColor, float customS, float customV)
    {
        float h, s, v;
    
        // 1. Переводим RGB в HSV, чтобы достать Hue (h)
        Color.RGBToHSV(originalColor, out h, out s, out v);

        // 2. Создаем новый RGB цвет, используя старый H и ваши S и V
        // customS и customV должны быть в диапазоне от 0.0f до 1.0f
        Color finalColor = Color.HSVToRGB(h, customS, customV);
    
        return finalColor;
    }
}