using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.Bezier;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Data;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect
{
    public class KeyframeSelectController : MonoBehaviour
    {
        [SerializeField] private KeyframeVizualizer keyframeVizualizer;

        private M_KeyframeSelectedStorage _storage;

        private BezierController _bezierController;
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        private IReadActiveBezierPointsData _activeBezierPoints;
        
        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap, M_KeyframeSelectedStorage storage, BezierController bezierController, IReadActiveBezierPointsData activeBezierPoints)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _storage = storage;
            this._bezierController = bezierController;
            _activeBezierPoints = activeBezierPoints;
        }

        private void Awake()
        {
            _storage.Keyframes = new List<global::TimeLine.Keyframe.Keyframe>();
            _gameEventBus.SubscribeTo((ref SelectKeyframeEvent data) =>
                SelectKeyframe(data.Keyframe.Keyframe));
            _gameEventBus.SubscribeTo((ref BezierSelectPointEvent data) =>
                SelectKeyframe(data.BezierPoint.BezierDragPoint._original));
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent _) =>
            {
                ClearStorage();
            });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent _) =>
            {
                ClearStorage();
            });
            _gameEventBus.SubscribeTo((ref DeselectAllKeyframeEvent _) =>
            {
                DeselectAllKeyframes();
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
            _bezierController.DeselectAll();

            foreach (var keyframe in _storage.Keyframes)
            {
                keyframeVizualizer.GetKeyframeObjectData(keyframe)?.KeyframeSelect.SelectColor(true);
                _activeBezierPoints.GetFromKeyframe(keyframe)?.BezierSelectPoint?.SelectNoEvent();
            }
        }

        internal void SelectNoClear(global::TimeLine.Keyframe.Keyframe Keyframe)
        {
            if (!_storage.Keyframes.Contains(Keyframe))
            {
                _storage.Keyframes.Add(Keyframe);
            }
            
            keyframeVizualizer.DisableAll();
            _bezierController.DeselectAll();

            foreach (var keyframe in _storage.Keyframes)
            {
                keyframeVizualizer.GetKeyframeObjectData(keyframe).KeyframeSelect.SelectColor(true);
                _activeBezierPoints.GetFromKeyframe(keyframe)?.BezierSelectPoint?.SelectNoEvent();
            }
        }
        
        internal void DeselectKeyframe(global::TimeLine.Keyframe.Keyframe Keyframe)
        {
            if (_storage.Keyframes.Contains(Keyframe))
            {
                _storage.Keyframes.Remove(Keyframe);
            }
            
            keyframeVizualizer.DisableAll();
            _bezierController.DeselectAll();

            foreach (var keyframe in _storage.Keyframes)
            {
                keyframeVizualizer.GetKeyframeObjectData(keyframe).KeyframeSelect.SelectColor(true);
                _activeBezierPoints.GetFromKeyframe(keyframe)?.BezierSelectPoint?.SelectNoEvent();
            }
        }

        internal void DeselectAllKeyframes()
        {
            keyframeVizualizer.DisableAll();
            _bezierController.DeselectAll();
            _storage.Keyframes.Clear();
        }
    }
}