using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.Components;
using TimeLine.EventBus.Events.TrackObject;
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

        internal void Setup(ComponentNames componentName, Entity entity, bool isRemoveble)
        {
            button.onClick.AddListener(() =>
            {
                _contextMenuController.Setup(new List<(Action, string, bool)>
                {
                    (() =>
                    {
                        _gameEventBus.Raise(
                            new RemoveComponentEvent(_trackObjectStorage.GetTrackObjectData(entity),
                                componentName));
                    }, "Remove component", isRemoveble),
                    (() => { _copyComponentController.Copy(componentName, entity); }, "Copy Component", true),
                    (() => { _copyComponentController.PasteNewComponent(entity); }, "Past component as new", !_copyComponentController.CheckAvailabilityType(componentName)),
                    (() => { _copyComponentController.PasteValues(componentName, entity); }, "Past component values and animation", _copyComponentController.CompareTypes(componentName))
                });

                _contextMenuController.ShowMenu();
            });
        }
    }
}