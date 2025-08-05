using UnityEngine;

namespace TimeLine
{
    public class SceneTrackObject : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private TrackObjectSO trackObjectSO;

        internal void Setup(TrackObjectSO trackObjectSO)
        {
            spriteRenderer.sprite = trackObjectSO.sprite;
            gameObject.name = trackObjectSO.name;
        }
    }
}
