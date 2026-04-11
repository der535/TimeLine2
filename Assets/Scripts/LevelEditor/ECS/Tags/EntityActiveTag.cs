using Unity.Entities;

// Этот интерфейс позволяет включать/выключать компонент без его удаления
namespace TimeLine.LevelEditor.ECS.Components
{
    public struct EntityActiveTag : IComponentData, IEnableableComponent 
    {
        public bool IsActive;
    }
}