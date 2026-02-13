namespace TimeLine.LevelEditor.ValueEditor.NodeLogic
{
    public class AddLogic : global::NodeLogic
    {
        public AddLogic()
        {
            InputDefinitions = new()
            {
                ("Float 1", DataType.Float),
                ("Float 2", DataType.Float)
            };
            OutputDefinitions = new()
            {
                ("Sum", DataType.Float),
            };
        }

        public override object GetValue(int outputIndex = 0)
        {
            return (float)GetInputValue(0, 0) + (float)GetInputValue(1, 0);
        }
    }
}