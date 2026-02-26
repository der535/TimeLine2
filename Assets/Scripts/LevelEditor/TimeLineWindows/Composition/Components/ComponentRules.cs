using System;
using System.Collections.Generic;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using UnityEngine;
using Zenject;

namespace TimeLine.Components
{
    public static class ComponentRules
    {
        private static readonly Dictionary<Type, Rule> Rules = new()
        {
            { typeof(SpriteRendererComponent), new Rule(typeof(SpriteRendererComponent), maxInstances: 1) },
            { typeof(NameComponent), new Rule(typeof(NameComponent), maxInstances: 1) },
            { typeof(TransformComponent), new Rule(typeof(TransformComponent), maxInstances: 1) },
            { typeof(BoxCollider2DComponent), new Rule(typeof(BoxCollider2DComponent), maxInstances: 1) },
            { typeof(CompositionOffset), new Rule(typeof(CompositionOffset), maxInstances: 1) },
            { typeof(CircleCollider2DComponent), new Rule(typeof(CircleCollider2DComponent), maxInstances: 1) },
            { typeof(CapsuleCollider2DComponent), new Rule(typeof(CapsuleCollider2DComponent), maxInstances: 1) },
            { typeof(EdgeCollider2DComponent), new Rule(typeof(EdgeCollider2DComponent), maxInstances: 1) },
            { typeof(ShakeComponent), new Rule(typeof(ShakeComponent), maxInstances: 1) },
            { typeof(PolygonCollider2DComponent), new Rule(typeof(PolygonCollider2DComponent), maxInstances: 1) },
            { typeof(ActiveObjectControllerComponent), new Rule(typeof(ActiveObjectControllerComponent), maxInstances: 1) },
            { typeof(RadialSunburstMaterial), new Rule(typeof(RadialSunburstMaterial), maxInstances: 1, requiredComponent: typeof(SpriteRendererComponent)) },
        };

        public static Dictionary<string, Type> GetAllComponents(GameObject gameObject)
        {
            Dictionary<string, Type> components = new Dictionary<string, Type>();
            foreach (var rule in Rules)
            {
                if (CanAdd(rule.Key, gameObject))
                    components.Add(rule.Key.Name, rule.Key);
            }

            return components;
        }

        public static Type GetComponentType(string componentName)
        {
            if (string.IsNullOrWhiteSpace(componentName))
                return null;

            foreach (var rule in Rules)
            {
                if (string.Equals(rule.Key.Name, componentName, StringComparison.OrdinalIgnoreCase))
                {
                    return rule.Key;
                }
            }

            Debug.LogWarning($"No component type found with name: {componentName}");
            return null;
        }

        public static bool CanAdd(Type componentType, GameObject target)
        {
            // Debug.Log(componentType.Name);
            if (componentType == null || target == null) return false;
            
            if (!Rules.TryGetValue(componentType, out var rule))
            {
                Debug.LogWarning($"No rule defined for component: {componentType.Name}");
                return false;
            }

            return rule.CanAdd(target);
        }

        public static Component AddComponentSafely(Type componentType, GameObject target, DiContainer container)
        {
            if (CanAdd(componentType, target))
            {
                var comp = target.AddComponent(componentType);

                if (container != null)
                {
                    container.Inject(comp); // 👈 внедряем зависимости
                }
                else
                {
                    Debug.Log(container);
                    Debug.LogWarning($"No Zenject container found to inject into {componentType.Name}");
                }

                // Debug.Log($"✅ Added and injected {componentType.Name} to {target.name}");
                return comp;
            }
            else
            {
                Debug.LogWarning($"❌ Cannot add {componentType.Name} to {target.name}");
                return null;
            }
        }

        public static Component AddComponentSafely(string componentName, GameObject target, DiContainer container)
        {
            Type componentType = GetComponentType(componentName);

            if (componentType == null) return null;

            if (CanAdd(componentType, target))
            {
                var comp = target.AddComponent(componentType); // AddComponent с Type
                container.Inject(comp); 
                Debug.Log($"✅ Added {componentType.Name} to {target.name}");
                return comp;
            }
            else
            {
                Debug.LogWarning($"❌ Cannot add {componentType.Name} to {target.name}");
                return null;
            }
        }

        public static Component GetOrAddComponentSafely(Type componentType, GameObject target, DiContainer container)
        {
            if (componentType == null || target == null)
                return null;

            // Сначала попробуем получить существующий компонент
            var existing = target.GetComponent(componentType);
            if (existing != null)
            {
                return existing;
            }

            // Если его нет — попробуем добавить по правилам
            return AddComponentSafely(componentType, target, container);
        }

        public static Component GetOrAddComponentSafely(string componentName, GameObject target, DiContainer container)
        {
            Type componentType = GetComponentType(componentName);
            if (componentType == null)
                return null;

            return GetOrAddComponentSafely(componentType, target, container);
        }
    }
}