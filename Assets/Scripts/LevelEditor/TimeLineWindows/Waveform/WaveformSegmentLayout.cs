// WaveformSegmentLayout.cs - Добавляем к родительскому объекту

using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class WaveformSegmentLayout : LayoutGroup
{
    public override void CalculateLayoutInputHorizontal() {}
    public override void CalculateLayoutInputVertical() {}
    
    [Button]
    public override void SetLayoutHorizontal()
    {
        if(transform.childCount == 0) return;
        
        // Распределяем дочерние объекты равномерно
        float segmentWidth = rectTransform.rect.width / transform.childCount;
        // Debug.Log(segmentWidth);
        for(int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = (RectTransform)transform.GetChild(i);
            SetChildAlongAxis(child, 0, segmentWidth * i, segmentWidth);
        }
    }
    [Button]

    public override void SetLayoutVertical()
    {
        // Заполняем всю высоту
        for(int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = (RectTransform)transform.GetChild(i);
            SetChildAlongAxis(child, 1, 0, rectTransform.rect.height);
        }
    }
}