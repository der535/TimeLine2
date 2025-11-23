using System;
using EventBus;
using TimeLine.LevelEditor.Player;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayModeController : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [Space]
        [SerializeField] private float startDelay;
        [SerializeField] private TrackObjectStorage trackObjectStorage;

        [Space] 
        [SerializeField] private Camera editCamera;
        [SerializeField] private Camera playCamera;
        
        private Main _main;
        internal bool IsPlaying;
        private ActionMap _actionMap;

        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(Main main, GameEventBus gameEventBus, ActionMap actionMap)
        {
            _main = main;
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref RestartGameEvent data) =>
            {
                _main.SetTimeInTicks((float)trackObjectStorage.GetMinTime());
                Invoke(nameof(Play), 0.3f);
            });
            
            _actionMap.Editor.ESC.started += _ => ExitPlayMode();
        }
        
        public void TurnToPlayMode()
        {
            IsPlaying = true;
            editCamera.gameObject.SetActive(false);
            playCamera.gameObject.SetActive(true);
            _gameEventBus.Raise(new TurnToPlayModeEvent());
            print((float)trackObjectStorage.GetMinTime());
            _main.SetTimeInTicks((float)trackObjectStorage.GetMinTime());
            Invoke(nameof(Play), startDelay);
        }

        public void ExitPlayMode()
        {
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
