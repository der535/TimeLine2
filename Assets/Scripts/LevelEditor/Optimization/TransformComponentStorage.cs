using System.Collections.Generic;

namespace TimeLine.LevelEditor.Optimization
{
    /// <summary>
    /// Класс для уведомления всех TransformComponent после применении анимации
    /// </summary>
    public static class TransformComponentStorage
    {
        static List<TransformComponent> _components = new();

        /// <summary>
        /// Добавить компонент в список
        /// </summary>
        /// <param name="component">Добовляемый компонент</param>
        internal static void AddComponent(TransformComponent component)
        {
            _components.Add(component);
        }

        /// <summary>
        /// Удалить компонент из списка
        /// </summary>
        /// <param name="component">Убираемый компонент</param>
        internal static void RemoveComponent(TransformComponent component)
        {
            _components.Remove(component);
        }

        /// <summary>
        /// Уведомить всех
        /// </summary>
        internal static void InvokeAllComponents()
        {
            foreach (var component in _components)
            {
                component.ChangeTransform?.Invoke();
            }
        }
        
        //todo уведомлять только тех кто реально изменился
    }
}