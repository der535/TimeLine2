using System;
using TimeLine.LevelEditor.ValueEditor.FieldNodeTest;
using Unity.Entities;

public class ComponentFieldExtractor<T> : IFieldExtractor where T : unmanaged, IComponentData
{
    private readonly Func<T, float> _fieldSelector;

    public ComponentFieldExtractor(Func<T, float> fieldSelector)
    {
        _fieldSelector = fieldSelector;
    }

    public float GetFloatValue(Entity entity, EntityManager em)
    {
        if (!em.HasComponent<T>(entity)) return 0f;
        
        // Достаем компонент целиком (быстро)
        T data = em.GetComponentData<T>(entity);
        
        // Вызываем нашу маленькую функцию, которая знает, какое поле вернуть
        return _fieldSelector(data);
    }
}