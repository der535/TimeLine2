using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.Bezier;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect
{
    public class KeyframeSelectController : MonoBehaviour
    {
        [SerializeField] private KeyfeameVizualizer keyframeVizualizer;

        private M_KeyframeSelectedStorage _storage;

        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        private BezierController bezierController;
        
        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap, M_KeyframeSelectedStorage storage, BezierController bezierController)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _storage = storage;
            this.bezierController = bezierController;
        }

        private void Awake()
        {
            _storage.Keyframes = new List<global::TimeLine.Keyframe.Keyframe>();
            _gameEventBus.SubscribeTo((ref SelectKeyframeEvent data) =>
                SelectKeyframe(data.Keyframe.Keyframe));
            _gameEventBus.SubscribeTo((ref BezierSelectPointEvent data) =>
                SelectKeyframe(data.BezierPoint.BezierDragPoint._keyframe));
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                ClearStorage();
            });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                ClearStorage();
            });
        }

        public void ClearStorage()
        {
            _storage.Keyframes.Clear();
        }


        internal void SelectKeyframe(global::TimeLine.Keyframe.Keyframe Keyframe)
        {
            if (_actionMap.Editor.LeftShift.IsPressed())
            {
                if (_storage.Keyframes.Contains(Keyframe))
                {
                    _storage.Keyframes.Remove(Keyframe);
                }
                else
                {
                    _storage.Keyframes.Add(Keyframe);
                }
            }
            else
            {
                if (!_storage.Keyframes.Contains(Keyframe))
                {
                    _storage.Keyframes.Clear();
                    _storage.Keyframes.Add(Keyframe);
                }
            }
            
            keyframeVizualizer.DisableAll();
            bezierController.DeselectAll();

            foreach (var keyframe in _storage.Keyframes)
            {
                keyframeVizualizer.GetKeyframeObjectData(keyframe).KeyframeSelect.SelectColor(true);
                bezierController.GetBezierPoint(keyframe)?.BezierSelectPoint?.SelectNoEvent();
                // bezierController.GetBezierPoint(keyframe)?.Select(false);
            }
        }
        
        internal void DeselectKeyframe(global::TimeLine.Keyframe.Keyframe Keyframe)
        {
            if (_storage.Keyframes.Contains(Keyframe))
            {
                _storage.Keyframes.Remove(Keyframe);
            }
            
            keyframeVizualizer.DisableAll();
            bezierController.DeselectAll();

            foreach (var keyframe in _storage.Keyframes)
            {
                keyframeVizualizer.GetKeyframeObjectData(keyframe).KeyframeSelect.SelectColor(true);
                bezierController.GetBezierPoint(keyframe)?.BezierSelectPoint?.SelectNoEvent();
                // bezierController.GetBezierPoint(keyframe)?.Select(false);
            }
        }
    }
}