using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class PlayAndStopController : MonoBehaviour
    {
        [SerializeField] private WindowsFocus gameWindows;
        
        private Main _main;
        private ActionMap _actionMap;
        private M_PlaybackState _playState;

        [Inject]
        private void Construct(ActionMap actionMap, Main main, M_PlaybackState playbackState)
        {
            _actionMap = actionMap;
            _main = main;
            _playState = playbackState;
        }

        private void Start()
        {
            _actionMap.Editor.Space.performed += ctx =>
            {
                if(gameWindows.IsFocused) return;
                
                if (_playState.IsPlaying == false)
                {
                    _main.Play();
                }
                else
                {
                    _main.Pause();
                }
            };
        }
    }
}
