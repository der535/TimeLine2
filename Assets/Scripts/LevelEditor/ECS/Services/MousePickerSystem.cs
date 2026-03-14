using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.ECS.Services
{
    public partial class MousePickerSystem : SystemBase
    {
        [SerializeField] private Camera mapCamera;
        
        protected override void OnUpdate()
        {

            
            // 1. Проверяем нажатие мыши (через обычный Input для простоты)
            if (!UnityEngine.Input.GetMouseButtonDown(0)) return;

            // 2. Переводим координаты мыши в мировые (для 2D)
            float3 mouseWorldPos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            mouseWorldPos.z = 0; // Обнуляем Z, так как мы в 2D

            Entity selectedEntity = Entity.Null;

            // 3. Итерируем по всем объектам, которые потенциально можно "тыкнуть"
            // Предположим, у них есть LocalToWorld и какой-то размер (например, твой компонент)
            foreach (var (ltw, entity) in SystemAPI.Query<RefRO<LocalToWorld>>().WithEntityAccess())
            {
                // Получаем инвертированную матрицу объекта
                // Это переносит точку мыши из Мирового пространства в Локальное пространство объекта
                float4x4 worldToLocal = math.inverse(ltw.ValueRO.Value);
                float3 localMousePos = math.transform(worldToLocal, mouseWorldPos);

                // 4. Проверка границ (например, если объект 1x1 метр в локальных координатах)
                // Здесь 0.5f - это половина стандартного размера. 
                // Если у тебя разные размеры, нужно брать их из компонента (например, BoxGeometry)
                float2 halfSize = new float2(0.5f, 0.5f); 

                if (localMousePos.x >= -halfSize.x && localMousePos.x <= halfSize.x &&
                    localMousePos.y >= -halfSize.y && localMousePos.y <= halfSize.y)
                {
                    selectedEntity = entity;
                    // Нашли объект! Можно прервать цикл или искать тот, что "выше" по Z
                    break; 
                }
            }

            if (selectedEntity != Entity.Null)
            {
                Debug.Log($"Выбрана сущность без коллайдера: {selectedEntity}");
                // Передай эту сущность в свой Drawer
            }
        }
    
    }
}