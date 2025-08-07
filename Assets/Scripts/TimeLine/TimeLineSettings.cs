using UnityEngine;

namespace TimeLine
{
    public class TimeLineSettings : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float distanceBetweenBeatLines = 70;

        public float DistanceBetweenBeatLines => distanceBetweenBeatLines;
    }
}
