using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeLine.Components
{
    public static class ComponentRules
    {
        private static readonly Dictionary<Type, Rule> _rules = new Dictionary<Type, Rule>
        {
            { typeof(RandomTransformComponent), new Rule(typeof(RandomTransformComponent), maxInstances: 1) },
            { typeof(DynamicTransformСomponent), new Rule(typeof(DynamicTransformСomponent), maxInstances: 1) }
        };

        public static  Dictionary<string, Type>  GetAllComponents(GameObject gameObject)
        {
             Dictionary<string, Type>  components = new  Dictionary<string, Type> ();
            foreach (var rule in _rules)
            {
                if(CanAdd(rule.Key, gameObject))
                    components.Add(rule.Key.Name, rule.Key);
            }
            return components;
        }

        public static bool CanAdd(Type componentType, GameObject target)
        {
            if (!_rules.TryGetValue(componentType, out var rule))
            {
                Debug.LogWarning($"No rule defined for component: {componentType.Name}");
                return false;
            }

            return rule.CanAdd(target);
        }

        public static Component AddComponentSafely(Type componentType, GameObject target)
        {
            if (CanAdd(componentType, target))
            {
                var comp = target.AddComponent(componentType); // AddComponent с Type
                return comp;
                Debug.Log($"✅ Added {componentType.Name} to {target.name}");
            }
            else
            {
                Debug.LogWarning($"❌ Cannot add {componentType.Name} to {target.name}");
                return null;
            }
        }
    }
}