using System;
using System.Collections.Generic;
using System.Linq;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Data;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.View;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Controller
{
    public class BezierFocusController : MonoBehaviour
    {
        [SerializeField] private float focusBorderSpacing = 30;

        private TimeLineKeyframeZoom _timeLineKeyframeZoom;
        private M_KeyframeSelectedStorage _storage;
        private BezierVerticalPosition _bezierVerticalPosition;
        private VerticalBezierZoom _verticalBezierZoom;
        private ScrollTimeLineKeyframe _scrollTimeLineKeyframe;
        private KeyframeReferences _keyframeReferences;
        private ActionMap _actionMap;
        private IReadActiveBezierPointsData _activeBezierPoints;

        [Inject]
        private void Construct(M_KeyframeSelectedStorage storage, BezierVerticalPosition bezierVerticalPosition,
            VerticalBezierZoom verticalBezierZoom, ScrollTimeLineKeyframe scrollTimeLineKeyframe,
            TimeLineKeyframeZoom timeLineKeyframeZoom, KeyframeReferences keyframeReferences, ActionMap actionMap, IReadActiveBezierPointsData readActiveBezierPointsData)
        {
            _storage = storage;
            _bezierVerticalPosition = bezierVerticalPosition;
            _verticalBezierZoom = verticalBezierZoom;
            _scrollTimeLineKeyframe = scrollTimeLineKeyframe;
            _timeLineKeyframeZoom = timeLineKeyframeZoom;
            _actionMap = actionMap;
        }

        private void Start()
        {
            _actionMap.Editor.F.started += _ => Focus(_activeBezierPoints.Get());
        }

        internal void Focus(List<BezierPoint> activePoints)
        {
            // Список ключевых кадров, на которых будет производиться фокусировка
            List<global::TimeLine.Keyframe.Keyframe> focusPoints = new();

            // Выбор целевых ключевых кадров:
            if (_storage.Keyframes.Count > 1)
            {
                // Если выделено более одного ключевого кадра — используем их
                focusPoints = _storage.Keyframes;
            }
            else if (activePoints.Count > 1)
            {
                // Иначе, если активно более одной точки Безье — берём их ключевые кадры
                focusPoints = activePoints.Select(x => x.BezierDragPoint._keyframe).ToList();
            }
            else
            {
                // Недостаточно данных для фокусировки — выходим
                return;
            }

            // Инициализация границ по времени и значению
            float maxTime = -1; // Максимальное время (в тиках)
            float minTime = float.MaxValue; // Минимальное время (в тиках)
            float maxValue = float.MinValue; // Максимальное значение параметра
            float minValue = float.MaxValue; // Минимальное значение параметра

            // Проход по всем целевым ключевым кадрам для определения экстремумов
            foreach (var keyframe in focusPoints)
            {
                // Обновляем временные границы
                if (keyframe.Ticks > maxTime)
                    maxTime = (float)keyframe.Ticks;
                if (keyframe.Ticks < minTime)
                    minTime = (float)keyframe.Ticks;

                // Обновляем границы значений (предполагается, что данные — float)
                if (keyframe.GetData().GetValue() is float value)
                {
                    if (value > maxValue) maxValue = value;
                    if (value < minValue) minValue = value;
                }
            }

            // Преобразуем временные границы из тиков в "биты" (beat-based units)
            // TICKS_PER_BEAT — константа, определяющая, сколько тиков в одном бите
            var timeDelta = maxTime / (float)TimeLineConverter.TICKS_PER_BEAT -
                            minTime / (float)TimeLineConverter.TICKS_PER_BEAT;

            // Рассчитываем целевой масштаб по горизонтали (пикселей на бит):
            // Доступная ширина = общая ширина области - отступы - ширина левой панели
            var targetWidth = (_keyframeReferences.rootPoints.rect.width - focusBorderSpacing -
                               _keyframeReferences.treePanelAnimations.sizeDelta.x) /
                              timeDelta;
            var result = targetWidth;

            // Преобразуем минимальное и максимальное время в позиции (в пикселях) при текущем масштабе
            var one = result * (minTime / (float)TimeLineConverter.TICKS_PER_BEAT);
            var two = result * (maxTime / (float)TimeLineConverter.TICKS_PER_BEAT);

            // Вычисляем смещение для центрирования: разница между крайними позициями минус ширина панели
            var offset = two - one - _keyframeReferences.treePanelAnimations.sizeDelta.x;

            // Аналогично для вертикальной оси:
            var valueDelta = maxValue - minValue;
            // Доступная высота = общая высота области - отступы
            var targetHeight = (_keyframeReferences.rootPoints.rect.height - focusBorderSpacing) / valueDelta;

            // Позиции верхней и нижней границ в пикселях
            var positionOne = minValue * targetHeight;
            var positionTwo = maxValue * targetHeight;
            var positionOffset = positionTwo - positionOne;

            // Применяем вычисленные масштабы и позиции:
            _timeLineKeyframeZoom.SetZoom(result); // Горизонтальный масштаб (время)
            _scrollTimeLineKeyframe.SetPosition(-(offset / 2 + one)); // Горизонтальное смещение (центрирование)
            _verticalBezierZoom.SetZoom(targetHeight); // Вертикальный масштаб (значения)
            _bezierVerticalPosition.SetPosition(-(positionOffset / 2 + positionOne)); // Вертикальное смещение
        }
    }
}