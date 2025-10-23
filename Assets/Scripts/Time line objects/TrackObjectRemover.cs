using System.Collections.Generic;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TrackObjectRemover : MonoBehaviour
    {
        private TrackObjectStorage _trackObjectStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private SelectObjectController _selectObjectController;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController)
        {
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
            _selectObjectController = selectObjectController;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.X))
            {
                Remove();
            }
        }


        internal void Remove()
        {
            //[x] Уделить все кейфрем треки 
            //[x] Удалить трек обжект 
            //[x] Удалить объект на сцене
            //[x] Удалить ветку 

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

        private void ListRemove(TrackObjectGroup list)
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