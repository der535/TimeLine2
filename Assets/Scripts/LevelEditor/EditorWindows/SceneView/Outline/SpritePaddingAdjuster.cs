using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public static class SpritePaddingAdjuster
{
    public static Sprite AdjustSpritePaddingPercent(Sprite originalSprite, float paddingPercent)
    {
        Texture2D originalTex = originalSprite.texture;
    
        // 1. Рассчитываем padding в пикселях на основе процентов
        // paddingPercent = 0.1f (это 10%)
        int paddingX = Mathf.RoundToInt(originalTex.width * paddingPercent);
        int paddingY = Mathf.RoundToInt(originalTex.height * paddingPercent);
    
        // 2. Создаем новую текстуру с увеличенным размером
        int newWidth = originalTex.width + (paddingX * 2);
        int newHeight = originalTex.height + (paddingY * 2);
    
        Texture2D newTex = new Texture2D(newWidth, newHeight);
        newTex.filterMode = originalTex.filterMode;

        // 3. Заполняем прозрачностью
        Color[] clearPixels = new Color[newWidth * newHeight];
        for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = Color.clear;
        newTex.SetPixels(clearPixels);

        // 4. Копируем старую текстуру в центр новой
        Color[] originalPixels = originalTex.GetPixels();
        newTex.SetPixels(paddingX, paddingY, originalTex.width, originalTex.height, originalPixels);
        newTex.Apply();

        // 5. Создаем новый спрайт
        float ppu = originalSprite.pixelsPerUnit;
        return Sprite.Create(
            newTex, 
            new Rect(0, 0, newWidth, newHeight), 
            new Vector2(0.5f, 0.5f), 
            ppu,                     
            0,                       
            SpriteMeshType.FullRect  
        );
    }
}