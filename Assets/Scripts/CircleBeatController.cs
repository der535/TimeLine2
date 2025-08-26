using UnityEngine;
using UnityEngine.UI;

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
