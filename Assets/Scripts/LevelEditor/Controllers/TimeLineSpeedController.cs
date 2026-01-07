using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace TimeLine.LevelEditor.Controllers
{
    public class TimeLineSpeedController : MonoBehaviour
    {
        [Space] [SerializeField] private AudioSource audioSource;

        private float _speed = 1;
        
        internal float CurrentSpeed => _speed;
        
        internal void SetSpeed(float speed)
        {
            _speed = speed;
            audioSource.pitch = _speed;
        }

    }
}