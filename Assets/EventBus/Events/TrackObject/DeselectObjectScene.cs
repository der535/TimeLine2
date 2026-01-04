using System.Collections;
using EventBus;
using EventBus.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems; // Обязательно для интерфейсов событий
using Zenject;

namespace TimeLine
{
    // Добавляем интерфейс IPointerDownHandler
    public class DeselectObjectScene : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private UnityEvent onPressed;

        private GameEventBus _gameEventBus;
        private bool isSelected;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo<ObjectUnderCursorEvent>((ref ObjectUnderCursorEvent data) => StartCoroutine(Select()));
        }

        private IEnumerator Select()
        {
            isSelected = true;
            yield return new WaitForEndOfFrame();
            isSelected = false;
        }

        // Этот метод автоматически заменяет OnMouseDown и проверку IsPointerOverGameObject
        public void OnPointerDown(PointerEventData eventData)
        {
            // 1. Проверяем, что нажата именно левая кнопка мыши
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            // 2. Проверяем флаг временной блокировки (из вашего Coroutine)
            if (isSelected)
            {
                return;
            }

            // ПРИМЕЧАНИЕ: Нам больше не нужна проверка EventSystem.current.IsPointerOverGameObject(),
            // так как если клик будет поглощен UI-элементом, это событие (OnPointerDown) 
            // просто не дойдет до объекта сцены.

            onPressed?.Invoke();
        }
    }
}