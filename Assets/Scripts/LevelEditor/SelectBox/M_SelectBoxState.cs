using UnityEngine;


public class M_SelectBoxState
{
    public bool IsActive; // Активность выделенного квадрата
    public bool IsDragging; //Флаг предотвращает появление области выделения если до этого уже выделенн какойто объект
    public bool HasExceededDeadZone; //Проверка мёртвой зоны
    public bool CursorIsInside; //Проверка находится ли курсор внутри зоны выделения
    public Vector2 StartPosition; //Стартовая позиция мыши
    public Bounds SelectionBounds; //Рамки области выделения
}