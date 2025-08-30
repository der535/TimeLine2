using System;
using UnityEngine;
using System.Collections.Generic;
using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using Zenject;

public class ParentMain : MonoBehaviour
{
    public List<TrackObjectData> sceneGameObjects = new List<TrackObjectData>();
    private GameEventBus _gameEventBus;
    public Action<List<TrackObjectData>> OnTrackObjectSelected;
    
    [Inject]
    void Construct(GameEventBus gameEventBus)
    {
        _gameEventBus = gameEventBus;
    }

    internal void InvokeOnTrackObjectSelected()
    {
        print(sceneGameObjects.Count);
        OnTrackObjectSelected?.Invoke(sceneGameObjects);
    }

    private void Start()
    {
        sceneGameObjects.Add(new TrackObjectData(null, null, null));

        _gameEventBus.SubscribeTo((ref AddTrackObjectDataEvent addTrackObjectDataEvent) =>
            RefreshDropdown(addTrackObjectDataEvent.TrackObjectData, true));
        _gameEventBus.SubscribeTo((ref RemoveTrackObjectDataEvent removeTrackObjectDataEvent) =>
            RefreshDropdown(removeTrackObjectDataEvent.TrackObjectData, false));
        OnTrackObjectSelected?.Invoke(sceneGameObjects);
    }
    

    // Метод для принудительного обновления dropdown из других скриптов
    private void RefreshDropdown(TrackObjectData currentParent, bool add)
    {
        if (add) sceneGameObjects.Add(currentParent);
        else
        {
            sceneGameObjects.Remove(currentParent);
        }
        OnTrackObjectSelected?.Invoke(sceneGameObjects);
    }
}