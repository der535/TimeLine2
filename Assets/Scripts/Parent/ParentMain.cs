using UnityEngine;
using System.Collections.Generic;
using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TMPro;
using Zenject;

public class ParentMain : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    
    [SerializeField] private TrackObjectData currentParent;
    [SerializeField] private TrackObjectData currentoObjectData;

    private TrackObjectStorage _trackObjectStorage;

    private List<TrackObjectData> _trackObjectDatas = new();
    private List<TrackObjectData> _localtrackObjectDatas = new();

    private GameEventBus _eventBus;

    [Inject]
    private void Construct(GameEventBus eventBus, TrackObjectStorage objectManager)
    {
        _eventBus = eventBus;
        _trackObjectStorage = objectManager;
    }

    private void Start()
    {
        _eventBus.SubscribeTo((ref AddTrackObjectDataEvent data) => UpdateDropdown(data.TrackObjectData, true));
        _eventBus.SubscribeTo((ref RemoveTrackObjectDataEvent data) => UpdateDropdown(data.TrackObjectData, false));
        _eventBus.SubscribeTo((ref SelectObjectEvent data) => SelectObject(data.Track));

        UpdateDropdown(new TrackObjectData(null, null, null), true);

        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void SelectObject(TrackObjectData trackObjectData)
    {
        if (trackObjectData.sceneObject == null) return;

        currentoObjectData = trackObjectData;
        currentParent = _trackObjectStorage.GetTrackObjectData(
            trackObjectData.sceneObject.transform.parent?.gameObject
        );

        // Создаём копию списка вместо использования ссылки
        _localtrackObjectDatas = new List<TrackObjectData>(_trackObjectDatas);
        _localtrackObjectDatas.Remove(trackObjectData); // Теперь это не затрагивает исходный список

        dropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (TrackObjectData obj in _localtrackObjectDatas)
        {
            options.Add(obj.sceneObject?.name ?? "null");
        }

        dropdown.AddOptions(options);

        if (currentParent != null)
        {
            int index = _localtrackObjectDatas.IndexOf(currentParent);
            if (index >= 0)
            {
                dropdown.SetValueWithoutNotify(index);
                dropdown.captionText.text = currentParent.sceneObject?.name;
            }
        }
        else if (_localtrackObjectDatas.Count > 0)
        {
            dropdown.SetValueWithoutNotify(0);
        }
    }

    private void UpdateDropdown(TrackObjectData trackObjectData, bool isAdd)
    {
        if (isAdd) _trackObjectDatas.Add(trackObjectData);
        else _trackObjectDatas.Remove(trackObjectData);

        SelectObject(currentParent);
    }

    private void OnDropdownValueChanged(int index)
    {
        if (index >= 0 && index < _localtrackObjectDatas.Count)
        {
            if(_localtrackObjectDatas[index]?.sceneObject != null)
                currentoObjectData.sceneObject.transform.SetParent(_localtrackObjectDatas[index].sceneObject.transform);
            else
            {
                currentoObjectData.sceneObject.transform.SetParent(null);
            }
            currentParent = _localtrackObjectDatas[index];
        }
    }
}