using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    /// <summary>
    /// Класс отвечает за отображение и редактирование длительности (Duration) 
    /// выделенного объекта на таймлайне через текстовое поле.
    /// </summary>
    public class DisplayTrackDuraction : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TMP_InputField _inputField;

        private GameEventBus _gameEventBus;
        private TrackObjectStorage _trackObjectStorage;
        
        /// <summary>
        /// Внедрение зависимостей через Zenject.
        /// </summary>
        [Inject]
        private void Construct(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage)
        {
            _gameEventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
        }

        private void Awake()
        {
            // Подписка на событие выделения объекта:
            // Берем последний выделенный объект (индекс [^1]) и выводим его длительность в тиках.
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) => 
                _inputField.text = data.Tracks[^1].trackObject.TimeDuractionInTicks.ToString());

            // Подписка на событие снятия выделения с конкретного объекта:
            // Обновляем текст, чтобы он соответствовал оставшемуся последнему выделенному объекту.
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                // Проверка на пустой список может понадобиться здесь, если после деселекта ничего не осталось
                _inputField.text = data.SelectedObjects[^1].trackObject.TimeDuractionInTicks.ToString();
            });

            // Подписка на событие снятия выделения со всех объектов:
            // Очищаем поле ввода, так как редактировать нечего.
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => _inputField.text = "");

            // Слушатель завершения редактирования в InputField (нажатие Enter или потеря фокуса).
            _inputField.onEndEdit.AddListener(text =>
            {
                if (float.TryParse(text, out float newDuration))
                {
                    // Применяем новую длительность к текущему выделенному объекту.
                    _trackObjectStorage.selectedObject.trackObject.ChangeDurationInTicks(newDuration);
                }
            });
        }
    }
}