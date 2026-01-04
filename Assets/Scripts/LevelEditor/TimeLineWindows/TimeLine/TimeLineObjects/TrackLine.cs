using UnityEngine;

namespace TimeLine
{
    /// <summary>
    /// Скрипт, который висит на линиях, эти линии представляют собой плоскость на которой распологаются trackobject
    /// </summary>
    public class TrackLine : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        
        public RectTransform RectTransform => rect;
    }
}
