using System.Collections.Generic;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public interface IComponentDrawer
    {
        public void Setup(CustomInspectorDrawer drawer, TrackObjectStorage trackObjectStorage, KeyframeCreator keyframeCreator, ToolsController toolsController);
        /// <summary>
        /// Проверка есть ли в переденных компонентах тот компонент который необходим для отрисовки
        /// </summary>
        /// <param name="component">Список компонентов который есть в объекте</param>
        /// <returns>Результат проверки и список компонентов необходимые для отрисовки</returns>
        public bool GetComponent(List<ComponentType> component);

        public void Draw(Entity target);
    }
}