using UnityEngine;

namespace TimeLine
{
    public class TrackLine : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        
        public RectTransform RectTransform => rect;
    }
}
