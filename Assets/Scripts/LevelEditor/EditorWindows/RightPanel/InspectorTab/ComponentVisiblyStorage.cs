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
        Dictionary<object, bool> components = new();

        internal void SetVisibility(Type o, bool visibility)
        {
            components[o] = visibility; // Всегда обновляет значение
        }

        internal bool? GetVisibility(Type o)
        {
            if (components.ContainsKey(o))
                return components[o];
            return null;
        }
    }
}