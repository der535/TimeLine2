using Unity.Transforms;

namespace TimeLine.LevelEditor.ValueEditor.FieldNodeTest
{
    public class SetupAnimationDataResolver
    {
        // Регистрируем разные поля разных компонентов под понятными именами
        SetupAnimationDataResolver()
        {
            AnimationDataResolver.RegisterField<LocalTransform>("Transform.Position.X", d => d.Position.x);
            AnimationDataResolver.RegisterField<LocalTransform>("Transform.Position.Y", d => d.Position.y);
        }
    }
}