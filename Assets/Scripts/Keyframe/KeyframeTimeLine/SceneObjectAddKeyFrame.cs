using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class SceneObjectAddKeyFrame : MonoBehaviour
    {
        private TrackObjectStorage _trackObjectStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private BranchCollection _branchCollection;
        private Main _main;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage, 
            KeyframeTrackStorage keyframeTrackStorage,
            BranchCollection branchCollection, 
            Main main)
        {
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
            _branchCollection = branchCollection;
            _main = main;
        }

        public void AddKeyframe(GameObject target, AnimationData data, string componentName, string fieldName)
        {
            TrackObjectData trackObjectData = _trackObjectStorage.GetTrackObjectData(target);

            TreeNode node = _branchCollection.AddNodeToBranch(trackObjectData.branch.ID, trackObjectData.branch.Name,
                componentName, fieldName);

            if (_keyframeTrackStorage.GetTrack(node) == null)
                _keyframeTrackStorage.AddTrack(node, new Track(target, target.name), trackObjectData.trackObject);

            _keyframeTrackStorage.AddKeyframe(node, _main.CurrentTime - trackObjectData.trackObject.StartTime,
                new PositionData(target.transform.position));
        }
    }
}