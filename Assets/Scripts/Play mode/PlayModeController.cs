using System;
using EventBus;
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
        private bool _isPlaying;

        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(Main main, GameEventBus gameEventBus)
        {
            _main = main;
            _gameEventBus = gameEventBus;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                ExitPlayMode();
        }

        public void TurnToPlayMode()
        {
            _isPlaying = true;
            editCamera.gameObject.SetActive(false);
            playCamera.gameObject.SetActive(true);
            _gameEventBus.Raise(new TurnToPlayModeEvent());
            Invoke(nameof(Play), startDelay);
        }

        public void ExitPlayMode()
        {
            _isPlaying = false;
            editCamera.gameObject.SetActive(true);
            playCamera.gameObject.SetActive(false);
            _gameEventBus.Raise(new ExitPlayEvent());
            _main.Pause();
        }

        private void Play()
        {
            if(!_isPlaying) return;
            
            _main.SetTimeInTicks((float)trackObjectStorage.GetMinTime());
            _main.Play();
        }
    }
}
