using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeLine
{
    /// <summary>
    /// Скрипт который отвечает за то будет ле компонент развёрнутый или нет
    /// </summary>
    public class ComponentVisiblyStorage : MonoBehaviour
    {
        Dictionary<string, bool> components = new();

        internal void SetVisibility(string componentName, bool visibility)
        {
            components[componentName] = visibility; // Всегда обновляет значение
        }

        internal bool? GetVisibility(string componentName)
        {
            if (components.ContainsKey(componentName))
                return components[componentName];
            return null;
        }
    }
}