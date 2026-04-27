using System.Collections.Generic;
using System.Linq;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.LevelEditor.GeneralEditor;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace TimeLine.Keyframe
{
    public class KeyframeTrackStorage : MonoBehaviour
    {
        private List<TrackData> tracks = new();

        private GameEventBus _gameEventBus;

        [SerializeField] private AnimationCurve curve;
        private BranchCollection _branchCollection;
        private PlayAndStopButton _playAndStopButton;

        [Inject]
        private void Construct(GameEventBus gameEventBus, BranchCollection branchCollection, PlayAndStopButton playModeController)
        {
            _gameEventBus = gameEventBus;
            _branchCollection = branchCollection;
            _playAndStopButton = playModeController;
        }

        void Awake()
        {
            _gameEventBus.SubscribeTo((ref TickSmoothTimeEvent data) => { Evaluate(data.Time); });
        }

        internal List<TrackData> GetTracks() => tracks;
        
        internal void Evaluate(double time)
        {
            foreach (var variable in tracks)
            {
                if (variable.Active)
                {
                    // print(variable.Track.Keyframes.Count);
                    UpdatingFromAnimation.isUpdatingFromAnimation = true;
                    variable.Track.Evaluate(time - variable.TrackObjectData.GetGlobalTicksPosition());
                    UpdatingFromAnimation.isUpdatingFromAnimation = false;
                }
            }
        }

        public void RemoveTrack(TreeNode treeNode)
        {
            foreach (var track in tracks.ToList().Where(track => track.TreeNode == treeNode))
            {
                tracks.Remove(track);
            }
        }

        public void RemoveTrackWithNode(Track trackr)
        {
            foreach (var track in tracks.ToList().Where(track => track.Track == trackr))
            {
                Branch branch = _branchCollection.GetBranch(track.BranchId);
                branch.RemoveNode(track.TreeNode);
                // _branchCollection.AddNodeToBranch()
                // track.TreeNode

                tracks.Remove(track);
            }
        }

        public void SetActiveTrack(TreeNode treeNode, bool active)
        {
            foreach (var track in tracks.ToList().Where(track => track.TreeNode == treeNode))
            {
                track.Active = active;
            }
        }

        public void AddTrack(TreeNode treeNode, Track track, TrackObjectData trackObjectData, string branchId, bool updateTreeUI)
        {
            tracks.Add(new TrackData(treeNode, track, trackObjectData, branchId));
            if (updateTreeUI) _gameEventBus.Raise(new AddTrackEvent(track));
        }

        public Track GetTrack(TreeNode treeNode)
        {
            foreach (var track in tracks.ToList().Where(track => track.TreeNode == treeNode))
            {
                return track.Track;
            }

            return null;
        }

        public void AddKeyframe(TreeNode treeNode, double time, EntityAnimationData data)
        {
            Track track = GetTrack(treeNode);
            _gameEventBus.Raise(new AddKeyframeEvent(track.AddKeyframe(time, data)));
        }
    }

    public class TrackData
    {
        public TrackData(TreeNode treeNode, Track track, TrackObjectData trackObjectData, string branchId)
        {
            BranchId = branchId;
            TreeNode = treeNode;
            Track = track;
            TrackObjectData = trackObjectData;
        }
        
        public string BranchId;
        public TreeNode TreeNode;
        public Track Track;
        public TrackObjectData TrackObjectData;
        public bool Active = true;
    }
}