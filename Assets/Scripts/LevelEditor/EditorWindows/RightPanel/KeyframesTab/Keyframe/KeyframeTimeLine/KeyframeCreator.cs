using TimeLine.CustomInspector.Logic;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframeCreator : MonoBehaviour
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
        
        public void CreateKeyframe(AnimationData animationData, GameObject target, string componentName, InspectableParameter fieldName)
        {
            TrackObjectPacket trackObjectPacket = _trackObjectStorage.GetTrackObjectData(target);
        
            TreeNode node = _branchCollection.AddNodeToBranch(trackObjectPacket.branch.ID, trackObjectPacket.branch.Name,
                componentName+"/"+fieldName.Name);
            
            if (_keyframeTrackStorage.GetTrack(node) == null)
                _keyframeTrackStorage.AddTrack(node, new Track(target, target.name, fieldName.AnimationColor),
                    trackObjectPacket.components.Data, trackObjectPacket.branch.ID);
            
            _keyframeTrackStorage.AddKeyframe(node, TimeLineConverter.Instance.TicksCurrentTime() - trackObjectPacket.components.Data.StartTimeInTicks,
                animationData);
        }
    }
}
