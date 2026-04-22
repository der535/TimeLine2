using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject
{
    public class TrackObjectState
    {
        public bool IsDragging;
        public bool IsResizing;
        public bool IsRightResizing;
        public bool WasResizing;
        
        public double StartResizingDuractionInTicks;
        public double StartResizingTimeInTicks;

        public double StartReduceRight;
        public double StartReduceLeft;
        
        public double InitialStartTimeInTicks;
        public Vector2 StartMousePosition;
        public double StartTrackObjectTicks;
        public float StartMouseXLocal;
        
        public double DeltaticksRight;
        public double DeltaticksLeft;
        
        public bool DeathZonePass;
        
    }
}