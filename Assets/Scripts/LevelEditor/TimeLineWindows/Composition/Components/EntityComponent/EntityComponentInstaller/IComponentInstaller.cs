using Unity.Entities;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller
{
    public interface IComponentInstaller
    {
        public ComponentNames GetComponentName();
        public void Install(Entity entity);
        public void Remove(Entity entity);
    }
}