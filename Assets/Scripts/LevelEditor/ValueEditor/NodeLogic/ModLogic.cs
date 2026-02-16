namespace TimeLine.LevelEditor.ValueEditor.NodeLogic
{
    public class ModLogic : global::NodeLogic
    {
        public ModLogic()
        {
            InputDefinitions = new()
            {
                ("Dividend", DataType.Float),
                ("Divisor", DataType.Float)
            };
            OutputDefinitions = new()
            {
                ("Remainder", DataType.Float),
            };
        }

        public override object GetValue(int outputIndex = 0)
        {
            return  (float)GetInputValue(0, 0) % (float)GetInputValue(1, 0);
        }
    }
}