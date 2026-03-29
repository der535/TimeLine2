using System;
using EventBus;
using TimeLine.LevelEditor.LevelEffects;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class ECSServiceLocator : MonoBehaviour
    {
        public static ECSServiceLocator Instance;
        
        public GameEventBus GameEventBus;
        public TrackObjectStorage TrackObjectStorage;
        public ShakeCameraController ShakeCameraController;
        public M_PlaybackState M_PlaybackState;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage, ShakeCameraController shakeCameraController, M_PlaybackState main)
        {
            this.GameEventBus = gameEventBus;
            this.TrackObjectStorage = trackObjectStorage;
            this.ShakeCameraController = shakeCameraController;
            this.M_PlaybackState = main;
        }
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
