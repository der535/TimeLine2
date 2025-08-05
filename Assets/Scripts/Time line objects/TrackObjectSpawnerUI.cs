using UnityEngine;

namespace TimeLine
{
    public class TrackObjectSpawnerUI : MonoBehaviour
    {
        [SerializeField] private TrackObjectSpawner trackObjectSpawner;
        [SerializeField] private GameObject trackObjectUIPrefab;
        [SerializeField] private TrackObjectSO[] trackObjects;
        [SerializeField] private RectTransform root;

        private void Start()
        {
            foreach (var trackObject in trackObjects)
            {
               TrackObjectUI trackObjectUI = Instantiate(trackObjectUIPrefab, root).GetComponent<TrackObjectUI>();
               trackObjectUI.Setup(trackObject, () =>
                   trackObjectSpawner.Spawn(trackObject));
            }
        }
    }
}
