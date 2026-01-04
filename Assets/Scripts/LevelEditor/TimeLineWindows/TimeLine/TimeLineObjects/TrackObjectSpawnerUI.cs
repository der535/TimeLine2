using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using UnityEngine;
using UnityEngine.Serialization;

namespace TimeLine
{
    public class TrackObjectSpawnerUI : MonoBehaviour
    {
        [FormerlySerializedAs("objectSpawner")] [FormerlySerializedAs("trackObjectSpawner")] [SerializeField] private FacadeObjectSpawner facadeObjectSpawner;
        [SerializeField] private GameObject trackObjectUIPrefab;
        [SerializeField] private BaseSpriteStorage sprites;
        [SerializeField] private RectTransform root;

        private void Start()
        {
            foreach (var trackObject in sprites.Sprites)
            {
               TrackObjectUI trackObjectUI = Instantiate(trackObjectUIPrefab, root).GetComponent<TrackObjectUI>();
               trackObjectUI.Setup(trackObject, () => facadeObjectSpawner.CreateSceneObjectAndAddSprite(trackObject));
            }
        }
    }
}
