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
        private Dictionary<GameObject, Track> gameObjectTracks = new(); // Новый словарь
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
            gameObjectTracks[track.targetObject] = track; // Регистрируем трек
            _gameEventBus.Raise(new AddTrackEvent(track));
        }
        
        // public void OnAddKeyframeRequest(ref AddKeyframeRequestEvent e)
        // {
        //     if (gameObjectTracks.TryGetValue(e.targetObject, out Track track))
        //     {
        //         // AddKeyframe(track, CurrentTime, e.data);
        //     }
        // }

        public Track GetTrack(TreeNode treeNode)
        {
            Track track;
            (track, _) = tracks.GetValueOrDefault(treeNode);
            return track;
        }

        public void AddKeyframe(TreeNode treeNode, float time, AnimationData data)
        {
            print("Adding keyframe");
            Track track;
            (track, _) = tracks[treeNode];
            _gameEventBus.Raise(new AddKeyframeEvent(track.AddKeyframe(time, data)));
        }
    }
}
