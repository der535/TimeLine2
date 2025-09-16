using System;
using UnityEngine;

namespace TimeLine.Components
{
    public class Rule
    {
        private readonly Type _componentType;

        public int MaxInstances { get; }

        public Rule(Type componentType, int maxInstances = 1, Type requiredComponent = null, Func<GameObject, bool> customCondition = null)
        {
            _componentType = componentType;
            MaxInstances = maxInstances;
        }

        public bool CanAdd(GameObject target)
        {
            if (MaxInstances >= 0)
            {
                int currentCount = target.GetComponents(_componentType).Length;
                if (currentCount >= MaxInstances) return false;
            }

            return true;
        }
    }
}