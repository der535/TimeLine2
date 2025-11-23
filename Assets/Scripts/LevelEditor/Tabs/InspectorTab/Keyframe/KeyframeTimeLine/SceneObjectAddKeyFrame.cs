using TimeLine.CustomInspector.Logic;
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

        public void AddKeyframe(GameObject target, AnimationData data, string componentName, InspectableParameter fieldName)
        {
            TrackObjectData trackObjectData = _trackObjectStorage.GetTrackObjectData(target);

            TreeNode node = _branchCollection.AddNodeToBranch(trackObjectData.branch.ID, trackObjectData.branch.Name,
                componentName, fieldName.Name);

            if (_keyframeTrackStorage.GetTrack(node) == null)
                _keyframeTrackStorage.AddTrack(node, new Track(target, target.name, fieldName.AnimationColor), trackObjectData.trackObject);

            _keyframeTrackStorage.AddKeyframe(node, _main.TicksCurrentTime() - trackObjectData.trackObject.StartTimeInTicks,
                new PositionData(target.transform.position));
        }
    }
}