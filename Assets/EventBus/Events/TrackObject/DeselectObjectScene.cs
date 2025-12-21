using System.Collections;
using EventBus;
using EventBus.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Zenject;

namespace TimeLine
{
    public class DeselectObjectScene : MonoBehaviour
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

        void OnMouseDown()
        {
            if(isSelected) return;
            // Проверяем, находится ли курсор над UI-элементом
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Если курсор над UI — игнорируем клик
                return;
            }

            // Если не над UI — вызываем событие
            onPressed?.Invoke();
        }
    }
}