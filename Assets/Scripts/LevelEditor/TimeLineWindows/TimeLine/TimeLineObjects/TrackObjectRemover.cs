using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TrackObjectRemover : MonoBehaviour
    {
        [SerializeField] private WindowsFocus windowsFocus;
        
        private TrackObjectStorage _trackObjectStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private SelectObjectController _selectObjectController;
        private ActionMap _actionMap;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController,
            ActionMap actionMap,
            GameEventBus gameEventBus)
        {
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
            _selectObjectController = selectObjectController;
            _actionMap = actionMap;
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _actionMap.Editor.X.started += _ => Remove();
        }


        internal void Remove()
        {
            if(!windowsFocus.IsFocused) return;
            
            foreach (var select in _selectObjectController.SelectObjects)
            {
                if (select is TrackObjectGroup group)
                {
                    ListRemove(group);
                }
                else
                {
                    SingleRemove(select);
                }
            }

            _gameEventBus.Raise(new DeselectAllObjectEvent());
        }

        internal void SingleRemove(TrackObjectData select)
        {
            foreach (var nodes in select.branch.Nodes)
            {
                _keyframeTrackStorage.RemoveTrack(nodes);
            }

            Destroy(select.trackObject.gameObject);
            Destroy(select.sceneObject);

            _trackObjectStorage.Remove(select);
        }
        
        internal void SingleRemoveNoStorage(TrackObjectData select)
        {
            foreach (var nodes in select.branch.Nodes)
            {
                _keyframeTrackStorage.RemoveTrack(nodes);
            }

            Destroy(select.trackObject.gameObject);
            Destroy(select.sceneObject);
        }

        
        internal void SingleRemove(TrackObjectGroup select)
        {
            foreach (var nodes in select.branch.Nodes)
            {
                _keyframeTrackStorage.RemoveTrack(nodes);
            }

            Destroy(select.trackObject.gameObject);
            Destroy(select.sceneObject);
            
            _trackObjectStorage.Remove(select);
        }

        internal void ListRemove(TrackObjectGroup list)
        {
            foreach (var item in list.TrackObjectDatas)
            {
                if (item is TrackObjectGroup group)
                {
                    ListRemove(group);
                    SingleRemove(item);
                }
                else
                {
                    SingleRemove(item);
                }
            }
            SingleRemove(list);
        }
    }
}