using System;
using System.Collections.Generic;
using System.Linq;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ActionHistory;
using TimeLine.LevelEditor.ActionHistory.Commands;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.LevelEditor.CutTrackObject
{
    public class CutTrackObjectController : MonoBehaviour
    {
        [FormerlySerializedAs("_windowsFocus")]
        [SerializeField] private WindowsFocus windowsFocus;

        private SelectObjectController _selectObjectController;
        private ActionMap _actionMap;
        private M_PlaybackState _playbackState;
        private TrackObjectClipboard _clipboard;
        private TrackObjectStorage _storage;
        private TrackObjectRemover _trackObjectRemover;

        [Inject]
        private void Construct(
            SelectObjectController selectObjectController,
            ActionMap actionMap,
            M_PlaybackState playbackState,
            TrackObjectClipboard clipboard,
            TrackObjectStorage storage,
            TrackObjectRemover trackObjectRemover)
        {
            _selectObjectController = selectObjectController;
            _actionMap = actionMap;
            _playbackState = playbackState;
            _clipboard = clipboard;
            _storage = storage;
            _trackObjectRemover = trackObjectRemover;
        }

        private void Start()
        {
            _actionMap.Editor.CutRight.started += _ => ApplyToSelection(ResizeRight);
            _actionMap.Editor.CutLeft.started += _ => ApplyToSelection(ResizeLeft);
            _actionMap.Editor.CutHalf.started += _ => SplitObjects();
        }

        /// <summary>
        /// Обертка для применения действия ко всем выбранным объектам с проверкой фокуса окна.
        /// </summary>
        private void ApplyToSelection(Action<TrackObjectPacket> action)
        {
            if (!windowsFocus.IsFocused) return;

            var oldResizeData = _selectObjectController.SelectObjects.Select(x => (
                Duraction: x.components.Data.TimeDurationInTicks,
                startTime: x.components.Data.StartTimeInTicks,
                ReduceRight: x.components.Data.ReducedRight,
                ReduceLeft: x.components.Data.ReduceLeft
            )).ToList();

            List<(double Duraction, double startTime, double ReduceRight, double ReduceLeft)> previousSize, newSize;

            foreach (var trackObject in _selectObjectController.SelectObjects)
            {
                action?.Invoke(trackObject);
            }

            var newResizeData = _selectObjectController.SelectObjects.Select(x => (
                Duraction: x.components.Data.TimeDurationInTicks,
                startTime: x.components.Data.StartTimeInTicks,
                ReduceRight: x.components.Data.ReducedRight,
                ReduceLeft: x.components.Data.ReduceLeft
            )).ToList();

            CommandHistory.AddCommand(new ResizeTrackObjectCommand(_storage, _selectObjectController.SelectObjects.ToList(), oldResizeData, newResizeData, ""), false);
        }

        private void ResizeRight(TrackObjectPacket trackObject)
        {
            double currentTime = _playbackState.SmoothTimeInTicks;
            var data = trackObject.components.Data;

            var endPosition = data.TimeDurationInTicks + data.StartTimeInTicks;
            var delta = endPosition - currentTime;
            var newDuration = data.TimeDurationInTicks - delta;

            if (trackObject is TrackObjectGroup)
            {
                var fullDuration = data.TimeDurationInTicks + math.abs(data.ReducedRight);
                newDuration = math.clamp(newDuration, 1, fullDuration);

                var rightReduceNew = fullDuration - newDuration;

                trackObject.components.TrackObject.RightResize(newDuration);
                data.ReducedRight = -rightReduceNew;
            }
            else
            {
                trackObject.components.TrackObject.RightResize(newDuration);
            }
        }

        private void ResizeLeft(TrackObjectPacket trackObject)
        {
            Debug.Log(_playbackState.SmoothTimeInTicks);

            double currentTime = _playbackState.SmoothTimeInTicks;
            var data = trackObject.components.Data;

            if (trackObject is TrackObjectGroup)
            {
                var realStartPosition = data.StartTimeInTicks + data.ReduceLeft;
                var realDuration = data.TimeDurationInTicks + math.abs(data.ReduceLeft);
                Debug.Log(realStartPosition);

                var clampedTime = math.clamp(currentTime, realStartPosition, realStartPosition + realDuration - 1);

                trackObject.components.TrackObject.LeftResize(clampedTime);
                data.ReduceLeft = -clampedTime + realStartPosition;
            }
            else
            {
                trackObject.components.TrackObject.LeftResize(currentTime);
            }
        }

        private void SplitObjects()
        {
            if (!windowsFocus.IsFocused || _actionMap.Editor.LeftCtrl.IsPressed()) return;
            CommandHistory.AddCommand(new SplitTrackObjectsCommand(this, _storage, _trackObjectRemover, _selectObjectController.SelectObjects, ""), true);
        }

        /// <summary>
        /// Разделяет пополам трекобжекты
        /// </summary>
        /// <param name="trackObjects">Новые части (правые)</param>
        /// <returns></returns>
        internal List<TrackObjectPacket> SplitObjectsCommand(List<TrackObjectPacket> trackObjects)
        {
            double currentTime = _playbackState.SmoothTimeInTicks;

            // Фильтруем объекты, которые пересекаются с плейхедом
            var targetObjects = trackObjects
                .Where(obj => currentTime > obj.components.Data.StartTimeInTicks &&
                              currentTime < obj.components.Data.StartTimeInTicks + obj.components.Data.TimeDurationInTicks)
                .ToList();

            if (targetObjects.Count == 0) return null;

            // 1. Копируем данные для "правых" частей
            var copyData = _clipboard.CopyObjects(targetObjects);

            // 2. Обрезаем текущие объекты справа (они становятся левыми половинками)
            foreach (var obj in targetObjects)
                ResizeRight(obj);

            // 3. Вставляем скопированные объекты и обрезаем их слева (они становятся правыми половинками)
            var pastedList = _clipboard.PasteObjectsFromSave(copyData, 0, true);

            for (int i = 0; i < copyData.Count; i++)
                pastedList[i].components.Data.StartTimeInTicks = copyData[i].startTime;

            foreach (var obj in pastedList)
                ResizeLeft(obj);

            return pastedList;
        }
    }
}