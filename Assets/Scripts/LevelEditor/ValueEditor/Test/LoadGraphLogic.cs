using System.Collections.Generic;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.ValueEditor.Save;
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
                    if (keyframe.GetEntityData().Graph != null)
                    {
                        (keyframe.GetEntityData().Logic, keyframe.GetEntityData().initializedNodes)=_saveNodes.LoadLogicOnly(keyframe.GetEntityData().Graph,
                            TypeToDataType.Convert(keyframe.GetEntityData().GetType()));
                        
                    }
                }
            }
        }
        
        public void LoadGraph(List<TrackObjectPacket> trackObjectDatas)
        {
            List<TrackData> findTracks = new List<TrackData>();
            foreach (var trackObject in trackObjectDatas)
            {
                var trackData = _keyframeTrackStorage.GetTracks().Find(x => x.TrackObjectData == trackObject.components.Data);
                findTracks.Add(trackData);
            }
            
            
            foreach (var track in findTracks)
            {
                if (track?.Track.Keyframes != null)
                    foreach (var keyframe in track.Track.Keyframes)
                    {
                        if (keyframe.GetEntityData().Graph != null)
                        {
                            (keyframe.GetEntityData().Logic, keyframe.GetEntityData().initializedNodes) = _saveNodes.LoadLogicOnly(
                                keyframe.GetEntityData().Graph,
                                TypeToDataType.Convert(keyframe.GetEntityData().GetType()),objects:  trackObjectDatas);
                        }
                    }
            }
        }
    }
}