using TimeLine.EventBus.Events.Player;
using TimeLine.LevelEditor.ECS.Tags;
using TimeLine.LevelEditor.Player;
using Unity.Entities;

namespace TimeLine.LevelEditor.ECS.System
{
    public partial struct HitSignalSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Создаем EntityCommandBuffer, чтобы безопасно удалить компонент-тег после обработки
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // Мы ищем сущности, у которых есть HitEventTag
            foreach (var (_, entity) in SystemAPI.Query<RefRO<HitEventTag>>().WithEntityAccess())
            {
                if (PlayerInvulnerable.IsInvulnerable() == false)
                    ECSServiceLocator.Instance.GameEventBus.Raise(new PlayerHitEvent());

                // 2. Помечаем тег на удаление, чтобы анимация не срабатывала каждый кадр
                ecb.RemoveComponent<HitEventTag>(entity);
            }

            // Выполняем накопленные команды (удаление тегов)
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}