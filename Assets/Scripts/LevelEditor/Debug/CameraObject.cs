using System;
using EventBus;
using NaughtyAttributes;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CameraObject : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        private GameEventBus _gameEventBus;
        private ObjectFactory _objectFactory;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, ObjectFactory objectFactory)
        {
            _gameEventBus = gameEventBus;
            _objectFactory = objectFactory;
        }

        [Button]
        private void Create()
        {
            TrackObjectPacket trackObjectPacket = _objectFactory.CreateFullObject("Camera");
            Destroy(trackObjectPacket.sceneObject);
            trackObjectPacket.sceneObject = _camera.gameObject;
        }
    }
}