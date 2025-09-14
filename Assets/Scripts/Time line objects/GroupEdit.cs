using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class GroupEdit : MonoBehaviour
    {
        [SerializeField] private GroupSeparate groupSeparate;

        private TrackObjectStorage _trackObjectStorage;
        private SelectObjectController _selectObjectController;
        private GroupCreater _groupCreater;
        private GameEventBus _gameEventBus;

        List<TrackObjectData> trackObjects = new();

        private bool _isEditing;

        [Inject]
        private void Construct(TrackObjectStorage trackObjectStorage, SelectObjectController selectObjectController,
            GroupCreater groupCreater, GameEventBus gameEventBus)
        {
            _trackObjectStorage = trackObjectStorage;
            _selectObjectController = selectObjectController;
            _groupCreater = groupCreater;
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo(((ref AddTrackObjectDataEvent data) =>
            {
                if(_isEditing)
                    trackObjects.Add(data.TrackObjectData);
            }));
        }

        public void Edit()
        {
            _isEditing = !_isEditing;

            if (_isEditing)
            {
                if (_selectObjectController.SelectObjects.Count > 1 ||
                    _selectObjectController.SelectObjects[0] is not TrackObjectGroup)
                {
                    _isEditing = false;
                    return;
                }

                _trackObjectStorage.HideAll();
                if (_selectObjectController.SelectObjects[0] is TrackObjectGroup group)
                {
                    trackObjects = group.TrackObjectDatas;
                    groupSeparate.Separate(group);
                }
            }
            else
            {
                _groupCreater.Create(trackObjects);
                _trackObjectStorage.ShowAll();
            }
        }
    }
}