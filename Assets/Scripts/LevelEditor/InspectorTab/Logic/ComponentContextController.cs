using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.Components;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ContextMenu;
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

        internal void Setup(BaseParameterComponent component, bool isRemoveble)
        {
            button.onClick.AddListener(() =>
            {
                print(ComponentRules.CanAdd(_copyComponentController.GetCopyComponent(), component.gameObject));
                _contextMenuController.Setup(new List<(Action, string, bool)>
                {
                    (() =>
                    {
                        _gameEventBus.Raise(
                            new RemoveComponentEvent(_trackObjectStorage.GetTrackObjectData(component.gameObject),
                                component));
                    }, "Remove component", isRemoveble),
                    (() => { _copyComponentController.Copy(component); }, "Copy Component", true),
                    (() => { _copyComponentController.PasteNewComponent(component.gameObject); },
                        "Past component as new", ComponentRules.CanAdd(_copyComponentController.GetCopyComponent(), component.gameObject)),
                    (() => { _copyComponentController.PasteValues(component.gameObject, component); },
                        "Past component values and animation", _copyComponentController.CompareTypes(component)),
                });

                _contextMenuController.ShowMenu();
            });
        }
    }
}