using System.Collections.Generic;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.InspectorTab.Components;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class NameDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer;
        private TrackObjectStorage _trackObjectStorage;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator, ToolsController toolsController, TimeLineRecorder timeLineRecorder)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _trackObjectStorage = trackObjectStorage;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(NameComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            // _customInspectorDrawer.CreateComponent(component,  false);

            if (component is NameComponent componentComponent)
            {
                _customInspectorDrawer.CreateStringField(componentComponent.Name);
            }
        }

        public bool GetComponent(List<ComponentType> component)
        {
            return 
                (
                    CheckIfComponentTypeInList.Check(component, typeof(LevelEditor.ECS.NameComponent))
                );
        }


        public void Draw( Entity target)
        {
            _customInspectorDrawer.CreateComponent(ComponentNames.NameComponent, target, false);

            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;
            
            if (CheckIfComponentTypeInList.Check(target, typeof(LevelEditor.ECS.NameComponent)))
            {
                // 1. Получаем копию данных из сущности (для структур)
                var nameData = manager.GetComponentData<LevelEditor.ECS.NameComponent>(target);
                

                // 2. Рисуем поле, передавая текущее значение
                _customInspectorDrawer.CreateStringField(nameData.Value.ToString(), "Name", (newValue) =>
                {
                    // 3. Когда текст изменился, создаем обновленную структуру
                    var updatedData = new LevelEditor.ECS.NameComponent { Value = newValue };
        
                    _trackObjectStorage.GetTrackObjectData(target).branch.Rename(newValue);
                    _trackObjectStorage.GetTrackObjectData(target).components.View.Rename(newValue);

                    // 4. ОБЯЗАТЕЛЬНО записываем данные обратно в EntityManager
                    // Без этого шага изменения останутся только в локальной переменной
                    manager.SetComponentData(target, updatedData);
                });
            }
        }
    }
}