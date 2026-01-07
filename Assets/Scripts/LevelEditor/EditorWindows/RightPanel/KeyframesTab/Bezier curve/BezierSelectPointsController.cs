using EventBus;
using TimeLine.EventBus.Events.Bezier;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve
{
    public class BezierSelectPointsController : MonoBehaviour
    {
        // public List<BezierPoint> selectedPoints = new();

        private GameEventBus _gameEventBus;
        private BezierController _bezierController;
        private M_KeyframeSelectedStorage _selectKeyframe;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, BezierController bezierController,
            M_KeyframeSelectedStorage selectedStorage)
        {
            _gameEventBus = gameEventBus;
            _bezierController = bezierController;
            _selectKeyframe = selectedStorage;
        }
        
        public void MultipleDrag(double tickDifferent, double valueDifferent,
            global::TimeLine.Keyframe.Keyframe thisKeyframe)
        {
            foreach (var keyframe in _selectKeyframe.Keyframes)
            {
                if (thisKeyframe == keyframe) continue;

                _bezierController.GetBezierPoint(keyframe).BezierDragPoint._keyframe.Ticks += tickDifferent;
                _bezierController.GetBezierPoint(keyframe).BezierDragPoint._keyframe.GetData().SetValue(
                    (float)((float)_bezierController.GetBezierPoint(keyframe).BezierDragPoint._keyframe.GetData()
                        .GetValue() + valueDifferent));
            }

            _bezierController.UpdatePositions();
        }
    }
}