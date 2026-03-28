using EventBus;
using TimeLine.EventBus.Events.Bezier;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Data;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve
{
    public class BezierSelectPointsController : MonoBehaviour
    {
        // public List<BezierPoint> selectedPoints = new();

        private GameEventBus _gameEventBus;
        private global::TimeLine.BezierController _bezierController;
        private KeyframeSelectedStorage _selectKeyframe;
        private IReadActiveBezierPointsData _activeBezierPoints;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, global::TimeLine.BezierController bezierController,
            KeyframeSelectedStorage selectedStorage, IReadActiveBezierPointsData activeBezierPoints)
        {
            _gameEventBus = gameEventBus;
            _bezierController = bezierController;
            _selectKeyframe = selectedStorage;
            _activeBezierPoints = activeBezierPoints;
        }
        
        public void MultipleDrag(double tickDifferent, double valueDifferent,
            global::TimeLine.Keyframe.Keyframe thisKeyframe)
        {
            foreach (var keyframe in _selectKeyframe.Keyframes)
            {
                if (thisKeyframe == keyframe) continue;

                _activeBezierPoints.GetFromKeyframe(keyframe).BezierDragPoint._keyframe.Ticks += tickDifferent;
                _activeBezierPoints.GetFromKeyframe(keyframe).BezierDragPoint._keyframe.GetEntityData().SetValue(
                    (float)((float)_activeBezierPoints.GetFromKeyframe(keyframe).BezierDragPoint._keyframe.GetEntityData()
                        .GetValue() + valueDifferent));
            }

            _bezierController.UpdatePositions();
        }
    }
}