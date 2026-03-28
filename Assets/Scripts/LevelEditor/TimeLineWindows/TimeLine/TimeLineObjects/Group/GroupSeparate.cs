using System.Collections.Generic;
using System.Linq;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
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
                        trackObject.components.Data.GroupOffset(-group.components.Data.StartTimeInTicks);
                        selectObject.components.Data.GroupOffsetTrack(null);
                        
                        if(trackObject.sceneObject.transform.parent == group.sceneObject.transform)
                            trackObject.sceneObject.transform.SetParent(null);

                        foreach (var node in selectObject.branch.Nodes)
                        {
                            foreach (var node2 in node.Children)
                            {
                                _keyframeTrackStorage.GetTrack(node2)?.SetParent();
                            }
                        }

                        trackObject.components.View.Show();

                        trackObject.components.trackObject.CalculatePosition();
                    }

                    _trackObjectStorage.SeparetaGroup(group);

                    _trackObjectRemover.SingleRemove(group);
                }
            }
        }
        
        internal List<TrackObjectPacket> SeparateSingle(TrackObjectGroup group)
        {
            List<TrackObjectPacket> trackObjects = new List<TrackObjectPacket>();
            
            foreach (var trackObject in group.TrackObjectDatas)
            {
                trackObject.components.Data.GroupOffset(-group.components.Data.StartTimeInTicks);
                trackObject.components.Data.GroupOffsetTrack(null);

                if (trackObject.sceneObject.transform.parent == group.sceneObject.transform)
                    trackObject.sceneObject.transform.SetParent(null);

                foreach (var node in group.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent();
                    }
                }

                trackObject.components.View.Show();
                trackObject.components.trackObject.CalculatePosition();
                trackObjects.Add(trackObject);
            }

            _trackObjectStorage.SeparetaGroup(group);
            _trackObjectRemover.SingleRemove(group);
            return trackObjects;
        }

        public void Separate(TrackObjectGroup trackObjectGroup)
        {
            foreach (var trackObject in trackObjectGroup.TrackObjectDatas)
            {
                trackObject.components.Data.GroupOffset(-trackObjectGroup.components.Data.StartTimeInTicks);
                trackObject.components.Data.GroupOffsetTrack(null);

                
                if(trackObject.sceneObject.transform.parent == trackObject.sceneObject.transform)
                    trackObject.sceneObject.transform.SetParent(null);

                foreach (var node in trackObjectGroup.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent();
                    }
                }

                trackObject.components.View.Show();

                trackObject.components.trackObject.CalculatePosition();
            }
            

            _trackObjectStorage.SeparetaGroup(trackObjectGroup);

            _trackObjectRemover.SingleRemove(trackObjectGroup);
        }
    }
}