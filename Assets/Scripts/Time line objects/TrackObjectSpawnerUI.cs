using UnityEngine;
using UnityEngine.Serialization;

namespace TimeLine
{
    public class TrackObjectSpawnerUI : MonoBehaviour
    {
        [SerializeField] private TrackObjectSpawner trackObjectSpawner;
        [SerializeField] private GameObject trackObjectUIPrefab;
        [FormerlySerializedAs("trackObjects")] [SerializeField] private Sprite[] sprites;
        [SerializeField] private RectTransform root;

        private void Start()
        {
            foreach (var trackObject in sprites)
            {
               TrackObjectUI trackObjectUI = Instantiate(trackObjectUIPrefab, root).GetComponent<TrackObjectUI>();
               trackObjectUI.Setup(trackObject, () => trackObjectSpawner.Spawn(trackObject));
            }
        }
    }
}
