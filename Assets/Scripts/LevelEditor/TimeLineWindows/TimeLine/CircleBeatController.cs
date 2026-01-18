using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class CircleBeatController : MonoBehaviour
    {
        [SerializeField] private Image soundImage;
        [SerializeField] private AudioSource audioSource;
        
        [SerializeField] private Button button;
        
        [SerializeField] private Color inactiveColorSound;
        [SerializeField] private Color activeColorSound;

        private bool _active;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
            
            _gameEventBus.SubscribeTo((ref ThemeChangedEvent data) =>
            {
                inactiveColorSound = data.Theme.circleBeatInActive;
                activeColorSound = data.Theme.circleBeatActive;
                soundImage.color = _active ? activeColorSound : inactiveColorSound;
            });
        }

        private void Start()
        {
            button.onClick.AddListener(ChangeState);
            ChangeState();
        }

        private void ChangeState()
        {
            _active = !_active;
            soundImage.color = _active ? activeColorSound : inactiveColorSound;
            audioSource.mute = _active;
        }
    }
}
