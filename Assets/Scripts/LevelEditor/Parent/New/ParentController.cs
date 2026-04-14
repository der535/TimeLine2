using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Parent;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Parent.New
{
    public class ParentController : IInitializable
    {
        private GameEventBus _gameEventBus;
        private ParentView _parentView;

        private TrackObjectPacket _selectedTrackObject;
        private TrackObjectPacket _selectedParent;
        private TrackObjectStorage _trackObjectStorage;
        
        private bool chooseNewParent = false;
        private string preveusParentName = "";

        [Inject]
        private void Constructor(GameEventBus eventBus, ParentView parentView, TrackObjectStorage trackObjectStorage)
        {
            _gameEventBus = eventBus;
            _parentView = parentView;
            _trackObjectStorage = trackObjectStorage;
        }

        public void Initialize()
        {
            return;
            _parentView.SetActivePanel(false);
            
            _parentView.SelectNewParent += () =>
            {
                chooseNewParent = true;
                _parentView.SetMode_ApplyNewParent();
            };

            _parentView.CancelSelectNewParent += () =>
            {
                chooseNewParent = false;
                _parentView.SetMode_SelectNewParent();
            };

            _parentView.ApplyNewParent += () =>
            {
                chooseNewParent = false;
                _parentView.SetMode_SelectNewParent();
                _parentView.SelectObject(_selectedTrackObject.components.Data.Name, _selectedParent.components.Data.Name);
                _selectedTrackObject.components.Data.ParentID = _selectedParent.sceneObjectID;
                _selectedTrackObject.sceneObject.transform.SetParent(_selectedParent.sceneObject.transform);
                _selectedParent = null;
            };

            _parentView.ClearParent += () =>
            {
                _selectedTrackObject.components.Data.ParentID = string.Empty;
                _selectedTrackObject.sceneObject.transform.SetParent(null);
                _parentView.SelectObject(_selectedTrackObject.components.Data.Name, "---");
            };
            
            
            _parentView.SetMode_SelectNewParent();


            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                _parentView.SetActivePanel(true);
                if (!chooseNewParent)
                {
                    _selectedTrackObject = data.Tracks[^1];
                    var parentObject = _trackObjectStorage.FindObjectByID(_selectedTrackObject.components.Data.ParentID);
                    preveusParentName = parentObject != null
                        ? parentObject.components
                            .Data.Name
                        : "---";
                    _parentView.SelectObject(_selectedTrackObject.components.Data.Name, preveusParentName);
                }
                else
                {
                    if (_selectedTrackObject == data.Tracks[^1]) return;
                    _selectedParent = data.Tracks[^1];
                    _parentView.NewParent(preveusParentName, _selectedParent.components.Data.Name);
                }
            });

            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                if (!chooseNewParent)
                {
                    _selectedTrackObject = data.SelectedObjects[^1];
                    var parentObject = _trackObjectStorage.FindObjectByID(_selectedTrackObject.components.Data.ParentID);
                    preveusParentName = parentObject != null
                        ? parentObject.components
                            .Data.Name
                        : "---";
                    _parentView.SelectObject(_selectedTrackObject.components.Data.Name, preveusParentName);
                }
                else
                {
                    if (_selectedTrackObject == data.SelectedObjects[^1]) return;
                    _selectedParent = data.SelectedObjects[^1];
                    _parentView.NewParent(preveusParentName, _selectedParent.components.Data.Name);
                }
            });
            
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                _parentView.SetActivePanel(false);
            });

        }
    }
}