namespace TimeLine.LevelEditor.MaxObjectIndex.Controller
{
    public class MaxObjectIndexData : IMaxObjectIndexDataReading
    {
        public int Index;
        public int GetNextIndex() => Index++;
    }
}