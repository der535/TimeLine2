using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Service;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.View;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Controller
{
    public class BezierVerticalPositionController
    {
        [Inject]
        private void Construct(BezierVerticalPosition bezierVerticalPosition, GameEventBus eventBus,
            KeyframeReferences keyframeReferences, BezierLineDrawer bezierLineDrawer, BezierCursorValue bezierCursorValue)
        {
            Debug.Log("BezierVerticalPositionController");
            eventBus.SubscribeTo((ref ScrollBezier scrollEvent) =>
            {
                keyframeReferences.rootPoints.offsetMax += new Vector2(0, scrollEvent.ScrollOffset);
                keyframeReferences.rootPoints.offsetMin += new Vector2(0, scrollEvent.ScrollOffset);
                bezierLineDrawer.UpdateBezierCurve();
            }, 1);

            // Обработка панорамирования — сохраняем значение под курсором неизменным
            eventBus.SubscribeTo((ref ZoomBezier data) =>
            {
                // if (!_active) return;

                float oldPan = data.OldZoom;
                float newPan = data.Zoom;

                float cursorValuePos = bezierCursorValue.GetCursorValuePosition(oldPan); // Значение под курсором ДО пана

                // Получаем позиции якоря ДО и ПОСЛЕ пана, БЕЗ УЧЕТА СКРОЛЛА
                float anchorPosBeforePan = TimeLineConverter.Instance.GetAnchorPositionFromValue(cursorValuePos, oldPan);
                float anchorPosAfterPan = TimeLineConverter.Instance.GetAnchorPositionFromValue(cursorValuePos, newPan);

                // Дельта — насколько сместилась точка под курсором из-за смены масштаба
                float delta = anchorPosBeforePan - anchorPosAfterPan;

                // Новая позиция root = старая + дельта (чтобы компенсировать смещение)
                float newPositionY = keyframeReferences.rootPoints.offsetMin.y + delta;


                bezierVerticalPosition.SetPosition(newPositionY);
                bezierLineDrawer.UpdateBezierCurve();
            });
        }
    }
}