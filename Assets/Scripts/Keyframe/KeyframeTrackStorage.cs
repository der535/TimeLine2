using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframeTrackStorage : MonoBehaviour
    {
        private Dictionary<TreeNode, (Track, TrackObject)> tracks = new();
        
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

        private void Evaluate(ref SmoothTimeEvent smoothTimeEvent)
        {
            foreach (var variable in tracks)
            {
                TrackObject trackObject;
                Track track;
                (track, trackObject) = variable.Value;
                track.Evaluate(smoothTimeEvent.Time - trackObject.StartTime);
            }
        }
        
        public void AddTrack(TreeNode treeNode, Track track, TrackObject trackObject)
        {
            tracks.Add(treeNode, (track, trackObject));
            _gameEventBus.Raise(new AddTrackEvent(track));
        }

        public Track GetTrack(TreeNode treeNode)
        {
            Track track;
            (track, _) = tracks.GetValueOrDefault(treeNode);
            return track;
        }

        public void AddKeyframe(TreeNode treeNode, float time, AnimationData data)
        {
            Track track;
            (track, _) = tracks[treeNode];
            _gameEventBus.Raise(new AddKeyframeEvent(track.AddKeyframe(time, data)));
        }
    }
}