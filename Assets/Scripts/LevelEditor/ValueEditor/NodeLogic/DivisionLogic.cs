namespace TimeLine.LevelEditor.ValueEditor.NodeLogic
{
    public class DivisionLogic : global::NodeLogic
    {
        public DivisionLogic()
        {
            InputDefinitions = new()
            {
                ("Dividend", DataType.Float),
                ("Divisor", DataType.Float)
            };
            OutputDefinitions = new()
            {
                ("Quotient", DataType.Float),
            };
        }

        public override object GetValue(int outputIndex = 0)
        {
            return (float)GetInputValue(0, 0) / (float)GetInputValue(1, 0);
        }
    }
}