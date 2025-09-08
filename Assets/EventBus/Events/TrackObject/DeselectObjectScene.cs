using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace TimeLine
{
    public class DeselectObjectScene : MonoBehaviour
    {
        [SerializeField] private UnityEvent onPressed;

        void OnMouseDown()
        {
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