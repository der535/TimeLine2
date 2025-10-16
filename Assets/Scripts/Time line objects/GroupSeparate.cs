using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class GroupSeparate : MonoBehaviour
    {
        private SelectObjectController _selectObjectController;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private TrackObjectStorage _trackObjectStorage;
        private TrackObjectRemover _trackObjectRemover;

        [Inject]
        private void Construct(SelectObjectController selectObjectController, KeyframeTrackStorage keyframeTrackStorage,
            TrackObjectStorage trackObjectStorage, TrackObjectRemover trackObjectRemover)
        {
            _selectObjectController = selectObjectController;
            _keyframeTrackStorage = keyframeTrackStorage;
            _trackObjectStorage = trackObjectStorage;
            _trackObjectRemover = trackObjectRemover;
        }

        public void Separate()
        {
            foreach (var selectObject in _selectObjectController.SelectObjects)
            {
                if (selectObject is TrackObjectGroup group)
                {
                    foreach (var trackObject in group.TrackObjectDatas)
                    {
                        trackObject.trackObject.GroupOffset(-group.trackObject.StartTimeInTicks);
                        
                        if(trackObject.sceneObject.transform.parent == group.sceneObject.transform)
                            trackObject.sceneObject.transform.SetParent(null);

                        foreach (var node in selectObject.branch.Nodes)
                        {
                            foreach (var node2 in node.Children)
                            {
                                _keyframeTrackStorage.GetTrack(node2)?.SetParent();
                            }
                        }

                        trackObject.trackObject.Show();

                        trackObject.trackObject.CalculatePosition();
                    }

                    _trackObjectStorage.SeparetaGroup(group);

                    _trackObjectRemover.SingleRemove(group);
                }
            }
        }

        public void Separate(TrackObjectGroup trackObjectGroup)
        {
            foreach (var trackObject in trackObjectGroup.TrackObjectDatas)
            {
                trackObject.trackObject.GroupOffset(-trackObjectGroup.trackObject.StartTimeInTicks);
                
                if(trackObject.sceneObject.transform.parent == trackObject.sceneObject.transform)
                    trackObject.sceneObject.transform.SetParent(null);

                foreach (var node in trackObjectGroup.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent();
                    }
                }

                trackObject.trackObject.Show();

                trackObject.trackObject.CalculatePosition();
            }

            _trackObjectStorage.SeparetaGroup(trackObjectGroup);

            _trackObjectRemover.SingleRemove(trackObjectGroup);
        }
    }
}