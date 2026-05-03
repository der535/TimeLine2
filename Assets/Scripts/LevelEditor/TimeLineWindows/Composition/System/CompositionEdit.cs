using System.Collections.Generic;
using DG.Tweening;
using EventBus;
using Newtonsoft.Json;
using NUnit.Framework;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ActionHistory;
using TimeLine.LevelEditor.ActionHistory.Commands;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class CompositionEdit : MonoBehaviour
    {
        [SerializeField] private SaveComposition composition;
        [SerializeField] private SaveLevel saveLevel;

        [FormerlySerializedAs("objectSpawner")] [FormerlySerializedAs("trackObjectSpawner")] [SerializeField]
        private FacadeObjectSpawner facadeObjectSpawner;

        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private GroupCreater groupCreater;
        [SerializeField] private GroupSeparate groupSeparate;
        [SerializeField] private TrackObjectRemover trackObjectRemover;
        [SerializeField] private CompositionUpdater compositionUpdater;

        private SetPositionInTimeline _setPositionInTimeline;

        private GameEventBus _gameEventBus;
        private SaveComposition _saveComposition;
        private EventBinder _eventBinder;
        private string _compositionID;
        private string _savedName;

        List<TrackObjectPacket> _editObjects = new();

        [Inject]
        void Construct(GameEventBus gameEventBus, SetPositionInTimeline setPositionInTimeline, SaveComposition saveComposition)
        {
            _gameEventBus = gameEventBus;
            _setPositionInTimeline = setPositionInTimeline;
            _saveComposition = saveComposition;
        }

        internal void Edit(GroupGameObjectSaveData compositionData)
        {
            Debug.Log($"edit called {compositionData.compositionID}");
            _gameEventBus.Raise(new StartCompositionEdit(compositionData));

            _compositionID = compositionData.compositionID;
            _savedName = compositionData.gameObjectName;
            trackObjectStorage.HideAll();

            

            _editObjects = facadeObjectSpawner.LoadObjects(compositionData.children);
            
            double sum = 0;

            foreach (var variable in _editObjects)
            {
                sum += variable.components.Data.GetGlobalTicksPosition();
            }

            sum /= _editObjects.Count;

            _eventBinder = new EventBinder();
            _eventBinder.Add(_gameEventBus, (ref AddTrackObjectDataEvent data) => _editObjects.Add(data.TrackObjectPacket));
            _eventBinder.Add(_gameEventBus, (ref RemoveTrackObjectDataEvent data) => _editObjects.Remove(data.TrackObjectPacket));
            
            _setPositionInTimeline.SetPosition((float)sum);
        }

        public void CancelEditCommand()
        {
            CommandHistory.AddCommand(new CancelCompositionCommand(this, _saveComposition, _compositionID, ""), true); 
        }

        public void CancelEdit()
        {
            _gameEventBus.Raise(new DeselectAllObjectEvent());
            
            _eventBinder.Dispose();
            trackObjectRemover.RemoveList( trackObjectStorage.GetAllActiveTrackData());
            trackObjectStorage.ShowAll();
            
            _gameEventBus.Raise(new EndCompositionEdit());
        }

        public void EndEditCommand()
        {
           CommandHistory.AddCommand(new EndEditCompositionCommand(this, _saveComposition, _compositionID, ""), true); 
        }
        
        public void EndEdit()
        {
            _eventBinder.Dispose();
            TrackObjectGroup trackObjectGroup =
                groupCreater.Create( trackObjectStorage.GetAllActiveTrackData(), _compositionID);
            // trackObjectGroup.sceneObject.GetComponent<NameComponent>().Name.Value = _savedName;
            GroupGameObjectSaveData data = saveLevel.SaveGroup(trackObjectGroup);
            data.gameObjectName = _savedName;
            composition.EditComposition(data, trackObjectGroup.compositionID);
            trackObjectRemover.ListRemove(trackObjectGroup);
            foreach (var ob in  trackObjectStorage.GetAllActiveTrackData())
            {
                trackObjectRemover.SingleRemove(ob);
            }

            compositionUpdater.UpdateCompositions(trackObjectGroup.compositionID);
            
            trackObjectStorage.ShowAll();

            _gameEventBus.Raise(new EndCompositionEdit());
            _gameEventBus.Raise(new DeselectAllObjectEvent());
        }
        
        
    }
}