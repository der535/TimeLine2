using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.Grid;
using TimeLine.LevelEditor.EscInput;
using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class PlayModeController : MonoBehaviour
    {
        [FormerlySerializedAs("player")] [SerializeField] private PlayerFreeMoveController playerFreeMove;
        [Space]
        [SerializeField] private float startDelay;
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private List<GameObject> editorObjects;

        [Space] 
        [SerializeField] private Camera editCamera;
        [SerializeField] private Camera playCamera;
        
        private Main _main;
        internal bool IsPlaying;

        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(Main main, GameEventBus gameEventBus)
        {
            _main = main;
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref RestartGameEvent data) =>
            {
                _main.SetTimeInTicks((float)trackObjectStorage.GetMinTime());
                Invoke(nameof(Play), 0.3f);
            }, 1);
            
            
            _gameEventBus.SubscribeTo((ref EscapePressedEvent data) =>
            {
                ExitPlayMode();
            }, -1);
        }
        
        public void TurnToPlayMode()
        {
            foreach (var editorObject in editorObjects)
            {
                editorObject.SetActive(false);
            }
            IsPlaying = true;
            editCamera.gameObject.SetActive(false);
            playCamera.gameObject.SetActive(true);
            _gameEventBus.Raise(new TurnToPlayModeEvent());
            _main.SetTimeInTicks((float)trackObjectStorage.GetMinTime());
            Invoke(nameof(Play), startDelay);
        }

        public void ExitPlayMode()
        {
            foreach (var editorObject in editorObjects)
            {
                editorObject.SetActive(true);
            }
            IsPlaying = false;
            editCamera.gameObject.SetActive(true);
            playCamera.gameObject.SetActive(false);
            _gameEventBus.Raise(new ExitPlayEvent());
            _main.Pause();
        }

        private void Play()
        {
            if(!IsPlaying) return;
            
            _main.Play();
        }
    }
}
