namespace TimeLine.LevelEditor.ValueEditor.NodeLogic
{
    public class SubtractionLogic : global::NodeLogic
    {
        public SubtractionLogic()
        {
            InputDefinitions = new()
            {
                ("Minuend", DataType.Float),
                ("Subtrahend", DataType.Float)
            };
            OutputDefinitions = new()
            {
                ("Difference", DataType.Float),
            };
        }

        public override object GetValue(int outputIndex = 0)
        {
            return (float)GetInputValue(0, 0) - (float)GetInputValue(1, 0);
        }
    }
}