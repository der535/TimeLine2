using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.ValueEditor;
using Zenject;

public class PlayerPositionLogic : NodeLogic
{
    PlayerComponents _playerPosition;

    [Inject]
    private void Construct(PlayerComponents playerPosition)
    {
        _playerPosition = playerPosition;
    }

    public PlayerPositionLogic()
    {
        OutputDefinitions = new()
        {
            ("X", DataType.Float),
            ("Y", DataType.Float),
            ("Z", DataType.Float)
        };
    }

    public override object GetValue(int outputIndex = 0)
    {
        switch (outputIndex)
        {
            case 0:
                return _playerPosition.GetPosition().x;
            case 1:
                return _playerPosition.GetPosition().y;
            case 2:
                return _playerPosition.GetPosition().z;
        }
        return 0;
    }
}