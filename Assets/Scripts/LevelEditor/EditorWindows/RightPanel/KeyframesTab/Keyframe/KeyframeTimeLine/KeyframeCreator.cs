using TimeLine.CustomInspector.Logic;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.ActionHistory;
using TimeLine.LevelEditor.ActionHistory.Commands;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframeCreator : MonoBehaviour
    {
        TrackObjectStorage _trackObjectStorage;
        BranchCollection _branchCollection;
        KeyframeTrackStorage _keyframeTrackStorage;
        KeyframeRemover _keyframeRemover;
        Main _main;
        
        [Inject]
        private void Construct(TrackObjectStorage trackObjectStorage, BranchCollection branchCollection, KeyframeTrackStorage keyframeTrackStorage, Main main,
            KeyframeRemover keyframeRemover)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;   
            
            _keyframeTrackStorage = keyframeTrackStorage;
            _main = main;
            _keyframeRemover = keyframeRemover;
        }

        public void CreateKeyframeCommand(EntityAnimationData animationData, Entity target, string trackName, Color animationColor, string componentName, ComponentNames componentNames)
        {
            CommandHistory.AddCommand(new CreateKeyframeCommand(animationData, target, trackName, animationColor, componentName, componentNames, this, _keyframeRemover, _keyframeTrackStorage, _trackObjectStorage), true);
        }
        
        public (TreeNode, double) CreateKeyframe(EntityAnimationData animationData, Entity target, string trackName, Color animationColor, string componentName, ComponentNames componentNames)
        {
            TrackObjectPacket trackObjectPacket = _trackObjectStorage.GetTrackObjectData(target);
        
            TreeNode node = _branchCollection.AddNodeToBranch(trackObjectPacket.branch.ID, trackObjectPacket.branch.Name,
                componentName+"/"+trackName);
            
            if (_keyframeTrackStorage.GetTrack(node) == null)
                _keyframeTrackStorage.AddTrack(node, new Track(target, trackName, animationColor, componentNames),
                    trackObjectPacket.components.Data, trackObjectPacket.branch.ID, true);

            var keyframeTime = TimeLineConverter.Instance.TicksCurrentTime() - trackObjectPacket.components.Data.GetGlobalTicksPosition();
            
            _keyframeTrackStorage.AddKeyframe(node, keyframeTime,
                animationData);

            return (node, keyframeTime);
        }
    }
}
