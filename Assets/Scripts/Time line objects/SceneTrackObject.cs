using UnityEngine;

namespace TimeLine
{
    public class SceneTrackObject : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private TrackObjectSO _trackObjectSO;

        internal void Setup(TrackObjectSO trackObjectSO)
        {
            _trackObjectSO = trackObjectSO;
            spriteRenderer.sprite = trackObjectSO.sprite;
            gameObject.name = trackObjectSO.name;
        }

        public TrackObjectSO Copy() => _trackObjectSO;
    }
}
