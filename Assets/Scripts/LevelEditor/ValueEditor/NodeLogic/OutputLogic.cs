using System.Collections.Generic;
using TimeLine.LevelEditor.ValueEditor;

public class OutputLogic : NodeLogic
{
    public DataType DataType;
    public OutputLogic() { } // Теперь Activator не будет ругаться
    
    public void Initialize(DataType type)
    {
        DataType = type;
        InputDefinitions = new List<(string name, DataType type)>
        {
            ("Result", type)
        };
    }

    public override object GetValue(int outputIndex = 0)
    {
        return GetInputValue(0, 0);
    }
}