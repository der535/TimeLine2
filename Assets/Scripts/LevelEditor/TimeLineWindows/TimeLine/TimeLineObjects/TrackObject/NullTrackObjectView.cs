using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using UnityEngine;

/// <summary>
/// Пустая реализация что бы при попытке изменить вид небыло ошибок
/// </summary>
public class NullTrackObjectView : ITrackObjectView
{
    public void Destroy()
    {
    }

    public void Hide()
    {
    }

    public void Show()
    {
    }

    public Transform GetParent()
    {
        return null;
    }

    public void SetParent(Transform parent)
    {
    }

    public Vector2 GetSizeDelta()
    {
        return Vector2.zero;
    }

    public Vector2 GetAnchorPosition()
    {
        return Vector2.zero;
    }

    public Vector2 GetOffsetMax()
    {
        return Vector2.zero;
    }

    public Vector2 GetOffsetMin()
    {
        return Vector2.zero;
    }

    public RectTransform GetRectTransform()
    {
        return null;
    }

    public void SetOffsetMax(Vector2 offset)
    {
    }

    public void SetOffsetMin(Vector2 offset)
    {
    }

    public void SetSizeDelta(Vector2 size)
    {
    }

    public void SetAnchorPosition(Vector2 position)
    {
    }

    public void SetColor(Color c)
    {
    }

    public void Rename(string name)
    {
        
    }

    public bool GetActive() => false;
}