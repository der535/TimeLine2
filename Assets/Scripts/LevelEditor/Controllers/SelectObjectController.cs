using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.LevelEditor.ActionHistory;
using TimeLine.LevelEditor.ActionHistory.Commands;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace TimeLine.EventBus.Events.TrackObject
{
    public class SelectObjectController : MonoBehaviour
    {
        [SerializeField] private SelectLock _selectLock;
        private GameEventBus _gameEventBus;
        private readonly List<TrackObjectPacket> _trackObjects = new();
        private TrackObjectStorage _trackObjectStorage;
        private TimeLineSettings _timeLineSettings;
        private TimeLineScroll _timeLineScroll;
        private ActionMap _actionMap;

        public List<TrackObjectPacket> SelectObjects => _trackObjects;
        public HashSet<TrackObjectPacket> SelectObjectsHash => _trackObjects.ToHashSet();

        [Inject]
        private void Construct(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage,
            TimeLineSettings timeLineSettings, TimeLineScroll timeLineScroll, ActionMap actionMap)
        {
            _gameEventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
            _timeLineSettings = timeLineSettings;
            _timeLineScroll = timeLineScroll;
            _actionMap = actionMap;
        }

        void Start()
        {
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                _trackObjects.Clear();
            });
        }

        public void UpdateSelection()
        {
            if (_trackObjects.Count > 0)
                _gameEventBus.Raise(new SelectObjectEvent(_trackObjects, true));
        }

        public void SelectMultiple(TrackObjectPacket trackObject)
        {
            var isMultiple = UnityEngine.Input.GetKey(KeyCode.LeftShift);

            if (_selectLock.IsLocked) return;

            if (isMultiple)
            {
                if (!_trackObjects.Contains(trackObject))
                {
                    List<TrackObjectPacket> newState = _trackObjects.ToList();
                    newState.Add(trackObject);
                    CommandHistory.ExecuteCommand(new SelectObjectCommand(_trackObjectStorage, this, _trackObjects.ToList(), newState, ""));
                }
                else
                {
                    List<TrackObjectPacket> newState = _trackObjects.ToList();
                    newState.Remove(trackObject);
                    Debug.Log(_trackObjects.Count);
                    Debug.Log(newState.Count);
                    CommandHistory.ExecuteCommand(new SelectObjectCommand(_trackObjectStorage, this, _trackObjects.ToList(), newState, ""));
                }
            }
            else
            {
                if (!_trackObjects.Contains(trackObject))
                    CommandHistory.ExecuteCommand(new SelectObjectCommand(_trackObjectStorage, this, _trackObjects.ToList(), new List<TrackObjectPacket>() { trackObject }, ""));
            }
        }

        public void SelectMultiple(List<TrackObjectPacket> trackObjects)
        {
            var changed = false;

            foreach (var trackObject in trackObjects)
            {
                if (!_trackObjects.Contains(trackObject))
                {
                    _trackObjects.Add(trackObject);
                    Debug.Log(trackObject.sceneObjectID);
                    changed = true;
                }
                else
                {
                    Deselect(trackObject);
                }
            }

            if (changed)
            {
                _gameEventBus.Raise(new SelectObjectEvent(_trackObjects, true));
            }
        }


        /// <summary>
        /// Этот метод вызывается только из команды выделения
        /// </summary>
        /// <param name="trackObjects"></param>
        public void SelectMultipleCommand(List<TrackObjectPacket> trackObjects)
        {
            _gameEventBus.Raise(new DeselectAllObjectEvent());
            _trackObjects.AddRange(trackObjects);
            if (_trackObjects.Count > 0)
                _gameEventBus.Raise(new SelectObjectEvent(_trackObjects, true));
            else
                _gameEventBus.Raise(new DeselectAllObjectEvent());
        }


        /// <summary>
        /// Метод который снимант выделение с объектов без вызова евента иза которого могло лагать
        /// </summary>
        /// <param name="trackObject"></param>
        public void DeselectVihoutEvent(TrackObjectPacket trackObject)
        {
            _trackObjects.Remove(trackObject);
            trackObject.components.View.SetColor(Color.gray);
        }


        public void Deselect(TrackObjectPacket trackObject)
        {
            _trackObjects.Remove(trackObject);
            if (_trackObjects.Count == 0)
            {
                _gameEventBus.Raise(new DeselectAllObjectEvent());
            }
            else
            {
                _gameEventBus.Raise(new DeselectObjectEvent(trackObject, SelectObjects));
            }
        }

        public void DeselectAll()
        {
            CommandHistory.ExecuteCommand(new SelectObjectCommand(_trackObjectStorage, this, _trackObjects.ToList(), new List<TrackObjectPacket>(), ""));
        }

        List<TrackObjectPacket> _savedTrackObjectsMove = new();

        public void StartMultipleMove()
        {
            Debug.Log(SelectObjects.Count);
            _savedTrackObjectsMove = SelectObjects.ToList();


            foreach (var trackObjectData in _trackObjects)
            {
                trackObjectData.components.TrackObject.SavePosition();
            }
        }

        public void StopMultipleMove()
        {
            if (_savedTrackObjectsMove.Count > 0)
            {
                var previousPositions = _savedTrackObjectsMove.Select(x => (x.components.State.StartTrackObjectTicks, x.components.State.TrackLineIndex)).ToList();
                var newPosition = _savedTrackObjectsMove.Select(x => (x.components.Data.StartTimeInTicks, x.components.Data.TrackLineIndex)).ToList();

                CommandHistory.ExecuteCommand(new MoveTrackObjectCommand(_trackObjectStorage,_savedTrackObjectsMove, previousPositions, newPosition, ""));
            }
        }


        public void MultipleMove(double ticks)
        {
            if (_trackObjects.Count <= 0 || ticks == 0) return;

            List<TrackObjectPacket> trackObjectPackets = _trackObjects.ToList();

            foreach (var trackObjectData in _trackObjects)
                trackObjectData.components.TrackObject.AddTicksMove(PositionBinding(ticks, trackObjectPackets));
        }

        internal double PositionBinding(double delta, List<TrackObjectPacket> exclusion)
        {
            // Если Shift не зажат или двигать некого — возвращаем исходную дельту
            if (!_actionMap.Editor.LeftShift.IsPressed() || exclusion.Count == 0) return delta;

            float visualPixelRadius = 15f;
            float pixelsPerBeat = _timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom;
            double bindRadiusInTicks = (visualPixelRadius / pixelsPerBeat) * TimeLineConverter.TICKS_PER_BEAT;

            // Собираем все потенциальные цели для магнита (те, кто НЕ двигается)
            var staticObjects = _trackObjectStorage.TrackObjects
                .Concat(_trackObjectStorage.TrackObjectGroups)
                .Where(staticObj => !exclusion.Any(ex => ex.components.Data == staticObj.components.Data))
                .ToList();

            double bestDelta = delta;
            double minDistance = double.MaxValue;

            foreach (var movingPacket in exclusion)
            {
                var state = movingPacket.components.State;
                var data = movingPacket.components.Data;
                double projectedStart = state.StartTrackObjectTicks + delta;
                double projectedEnd = projectedStart + data.TimeDurationInTicks;

                foreach (var staticPacket in staticObjects)
                {
                    var staticData = staticPacket.components.Data;
                    double staticStart = staticData.StartTimeInTicks;
                    double staticEnd = staticStart + staticData.TimeDurationInTicks;

                    // Проверяем 4 комбинации примагничивания:
                    // 1. Начало движущегося к Началу статического
                    CheckSnap(projectedStart, staticStart, state.StartTrackObjectTicks, ref bestDelta, ref minDistance, bindRadiusInTicks);
                    // 2. Начало движущегося к Концу статического
                    CheckSnap(projectedStart, staticEnd, state.StartTrackObjectTicks, ref bestDelta, ref minDistance, bindRadiusInTicks);
                    // 3. Конец движущегося к Началу статического
                    CheckSnap(projectedEnd, staticStart, state.StartTrackObjectTicks + data.TimeDurationInTicks, ref bestDelta, ref minDistance, bindRadiusInTicks);
                    // 4. Конец движущегося к Концу статического
                    CheckSnap(projectedEnd, staticEnd, state.StartTrackObjectTicks + data.TimeDurationInTicks, ref bestDelta, ref minDistance, bindRadiusInTicks);
                }
            }

            return bestDelta;
        }

