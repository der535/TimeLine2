using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class ObjectSettings : MonoBehaviour
    {
        private GameEventBus _gameEventBus;

        [Inject]
        public void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void OnMouseDown()
        {
            _gameEventBus.Raise(new SelectSceneObject(gameObject));
            // _settingPanel.Select(gameObject.transform, AddKeyframe);
            print("Select");
        }

        // public void AddKeyframe()
        // {
        //     TrackObjectData trackObjectData = _trackObjectStorage.GetTrackObjectData(gameObject);
        //
        //     TreeNode node = _vBranchCollection.AddNodeToBranch(trackObjectData.branch.ID, trackObjectData.branch.Name,
        //         "Transform", "Position");
        //
        //     if (_keyframeTrackStorage.GetTrack(node) == null)
        //         _keyframeTrackStorage.AddTrack(node, new Track(gameObject, gameObject.name),
        //             trackObjectData.trackObject);
        //
        //     print(gameObject.transform.position);
        //     
        //     _keyframeTrackStorage.AddKeyframe(node, _main.CurrentTime - trackObjectData.trackObject.StartTime,
        //         new PositionData(gameObject.transform.position));
        // }
    }
}