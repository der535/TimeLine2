using EventBus;
using TimeLine;

public struct StartCompositionEdit : IEvent
{
    public GroupGameObjectSaveData Group { get; }

    public StartCompositionEdit(GroupGameObjectSaveData group)
    {
        Group = group;
    }   
}