namespace TimeLine.LevelEditor.TrackObjectSize.Data
{
    public class TrackObjectSizeData : ITrackObjectSizeReader
    {
        private double _ticks;
        
        public void SetTicks(double ticks) { _ticks = ticks; }
        
        public double GetSize()
        {
            return _ticks;
        }
    }
}