using System.Collections.Generic;
using System.Linq;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine.Keyframe
{
    public class KeyframeTrackStorage : MonoBehaviour
    {
        private List<TrackData> tracks = new();

        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        void Awake()
        {
            _gameEventBus.SubscribeTo<SmoothTimeEvent>(Evaluate);
        }

        [Button]
        void PrintTracks()
        {
            foreach (var VARIABLE in tracks)
            {
                print(VARIABLE.Track.TrackName);
                print(VARIABLE.Track.Keyframes.Count);
            }
        }

        private void Evaluate(ref SmoothTimeEvent smoothTimeEvent)
        {
            foreach (var variable in tracks)
            {
                if (variable.Active)
                {
                    print(variable.Track.Keyframes.Count);
                    variable.Track.Evaluate(smoothTimeEvent.Time - variable.TrackObject.StartTime);
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

        public void SetActiveTrack(TreeNode treeNode, bool active)
        {
            foreach (var track in tracks.ToList().Where(track => track.TreeNode == treeNode))
            {
                track.Active = active;
            }
        }

        public void AddTrack(TreeNode treeNode, Track track, TrackObject trackObject)
        {
            print(track.TrackName);
            print(treeNode.Name);
            tracks.Add(new TrackData(treeNode, track, trackObject));
            _gameEventBus.Raise(new AddTrackEvent(track));
        }

        public Track GetTrack(TreeNode treeNode)
        {
            // print(treeNode.Name);
            foreach (var track in tracks.ToList().Where(track => track.TreeNode == treeNode))
            {
                return track.Track;
            }

            return null;
        }

        public void AddKeyframe(TreeNode treeNode, float time, AnimationData data)
        {
            Track track = GetTrack(treeNode);
            _gameEventBus.Raise(new AddKeyframeEvent(track.AddKeyframe(time, data)));
        }

        class TrackData
        {
            public TrackData(TreeNode treeNode, Track track, TrackObject trackObject)
            {
                TreeNode = treeNode;
                Track = track;
                TrackObject = trackObject;
            }

            public TreeNode TreeNode;
            public Track Track;
            public TrackObject TrackObject;
            public bool Active = true;
        }
    }
}