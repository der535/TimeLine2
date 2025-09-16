using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeLine
{
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