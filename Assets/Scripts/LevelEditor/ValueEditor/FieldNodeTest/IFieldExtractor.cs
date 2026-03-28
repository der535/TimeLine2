using Unity.Entities;

namespace TimeLine.LevelEditor.ValueEditor.FieldNodeTest
{
    public interface IFieldExtractor
    {
        // Возвращает float, потому что для анимаций и графиков это основной тип
        float GetFloatValue(Entity entity, EntityManager em);
    }
}