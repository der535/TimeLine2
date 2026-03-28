using EventBus;
using TimeLine;
using TimeLine.LevelEditor.Save;

public struct StartCompositionEdit : IEvent
{
    public GroupGameObjectSaveData Group { get; }

    public StartCompositionEdit(GroupGameObjectSaveData group)
    {
        Group = group;
    }   
}