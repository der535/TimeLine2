using TimeLine;
using TimeLine.LevelEditor.ECS.Components;
using TimeLine.LevelEditor.ECS.Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))] // Важно: работаем после того, как физика просчиталась
public partial struct PhysicsEventSystem : ISystem
{
    // Lookups позволяют проверять компоненты внутри Job
   [ReadOnly] private ComponentLookup<PlayerTag> _playerLookup;
   [ReadOnly] private ComponentLookup<DangerousObjectTag> _enemyLookup;

    public void OnCreate(ref SystemState state)
    {
        _playerLookup = state.GetComponentLookup<PlayerTag>(true);
        _enemyLookup = state.GetComponentLookup<DangerousObjectTag>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _playerLookup.Update(ref state);
        _enemyLookup.Update(ref state);

        var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        var ecbSingleton = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        // 1. ЗАПУСКАЕМ ТРИГГЕРЫ (Вход в зону)
        state.Dependency = new TriggerJob
        {
            PlayerLookup = _playerLookup,
            EnemyLookup = _enemyLookup,
            _ecb = ecb
        }.Schedule(simulation, state.Dependency);

        // 2. ЗАПУСКАЕМ КОЛЛИЗИИ (Физический удар)
        state.Dependency = new CollisionJob
        {
            PlayerLookup = _playerLookup,
            EnemyLookup = _enemyLookup,
            _ecb = ecb
        }.Schedule(simulation, state.Dependency);
    }

    // --- JOB ДЛЯ ТРИГГЕРОВ ---
    [BurstCompile]
    struct TriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<PlayerTag> PlayerLookup;
        [ReadOnly] public ComponentLookup<DangerousObjectTag> EnemyLookup;

        public EntityCommandBuffer _ecb;
        
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity a = triggerEvent.EntityA;
            Entity b = triggerEvent.EntityB;

            // ПРОВЕРКА: Участвует ли в этом столкновении Игрок?
            bool isPlayerA = PlayerLookup.HasComponent(a);
            bool isPlayerB = PlayerLookup.HasComponent(b);
        
            // ПРОВЕРКА: Участвует ли в этом столкновении Враг?
            bool isEnemyA = EnemyLookup.HasComponent(a);
            bool isEnemyB = EnemyLookup.HasComponent(b);

            // ЛОГИКА: Нас интересует только столкновение Игрока с Врагом
            if ((isPlayerA && isEnemyB) || (isPlayerB && isEnemyA))
            {
                Entity playerEntity = isPlayerA ? a : b;
                // Подаем сигнал: вешаем компонент на игрока
                _ecb.AddComponent<HitEventTag>(playerEntity);
            }
        }


    }

    // --- JOB ДЛЯ КОЛЛИЗИЙ ---
    [BurstCompile]
    struct CollisionJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<PlayerTag> PlayerLookup;
        [ReadOnly] public ComponentLookup<DangerousObjectTag> EnemyLookup;

        public EntityCommandBuffer _ecb;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity a = collisionEvent.EntityA;
            Entity b = collisionEvent.EntityB;

            // ПРОВЕРКА: Участвует ли в этом столкновении Игрок?
            bool isPlayerA = PlayerLookup.HasComponent(a);
            bool isPlayerB = PlayerLookup.HasComponent(b);
        
            // ПРОВЕРКА: Участвует ли в этом столкновении Враг?
            bool isEnemyA = EnemyLookup.HasComponent(a);
            bool isEnemyB = EnemyLookup.HasComponent(b);

            // ЛОГИКА: Нас интересует только столкновение Игрока с Врагом
            if ((isPlayerA && isEnemyB) || (isPlayerB && isEnemyA))
            {
                Entity playerEntity = isPlayerA ? a : b;
                // Подаем сигнал: вешаем компонент на игрока
                _ecb.AddComponent<HitEventTag>(playerEntity);
            }
        }
    }
}