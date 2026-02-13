using System.Collections.Generic;
using System.Linq;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.ValueEditor.Save;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.Test
{
    public class LoadGraphLogic
    {
        private KeyframeTrackStorage _keyframeTrackStorage;
        private SaveNodes _saveNodes;
        private NodeCreator _nodeCreator;

        [Inject]
        private void Construct(KeyframeTrackStorage keyframeTrackStorage, SaveNodes saveNodes, NodeCreator nodeCreator)
        {
            _keyframeTrackStorage = keyframeTrackStorage;
            _saveNodes = saveNodes;
            _nodeCreator = nodeCreator;
        }

        public void LoadGraph()
        {
            foreach (var track in _keyframeTrackStorage.GetTracks())
            {
                foreach (var keyframe in track.Track.Keyframes)
                {
                    if (!string.IsNullOrEmpty(keyframe.GetData().Graph))
                    {
                        (keyframe.GetData().Logic, keyframe.GetData().initializedNodes)=_saveNodes.LoadLogicOnly(keyframe.GetData().Graph,
                            TypeToDataType.Convert(keyframe.GetData().GetType()));
                        
                    }
                }
            }
        }
        
        public void LoadGraph(List<TrackObjectData> trackObjectDatas)
        {
            List<TrackData> findTracks = new List<TrackData>();
            foreach (var trackObject in trackObjectDatas)
            {
                var trackData = _keyframeTrackStorage.GetTracks().Find(x => x.TrackObject == trackObject.trackObject);
                findTracks.Add(trackData);
            }
            
            
            foreach (var track in findTracks)
            {
                if (track?.Track.Keyframes != null)
                    foreach (var keyframe in track.Track.Keyframes)
                    {
                        if (!string.IsNullOrEmpty(keyframe.GetData().Graph))
                        {
                            (keyframe.GetData().Logic, keyframe.GetData().initializedNodes) = _saveNodes.LoadLogicOnly(
                                keyframe.GetData().Graph,
                                TypeToDataType.Convert(keyframe.GetData().GetType()), trackObjectDatas);
                        }
                    }
            }
        }
    }
}