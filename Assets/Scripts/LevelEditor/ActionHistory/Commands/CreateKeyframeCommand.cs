using System;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class CreateKeyframeCommand : ICommand
    {
        private string _groupIP;

        private readonly string _description;

        private CompositionEdit _composition;
        private SaveComposition _save;

        private BranchCollection _branchCollection;
        private readonly TrackObjectStorage _trackObjectStorage;
        private TrackObjectPacket _trackObjectPacket;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private readonly EntityAnimationData _animationData;
        private readonly Entity _target;
        private readonly string _trackName;
        private readonly Color _animationColor;
        private readonly string _componentName;
        private readonly ComponentNames _componentNames;
        private readonly KeyframeCreator _keyframeCreator;
        private readonly KeyframeRemover _keyframeRemover;

        private string nodePath;
        private double keyframeTime;

        public CreateKeyframeCommand(
            EntityAnimationData animationData,
            Entity target, string trackName,
            Color animationColor, string componentName,
            ComponentNames componentNames,
            KeyframeCreator keyframeCreator,
            KeyframeRemover keyframeRemover,
            KeyframeTrackStorage keyframeTrackStorage,
            TrackObjectStorage trackObjectStorage)
        {
            _animationData = animationData;
            _target = target;
            _trackName = trackName;
            _animationColor = animationColor;
            _componentName = componentName;
            _componentNames = componentNames;
            _keyframeCreator = keyframeCreator;
            _keyframeRemover = keyframeRemover;
            _keyframeTrackStorage = keyframeTrackStorage;
            _trackObjectStorage = trackObjectStorage;
        }

        public string Description() => _description;

        public void Execute()
        {
            _trackObjectPacket = _trackObjectStorage.GetTrackObjectData(_target);
            var node = _keyframeCreator.CreateKeyframe(_animationData, _target, _trackName, _animationColor, _componentName, _componentNames);
            nodePath = node.Item1.Path;
            keyframeTime = node.Item2;
        }

        public void Undo()
        {
            var node = _trackObjectPacket.branch.FindNode(nodePath);
            var track = _keyframeTrackStorage.GetTrack(node);
            var keyframe = _keyframeTrackStorage.GetTrack(node).Keyframes.Find(x => Math.Abs(x.Ticks - keyframeTime) < 0.1f);
            _keyframeRemover.Remove(track, keyframe);
        }
    }
}