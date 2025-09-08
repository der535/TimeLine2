using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class TrackObjectStorage : MonoBehaviour
    {
        private List<TrackObjectData> _trackObjects = new();

        [FormerlySerializedAs("_selectedObject")] public TrackObjectData selectedObject;
        [FormerlySerializedAs("_oldSelectedObject")] public TrackObjectData oldSelectedObject;
        
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo<TickSmoothTimeEvent>(ActiveSceneObject);
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => DeselectObject());

            // Подписываемся на событие — но НЕ вызываем SelectObject рекурсивно!
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                // Ищем данные объекта и вызываем внутреннюю логику БЕЗ повторного Raise
                var trackObjectData = GetTrackObjectData(data.Track.trackObject);
                if (trackObjectData != null)
                {
                    InternalSelectObject(trackObjectData);
                }
            });
        }

        private void ActiveSceneObject(ref TickSmoothTimeEvent smoothTimeEvent)
        {
            foreach (var trackObject in _trackObjects)
            {
                trackObject.sceneObject.SetActive(
                    trackObject.trackObject.StartTimeInTicks <= smoothTimeEvent.Time &&
                    trackObject.trackObject.TimeDuractionInTicks + trackObject.trackObject.StartTimeInTicks > smoothTimeEvent.Time
                );
            }
        }
        
        internal void Add(GameObject sceneObject, TrackObject selectedObject, Branch branch)
        {
            TrackObjectData trackObjectData = new TrackObjectData(sceneObject, selectedObject, branch);
            _gameEventBus.Raise(new AddTrackObjectDataEvent(trackObjectData));
            _trackObjects.Add(trackObjectData);
        }

        internal void Remove(TrackObjectData trackObjectData)
        {
            _gameEventBus.Raise(new RemoveTrackObjectDataEvent(trackObjectData));
            _trackObjects.Remove(trackObjectData);
        }

        internal TrackObjectData GetTrackObjectData(GameObject gObject)
        {
            return _trackObjects.FirstOrDefault(trackObject => trackObject.sceneObject == gObject);
        }
        
        internal TrackObjectData GetTrackObjectData(TrackObject trackObject)
        {
            return _trackObjects.FirstOrDefault(trackObject2 => trackObject2.trackObject == trackObject);
        }

        public void UpdatePositionSelectedTrackObject()
        {
            _gameEventBus.Raise(new DragTrackObjectEvent(selectedObject));
        }

        private void DeselectObject()
        {
            selectedObject = null;
            oldSelectedObject = null;
            foreach (var trackObject in _trackObjects)
            {
                trackObject.trackObject.Deselect();
            }
        }

        // Публичный метод — вызывается извне (например, из ObjectSettings)
        public void SelectObject(TrackObject trackObjectToSelect)
        {
            // Ищем данные
            var targetData = GetTrackObjectData(trackObjectToSelect);
            if (targetData == null) return;

            // Сначала снимаем выделение
            DeselectObject();

            // Применяем выделение без Raise события (чтобы не вызвать рекурсию)
            InternalSelectObject(targetData);

            // Теперь безопасно вызываем событие — оно НЕ должно вызывать SelectObject снова!
            _gameEventBus.Raise(new SelectObjectEvent(targetData));
        }

        // Внутренняя логика выделения — без Raise события
        private void InternalSelectObject(TrackObjectData trackObjectData)
        {
            if (trackObjectData == oldSelectedObject)
            {
                trackObjectData.trackObject.SelectColor();
                return;
            }

            this.selectedObject = trackObjectData;
            oldSelectedObject = trackObjectData;
            trackObjectData.trackObject.SelectColor();
        }
    }
    
    [Serializable]
    public class TrackObjectData
    {
        public GameObject sceneObject;
        public TrackObject trackObject;
        public Branch branch;

        public TrackObjectData(GameObject sceneObject, TrackObject trackObject, Branch branch)
        {
            this.sceneObject = sceneObject;
            this.trackObject = trackObject;
            this.branch = branch;
        }
    }
}