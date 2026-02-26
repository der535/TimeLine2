using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject
{
    public interface ITrackObjectView
    {
        void Destroy();
        void Hide();
        void Show();
        Transform GetParent();
        void SetParent(Transform parent);
        Vector2 GetSizeDelta();
        Vector2 GetAnchorPosition();
        Vector2 GetOffsetMax();
        Vector2 GetOffsetMin();
        RectTransform GetRectTransform();
        void SetOffsetMax(Vector2 offset);
        void SetOffsetMin(Vector2 offset);
        void SetSizeDelta(Vector2 size);
        void SetAnchorPosition(Vector2 position);
        void SetColor(Color c);
        void Rename(string name);
        bool GetActive();
    }
}