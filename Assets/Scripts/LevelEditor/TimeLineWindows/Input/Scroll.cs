using EventBus;
using TimeLine.EventBus.Events.Input;
using UnityEngine;
using Zenject;

public class Scroll : MonoBehaviour
{
    private GameEventBus _eventBus;
    
    [Inject]
    private void Construct(GameEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    
    private void Update()
    {
        if (!Mathf.Approximately(Input.mouseScrollDelta.y, 0))
        {
            _eventBus.Raise(new MouseScrollDeltaY(Input.mouseScrollDelta.y));
        }
    }
}
