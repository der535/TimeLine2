using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframeCreater : MonoBehaviour
    {
        TrackObjectStorage _trackObjectStorage;
        BranchCollection _branchCollection;
        KeyframeTrackStorage _keyframeTrackStorage;
        Main _main;
        
        [Inject]
        private void Construct(TrackObjectStorage trackObjectStorage, BranchCollection branchCollection, KeyframeTrackStorage keyframeTrackStorage, Main main)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;
            _keyframeTrackStorage = keyframeTrackStorage;
            _main = main;
        }
        
        public void CreateKeyframe(AnimationData animationData, GameObject target, string componentName, string fieldName)
        {
            TrackObjectData trackObjectData = _trackObjectStorage.GetTrackObjectData(target);
        
            TreeNode node = _branchCollection.AddNodeToBranch(trackObjectData.branch.ID, trackObjectData.branch.Name,
                componentName, fieldName);
        
            if (_keyframeTrackStorage.GetTrack(node) == null)
                _keyframeTrackStorage.AddTrack(node, new Track(target, target.name),
                    trackObjectData.trackObject);
            
            _keyframeTrackStorage.AddKeyframe(node, _main.TicksCurrentTime() - trackObjectData.trackObject.StartTimeInTicks,
                animationData);
        }
    }
}
