
using TimeLine.LevelEditor.ValueEditor;

/// <summary>
/// Простая нода со значение float
/// </summary>
public class FloatLogic : NodeLogic
{
    /// <summary>
    /// Стандартное значение которое изменяется извне через content constructor
    /// </summary>
    public float Value;
    public FloatLogic()
    {
        OutputDefinitions = new()
        {
            ("Output", DataType.Float)
        };
    }
    public override object GetValue(int outputIndex = 0) => Value;
}