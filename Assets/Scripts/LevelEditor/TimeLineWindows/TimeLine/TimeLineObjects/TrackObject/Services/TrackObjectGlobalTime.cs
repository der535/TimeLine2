namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.Services
{
    public class TrackObjectGlobalTime
    {
        private TrackObjectComponents _trackObjectComponents;
        private TrackObjectData _trackObjectData;
        
        public TrackObjectGlobalTime(TrackObjectComponents trackObjectComponents, TrackObjectData trackObjectData)
        {
            _trackObjectComponents = trackObjectComponents;
            _trackObjectData = trackObjectData;
        }
        
        
        internal double Get()
        {
            return GetGlobalTime(_trackObjectComponents);
        }

        private double GetGlobalTime(TrackObjectComponents trackObject)
        {
            double realTime;
            if (_trackObjectData.offsetObject != null)
            {
                realTime = trackObject.Data.StartTimeInTicks + GetGlobalTime(_trackObjectData.offsetObject);
            }
            else
            {
                realTime = trackObject.Data.StartTimeInTicks;
            }

            return realTime;
        }

    }
}