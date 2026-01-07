using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    /// <summary>
    /// Скрипт отвечает за то что бы в TrackObject при перемещении мышкой внутри него обновлялись ключевые кадры
    /// </summary>
    public class DragUpdateTrackObject : MonoBehaviour
    {
        // Зависимости
        private TrackObjectStorage _trackObjectStorage;
        private GameEventBus _eventBus;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private Main _main;

        /// <summary>
        /// Инъекция зависимостей
        /// </summary>
        [Inject]
        private void Constructor(GameEventBus gameEventBus, Main main, TrackObjectStorage trackObjectStorage,
            KeyframeTrackStorage keyframeTrackStorage)
        {
            _eventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
            _main = main;
        }

        /// <summary>
        /// При старте подписываемся на event DragTrackObjectEvent он передаёт трек обжект
        /// который сейчас перемещается и дальше в KeyframeTrackStorage мы принудительно обновляем анимацию
        /// </summary>
        private void Start()
        {
            _eventBus.SubscribeTo((ref DragTrackObjectEvent trackObjectEvent) =>
            {
                _trackObjectStorage.CheckActiveTrackSingle(trackObjectEvent.Track);
                _keyframeTrackStorage.Evaluate(TimeLineConverter.Instance.TicksCurrentTime());
            });
        }
    }
}