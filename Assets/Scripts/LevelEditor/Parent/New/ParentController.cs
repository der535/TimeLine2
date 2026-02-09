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

        private TrackObjectData _selectedTrackObject;
        private TrackObjectData _selectedParent;
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
                _parentView.SelectObject(_selectedTrackObject.trackObject.Name, _selectedParent.trackObject.Name);
                _selectedTrackObject.trackObject._parentID = _selectedParent.sceneObjectID;
                _selectedTrackObject.sceneObject.transform.SetParent(_selectedParent.sceneObject.transform);
                _selectedParent = null;
            };

            _parentView.ClearParent += () =>
            {
                _selectedTrackObject.trackObject._parentID = string.Empty;
                _selectedTrackObject.sceneObject.transform.SetParent(null);
                _parentView.SelectObject(_selectedTrackObject.trackObject.Name, "---");
            };
            
            
            _parentView.SetMode_SelectNewParent();


            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                _parentView.SetActivePanel(true);
                if (!chooseNewParent)
                {
                    _selectedTrackObject = data.Tracks[^1];
                    var parentObject = _trackObjectStorage.FindObjectByID(_selectedTrackObject.trackObject._parentID);
                    preveusParentName = parentObject != null
                        ? parentObject.trackObject
                            .Name
                        : "---";
                    _parentView.SelectObject(_selectedTrackObject.trackObject.Name, preveusParentName);
                }
                else
                {
                    if (_selectedTrackObject == data.Tracks[^1]) return;
                    _selectedParent = data.Tracks[^1];
                    _parentView.NewParent(preveusParentName, _selectedParent.trackObject.Name);
                }
            });

            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                if (!chooseNewParent)
                {
                    _selectedTrackObject = data.SelectedObjects[^1];
                    var parentObject = _trackObjectStorage.FindObjectByID(_selectedTrackObject.trackObject._parentID);
                    preveusParentName = parentObject != null
                        ? parentObject.trackObject
                            .Name
                        : "---";
                    _parentView.SelectObject(_selectedTrackObject.trackObject.Name, preveusParentName);
                }
                else
                {
                    if (_selectedTrackObject == data.SelectedObjects[^1]) return;
                    _selectedParent = data.SelectedObjects[^1];
                    _parentView.NewParent(preveusParentName, _selectedParent.trackObject.Name);
                }
            });
            
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                _parentView.SetActivePanel(false);
            });

        }
    }
}