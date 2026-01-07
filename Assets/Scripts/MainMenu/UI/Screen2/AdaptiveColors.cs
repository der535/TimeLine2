using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TimeLine.MainMenu.UI.Screen2
{
    public class AdaptiveColors : MonoBehaviour
    {
        [SerializeField] private List<ObjectPaintingData> datas;
        [Space] [SerializeField] private Sprite sprite;

        [Button]
        private void Paint()
        {
            Color dominantColor = M_SpriteColorAnalyzer.GetDominantColor(sprite);
            foreach (var data in datas)
            {
                foreach (var item in data.items)
                {
                    Color targetColor =
                        M_SpriteColorAnalyzer.GetCustomizedColor(dominantColor, data.customS, data.customV);
                    if (string.IsNullOrEmpty(data.shaderPropertyName))
                        item.color = targetColor;
                    else
                    {
                        Material instance = new Material(item.material);
                        instance.SetColor(data.shaderPropertyName, targetColor);
                        item.material = instance;
                    }
                }
            }
        }
    }

    [Serializable]
    public class ObjectPaintingData
    {
        public List<Image> items;
        [Space] public float customS;
        public float customV;
        [Space] public string shaderPropertyName;
    }
}