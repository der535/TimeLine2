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
            TrackObjectData trackObjectData = _objectFactory.CreateFullObject("Camera");
            Destroy(trackObjectData.sceneObject);
            trackObjectData.sceneObject = _camera.gameObject;
        }
    }
}