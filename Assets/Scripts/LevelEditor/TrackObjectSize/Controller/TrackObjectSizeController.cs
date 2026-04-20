using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.TrackObjectSize.Data;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TrackObjectSize.Controller
{
    public class TrackObjectSizeController : MonoBehaviour
    {
        private TrackObjectSizeData _data;
            
        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, TrackObjectSizeData data)
        {
            _gameEventBus = gameEventBus;
            _data = data;
        }

        private void Start()
        {
            _data.SetTicks(TimeLineConverter.TICKS_PER_BEAT);
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
               if(data.UpdateVisual) 
                    _data.SetTicks(data.Tracks[^1].components.Data.TimeDurationInTicks);
            });
        }
    }
}