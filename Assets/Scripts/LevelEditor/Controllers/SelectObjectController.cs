using System.Collections.Generic;
using System.Linq;
using EventBus;
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
        private List<TrackObjectPacket> _trackObjects = new();
        private TrackObjectStorage _trackObjectStorage;
        private TimeLineSettings _timeLineSettings;
        private TimeLineScroll _timeLineScroll;
        private ActionMap _actionMap;

        public List<TrackObjectPacket> SelectObjects => this._trackObjects;
        public HashSet<TrackObjectPacket> SelectObjectsHash => this._trackObjects.ToHashSet();

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
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => { _trackObjects.Clear(); });
        }

        public void UpdateSelection()
        {
            if(_trackObjects.Count > 0)
                _gameEventBus.Raise(new SelectObjectEvent(_trackObjects, true));
        }

        public void SelectMultiple(TrackObjectPacket trackObject)
        {
            var isMultiple = UnityEngine.Input.GetKey(KeyCode.LeftShift);

            if (_selectLock.IsLocked) return;

            var changed = false;

            if (isMultiple)
            {
                if (!_trackObjects.Contains(trackObject))
                {
                    _trackObjects.Add(trackObject);
                    changed = true;
                }
                else
                {
                    Deselect(trackObject);
                }
            }
            else
            {
                if (!_trackObjects.Contains(trackObject))
                {
                    _trackObjects.Clear();
                    _trackObjects.Add(trackObject);
                    changed = true;
                }
            }

            if (changed)
            {
                _gameEventBus.Raise(new SelectObjectEvent(_trackObjects, true));
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
        /// Метод который выделяет объекты без вызова евента иза которого могло лагать
        /// </summary>
        /// <param name="trackObject"></param>
        public void SelectNoClearNoEvent(TrackObjectPacket trackObject)
        {
            if (_selectLock.IsLocked) return;

            if (!_trackObjects.Contains(trackObject))
            {
                _trackObjects.Add(trackObject);
                _gameEventBus.Raise(new SelectObjectEvent(_trackObjects, false));
            }
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
            _trackObjects.Clear();
            _gameEventBus.Raise(new DeselectAllObjectEvent());
        }

        public void StartMultipleMove(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.SavePosition();
            }
        }


        public void MultipleMove(global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self,
            double ticks)
        {
            if (_trackObjects.Count <= 0 || ticks == 0) return;

            List<TrackObjectPacket> trackObjectPackets = _trackObjects.Where(variable => variable.components.TrackObject != self).ToList();


            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.AddTicksMove(PositionBinding(ticks, trackObjectPackets));
            }
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
                double projectedEnd = projectedStart + state.StartTrackObjectTicks;

                foreach (var staticPacket in staticObjects)
                {
                    var staticData = staticPacket.components.Data;
                    double staticStart = staticData.StartTimeInTicks;
                    double staticEnd = staticStart + staticData.StartTimeInTicks;

                    // Проверяем 4 комбинации примагничивания:
                    // 1. Начало движущегося к Началу статического
                    CheckSnap(projectedStart, staticStart, state.StartTrackObjectTicks, ref bestDelta, ref minDistance, bindRadiusInTicks);
                    // 2. Начало движущегося к Концу статического
                    CheckSnap(projectedStart, staticEnd, state.StartTrackObjectTicks, ref bestDelta, ref minDistance, bindRadiusInTicks);
                    // 3. Конец движущегося к Началу статического
                    // CheckSnap(projectedEnd, staticStart, state.StartTrackObjectTicks + data.TimeDurationInTicks, ref bestDelta, ref minDistance, bindRadiusInTicks);
                    // 4. Конец движущегося к Концу статического
                    // CheckSnap(projectedEnd, staticEnd, state.StartTrackObjectTicks + data.TimeDurationInTicks, ref bestDelta, ref minDistance, bindRadiusInTicks);
                }
            }

            return bestDelta;
        }

// Вспомогательный метод для поиска ближайшей точки привязки
        private void CheckSnap(double projectedPos, double targetPos, double originalPos, ref double bestDelta, ref double minDistance, double radius)
        {
            double currentDist = math.abs(targetPos - projectedPos);
            if (currentDist <= radius && currentDist < minDistance)
            {
                minDistance = currentDist;
                // Новая дельта = Целевая позиция - Изначальная позиция объекта
                bestDelta = targetPos - originalPos;
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

        public void SaveResizingData(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.SaveResizingData();
            }
        }

        public void MultipleResizingRight(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self, double ticks)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.MultipleRightResize(ticks);
            }
        }

        public void MultipleResizingLeft(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self, double ticks)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.MultipleLeftResize(ticks);
            }
        }

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