using System;
using System.Collections.Generic;
using TimeLine.LevelEditor.ValueEditor.FieldNodeTest;
using Unity.Entities;

public static class AnimationDataResolver
{
    // Ключ - это строка (идентификатор поля, например "Player.PosX")
    private static Dictionary<string, IFieldExtractor> _registry = new();

    public static void RegisterField<T>(string id, Func<T, float> selector) where T : unmanaged, IComponentData
    {
        _registry[id] = new ComponentFieldExtractor<T>(selector);
    }

    public static float GetValue(string id, Entity entity, EntityManager em)
    {
        if (_registry.TryGetValue(id, out var extractor))
        {
            return extractor.GetFloatValue(entity, em);
        }
        return 0f;
    }
}