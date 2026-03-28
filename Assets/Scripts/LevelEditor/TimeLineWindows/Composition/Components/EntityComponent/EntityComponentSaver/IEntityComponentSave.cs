using System.Collections.Generic;
using TimeLine.LevelEditor.Save;
using Unity.Entities;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver
{
    public interface IEntityComponentSave
    {
        public (ComponentNames componentNames, Dictionary<string, object> save) Save(Entity entity);
        public  ComponentNames Check();
        public void Load(Dictionary<string, object> data, Entity target);
    }
}