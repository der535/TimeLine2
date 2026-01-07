using UnityEngine;
using UnityEngine.UI;

public class SyncScroll : MonoBehaviour
{
    [Header("Элементы прокрутки")]
    public ScrollRect scrollA;
    public ScrollRect scrollB;

    void OnEnable()
    {
        // Подписываемся на события изменения положения
        scrollA.onValueChanged.AddListener(OnScrollA);
        scrollB.onValueChanged.AddListener(OnScrollB);
    }

    void OnDisable()
    {
        // Отписываемся при выключении, чтобы избежать утечек памяти
        scrollA.onValueChanged.RemoveListener(OnScrollA);
        scrollB.onValueChanged.RemoveListener(OnScrollB);
    }

    private void OnScrollA(Vector2 value)
    {
        // Синхронизируем вертикальную позицию B с A
        if (scrollB.verticalNormalizedPosition != value.y)
        {
            scrollB.verticalNormalizedPosition = value.y;
        }
    }

    private void OnScrollB(Vector2 value)
    {
        // Синхронизируем вертикальную позицию A с B
        if (scrollA.verticalNormalizedPosition != value.y)
        {
            scrollA.verticalNormalizedPosition = value.y;
        }
    }
}