// Вспомогательный метод для поиска ближайшей точки привязки

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectedPos">Текущаяя позиция при перемещении</param>
        /// <param name="targetPos">Целевая позиция</param>
        /// <param name="originalPos">Начальная позиция</param>
        /// <param name="bestDelta">Дельта для позиции</param>
        /// <param name="minDistance">Минимальная дистанциая если число получилось ниже чем minDistance то записываем в него нового значение</param>
        /// <param name="radius">Радиус магнита</param>
        /// <param name="leftResize">Инвертирует расчёт для корекного подсчёта резайза с левой стороны</param>
        private void CheckSnap(double projectedPos, double targetPos, double originalPos, ref double bestDelta, ref double minDistance, double radius, bool leftResize = false)
        {
            double currentDist = math.abs(targetPos - projectedPos);

            if (currentDist <= radius && currentDist < minDistance)
            {
                minDistance = currentDist;
                // Новая дельта = Целевая позиция - Изначальная позиция объекта
                if (!leftResize)
                    bestDelta = targetPos - originalPos;
                else
                    bestDelta = originalPos - targetPos;
            }
        }


        public void MultipleChangeTrackLine(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self, int deltaIndex)
        {
            if (_trackObjects.Count <= 1) return;

            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.AddLineTrackIndex(deltaIndex);
            }
        }

        public List<TrackObjectPacket> savedReziseObjects = new();
        public List<(double Duraction, double startTime, double ReduceRight, double ReduceLeft)> oldResizeData = new();

        public void SaveResizingData(
            TrackObjectPacket self)
        {
            self.components.TrackObject.SaveResizingData();
            foreach (var trackObjectData in _trackObjects)
            {
                trackObjectData.components.TrackObject.SaveResizingData();
            }
            
            savedReziseObjects = _trackObjects.ToList();

            oldResizeData = _trackObjects.Select(x => (
                Duraction: x.components.Data.TimeDurationInTicks,
                startTime: x.components.Data.StartTimeInTicks,
                ReduceRight: x.components.Data.ReducedRight,
                ReduceLeft: x.components.Data.ReduceLeft
            )).ToList();

            if (!_trackObjects.Contains(self))
            {
                savedReziseObjects.Add(self);
                oldResizeData.Add((self.components.Data.TimeDurationInTicks, self.components.Data.StartTimeInTicks, self.components.Data.ReducedRight, self.components.Data.ReduceLeft));
            }
        }

        public void SaveResizeToHistory(TrackObjectPacket self)
        {
            var newResizeData = _trackObjects.Select(x => (
                Duraction: x.components.Data.TimeDurationInTicks,
                startTime: x.components.Data.StartTimeInTicks,
                ReduceRight: x.components.Data.ReducedRight,
                ReduceLeft: x.components.Data.ReduceLeft
            )).ToList();
            
            if (!_trackObjects.Contains(self))
            {
                newResizeData.Add((self.components.Data.TimeDurationInTicks, self.components.Data.StartTimeInTicks, self.components.Data.ReducedRight, self.components.Data.ReduceLeft));
            }
            
            CommandHistory.ExecuteCommand(new ResizeTrackObjectCommand(_trackObjectStorage, savedReziseObjects, oldResizeData, newResizeData, ""));
        }

        public void MultipleResizingRight(TrackObjectPacket self, double ticks)
        {
            List<TrackObjectPacket> trackObjectPackets = _trackObjects.ToList();

            if (!_trackObjects.Contains(self))
            {
                var delta = PositionBindingRight(ticks, new List<TrackObjectPacket>() { self });
                self.components.TrackObject.MultipleRightResize(delta);
            }
            else
                foreach (var trackObjectData in _trackObjects)
                    trackObjectData.components.TrackObject.MultipleRightResize(PositionBindingRight(ticks, trackObjectPackets));
        }

        internal double PositionBindingRight(double delta, List<TrackObjectPacket> exclusion)
        {
            // 1. Базовая дельта (ограничиваем ее сразу, если не нажат Shift, но есть лимиты)
            double finalDelta = delta;

            float visualPixelRadius = 15f;
            float pixelsPerBeat = _timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom;
            double bindRadiusInTicks = (visualPixelRadius / pixelsPerBeat) * TimeLineConverter.TICKS_PER_BEAT;

            // Собираем статические объекты для магнита
            var staticObjects = _trackObjectStorage.TrackObjects
                .Concat(_trackObjectStorage.TrackObjectGroups)
                .Where(staticObj => !exclusion.Any(ex => ex.components.Data == staticObj.components.Data))
                .ToList();

            double bestSnapDelta = delta;
            double minSnapDistance = double.MaxValue;

            // 2. Сначала находим "бутылочное горлышко" — минимальный лимит среди всех выделенных объектов
            double globalMaxAllowedDelta = double.MaxValue;
            double globalMinAllowedDelta = double.MinValue;

            foreach (var movingPacket in exclusion)
            {
                if (movingPacket.components.Data.EnableResizeLimits)
                {
                    // Вычисляем, сколько этот конкретный объект еще может "пройти" вправо
                    // Предполагаем, что StartReduceRight — это оставшийся запас хода
                    double individualLimit = math.abs(movingPacket.components.State.StartReduceRight);
                    double duraction = -movingPacket.components.State.StartResizingDuractionInTicks;

                    if (individualLimit < globalMaxAllowedDelta)
                    {
                        globalMaxAllowedDelta = individualLimit;
                    }

                    if (duraction > globalMinAllowedDelta)
                    {
                        globalMinAllowedDelta = duraction;
                    }
                }
            }

            // 3. Если зажат Shift, ищем точку примагничивания
            if (_actionMap.Editor.LeftShift.IsPressed() && exclusion.Count > 0)
            {
                foreach (var movingPacket in exclusion)
                {
                    var state = movingPacket.components.State;
                    // Позиция конца объекта при текущем движении мыши
                    double projectedEnd = state.StartResizingTimeInTicks + state.StartResizingDuractionInTicks + delta;

                    foreach (var staticPacket in staticObjects)
                    {
                        var staticData = staticPacket.components.Data;
                        double staticStart = staticData.StartTimeInTicks;
                        double staticEnd = staticStart + staticData.TimeDurationInTicks;

                        // Магнитим конец к началу или концу статики
                        CheckSnap(projectedEnd, staticStart, state.StartResizingTimeInTicks + state.StartResizingDuractionInTicks, ref bestSnapDelta, ref minSnapDistance, bindRadiusInTicks);
                        CheckSnap(projectedEnd, staticEnd, state.StartResizingTimeInTicks + state.StartResizingDuractionInTicks, ref bestSnapDelta, ref minSnapDistance, bindRadiusInTicks);
                    }
                }

                finalDelta = bestSnapDelta;
            }

            // 4. ГЛАВНОЕ: Ограничиваем итоговую дельту (будь то сдвиг мыши или магнит) общим лимитом
            if (finalDelta > globalMaxAllowedDelta)
            {
                finalDelta = globalMaxAllowedDelta;
            }

            // Debug.Log(finalDelta);
            // Debug.Log(globalMinAllowedDelta - 1);

            // Дополнительная проверка на отрицательный ресайз (чтобы не сделать длину меньше нуля, если нужно)
            finalDelta = math.max(finalDelta, globalMinAllowedDelta + 1);

            // 5. Обновляем визуальное состояние лимитов для каждого объекта (опционально для UI)
            foreach (var movingPacket in exclusion)
            {
                if (movingPacket.components.Data.EnableResizeLimits)
                {
                    movingPacket.components.Data.ReducedRight = movingPacket.components.State.StartReduceRight + finalDelta;
                }
            }

            return finalDelta;
        }

        public void MultipleResizingLeft(
            TrackObjectPacket self, double ticks)
        {
            List<TrackObjectPacket> trackObjectPackets = _trackObjects.ToList();

            if (!trackObjectPackets.Contains(self))
                self.components.TrackObject.MultipleLeftResize(PositionBindingLeft(ticks, new List<TrackObjectPacket>() { self }));
            else
                foreach (var trackObjectData in _trackObjects)
                    trackObjectData.components.TrackObject.MultipleLeftResize(PositionBindingLeft(ticks, trackObjectPackets));
        }


        internal double PositionBindingLeft(double delta, List<TrackObjectPacket> exclusion)
        {
            // 1. Базовая дельта (ограничиваем ее сразу, если не нажат Shift, но есть лимиты)
            double finalDelta = delta;

            float visualPixelRadius = 15f;
            float pixelsPerBeat = _timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom;
            double bindRadiusInTicks = (visualPixelRadius / pixelsPerBeat) * TimeLineConverter.TICKS_PER_BEAT;

            // Собираем статические объекты для магнита
            var staticObjects = _trackObjectStorage.TrackObjects
                .Concat(_trackObjectStorage.TrackObjectGroups)
                .Where(staticObj => !exclusion.Any(ex => ex.components.Data == staticObj.components.Data))
                .ToList();

            double bestSnapDelta = delta;
            double minSnapDistance = double.MaxValue;

            // 2. Сначала находим "бутылочное горлышко" — минимальный лимит среди всех выделенных объектов
            double globalMaxAllowedDelta = double.MaxValue;
            double globalMinAllowedDelta = double.MinValue;

            foreach (var movingPacket in exclusion)
            {
                if (movingPacket.components.Data.EnableResizeLimits)
                {
                    // Вычисляем, сколько этот конкретный объект еще может "пройти" вправо
                    // Предполагаем, что StartReduceRight — это оставшийся запас хода
                    double individualLimit = math.abs(movingPacket.components.State.StartReduceLeft);
                    double duraction = -movingPacket.components.State.StartResizingDuractionInTicks;

                    if (individualLimit < globalMaxAllowedDelta)
                    {
                        globalMaxAllowedDelta = individualLimit;
                    }

                    if (duraction > globalMinAllowedDelta)
                    {
                        globalMinAllowedDelta = duraction;
                    }
                }
            }

            // 3. Если зажат Shift, ищем точку примагничивания
            if (_actionMap.Editor.LeftShift.IsPressed() && exclusion.Count > 0)
            {
                foreach (var movingPacket in exclusion)
                {
                    var state = movingPacket.components.State;
                    // Позиция конца объекта при текущем движении мыши
                    double projectedStart = state.StartResizingTimeInTicks - delta;

                    foreach (var staticPacket in staticObjects)
                    {
                        var staticData = staticPacket.components.Data;
                        double staticStart = staticData.StartTimeInTicks;
                        double staticEnd = staticStart + staticData.TimeDurationInTicks;

                        // Магнитим конец к началу или концу статики
                        CheckSnap(projectedStart, staticStart, state.StartResizingTimeInTicks, ref bestSnapDelta, ref minSnapDistance, bindRadiusInTicks, true);
                        CheckSnap(projectedStart, staticEnd, state.StartResizingTimeInTicks, ref bestSnapDelta, ref minSnapDistance, bindRadiusInTicks, true);
                    }
                }

                Debug.Log(delta);

                if (Math.Abs(bestSnapDelta - delta) > 0.001f)
                {
                    // bestSnapDelta = math.abs(bestSnapDelta);
                    Debug.Log(bestSnapDelta);
                    Debug.Log(delta);
                }

                finalDelta = bestSnapDelta;
            }

            // 4. ГЛАВНОЕ: Ограничиваем итоговую дельту (будь то сдвиг мыши или магнит) общим лимитом
            if (finalDelta > globalMaxAllowedDelta)
            {
                finalDelta = globalMaxAllowedDelta;
            }

            // Debug.Log(finalDelta);
            // Debug.Log(globalMinAllowedDelta+1);

            // Дополнительная проверка на отрицательный ресайз (чтобы не сделать длину меньше нуля, если нужно)
            finalDelta = math.max(finalDelta, globalMinAllowedDelta + 1);

            // 5. Обновляем визуальное состояние лимитов для каждого объекта (опционально для UI)
            foreach (var movingPacket in exclusion)
            {
                if (movingPacket.components.Data.EnableResizeLimits)
                {
                    movingPacket.components.Data.ReduceLeft = movingPacket.components.State.StartReduceLeft + finalDelta;

                    // double newStartTimeInTicks = movingPacket.components.State.StartResizingTimeInTicks - finalDelta;
                    // double newDurationInTicks = movingPacket.components.State.StartResizingDuractionInTicks + finalDelta;
                    //
                    // newStartTimeInTicks = math.round(newStartTimeInTicks);
                    // newDurationInTicks =  math.round(newDurationInTicks);
                    //
                    // movingPacket.components.TrackObject.Rezise?.Invoke(newStartTimeInTicks - movingPacket.components.Data.StartTimeInTicks);
                    // movingPacket.components.Data.StartTimeInTicks = newStartTimeInTicks;
                    // movingPacket.components.Data.ChangeDurationInTicks(newDurationInTicks);
                }
            }

            return finalDelta;
        }

        // private double RoundTicksToGrid(double ticks)
        // {
        //     return _gridUI.RoundTicksToGrid(ticks);
        // }

        public void MultipleStopResizingLeft(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.ApplyKeyframeOffset();
            }
        }
    }
}