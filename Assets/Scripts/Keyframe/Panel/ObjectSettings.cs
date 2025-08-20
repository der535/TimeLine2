using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class ObjectSettings : MonoBehaviour
    {
        private GameEventBus _gameEventBus;

        [Inject]
        public void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void OnMouseDown()
        {
            _gameEventBus.Raise(new SelectSceneObject(gameObject));
            // _settingPanel.Select(gameObject.transform, AddKeyframe);
            print("Select");
        }

        public void AddKeyframe()
        {

        }
    }
}