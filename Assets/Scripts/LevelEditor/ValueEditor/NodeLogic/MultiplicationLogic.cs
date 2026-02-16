namespace TimeLine.LevelEditor.ValueEditor.NodeLogic
{
    public class MultiplicationLogic : global::NodeLogic
    {
        public MultiplicationLogic()
        {
            InputDefinitions = new()
            {
                ("Multiplicand", DataType.Float),
                ("Multiplier", DataType.Float)
            };
            OutputDefinitions = new()
            {
                ("Product", DataType.Float),
            };
        }

        public override object GetValue(int outputIndex = 0)
        {
            return (float)GetInputValue(0, 0) * (float)GetInputValue(1, 0);
        }
    }
}