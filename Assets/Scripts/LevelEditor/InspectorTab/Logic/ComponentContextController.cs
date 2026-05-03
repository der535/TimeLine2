using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.Components;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ActionHistory;
using TimeLine.LevelEditor.ActionHistory.Commands;
using TimeLine.LevelEditor.ContextMenu;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.CopyComponent
{
    public class ComponentContextController : MonoBehaviour
    {
        [SerializeField] private Button button;

        private CopyComponentController _copyComponentController;
        private ContextMenuController _contextMenuController;
        private TrackObjectStorage _trackObjectStorage;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(CopyComponentController copyComponentController,
            ContextMenuController contextMenuController, GameEventBus gameEventBus,
            TrackObjectStorage trackObjectStorage)
        {
            _copyComponentController = copyComponentController;
            _contextMenuController = contextMenuController;
            _gameEventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
        }

        /// <summary>
        /// Открывает контекстное меню с возможными взаимодействиями с компонентом
        /// </summary>
        /// <param name="componentName">Название компонента в котором было открыто окно</param>
        /// <param name="entity">Существо с которым идёт взаимодействие</param>
        /// <param name="isRemoveble">Компонент удаляемый?</param>
        internal void Setup(ComponentNames componentName, Entity entity, bool isRemoveble)
        {
            button.onClick.AddListener(() =>
            {
                _contextMenuController.Setup(new List<(Action, string, bool)>
                {
                    (() =>
                    {
                        CommandHistory.AddCommand(new RemoveComponentCommand(
                            _copyComponentController, 
                            componentName, 
                            entity, 
                            _gameEventBus, 
                            _trackObjectStorage, 
                            ""), true);
                        // _gameEventBus.Raise(
                        //     new RemoveComponentEvent(_trackObjectStorage.GetTrackObjectData(entity), componentName));
                    }, "Remove component", isRemoveble),
                    (() => { _copyComponentController.Copy(componentName, entity); }, "Copy Component", true),
                    (() => { CommandHistory.AddCommand(new PastComponentCommand(_copyComponentController, _copyComponentController._copyComponent, entity, _gameEventBus, _trackObjectStorage, ""), true); }, "Past component as new", !_copyComponentController.CheckAvailabilityType(componentName)),
                    (() => { _copyComponentController.PasteValues(componentName, entity); }, "Past component values and animation", _copyComponentController.CompareTypes(componentName))
                });

                _contextMenuController.ShowMenu();
            });
        }
    }
}