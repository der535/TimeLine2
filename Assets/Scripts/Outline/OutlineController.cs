
using System.Collections.Generic;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class OutlineController : MonoBehaviour
    {
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private GameObject outlinePrefab;
        [SerializeField] private float stroke;
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Color outlineColor;

        private List<SpriteRenderer> _outlines = new();

        GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus eventBus)
        {
            _gameEventBus = eventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                Clear();
                foreach (var track in data.Tracks)
                {
                    DrawOutline(track.sceneObject);
                }
            });

            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => Clear());
        }

        [Button]
        private void DrawOutline(GameObject selectedObject)
        {
            if (trackObjectStorage.GetTrackObjectData(selectedObject) is TrackObjectGroup trackObjectGroup)
            {
                CheckGroup(trackObjectGroup);
            }

            CheckSpriteRenderer(selectedObject);
        }

        private void CheckGroup(TrackObjectGroup trackObjectGroup)
        {
            foreach (var trackObject in trackObjectGroup.TrackObjectDatas)
            {
                if (trackObject is TrackObjectGroup trackObjectGroup2)
                {
                    CheckGroup(trackObjectGroup2);
                }
                else
                {
                    CheckSpriteRenderer(trackObject.sceneObject);
                }
            }
        }

        void CheckSpriteRenderer(GameObject selectedObject)
        {
            if (selectedObject.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                var outlines = new List<SpriteRenderer>();
                
                SpriteRenderer outlinePart =
                    Instantiate(outlinePrefab, selectedObject.transform).GetComponent<SpriteRenderer>();
                outlinePart.sprite = spriteRenderer.sprite;
                outlinePart.material = outlineMaterial;
                outlinePart.color = outlineColor;
                outlinePart.sortingLayerName = "UI";
                outlinePart.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                outlines.Add(outlinePart);

                _outlines.AddRange(outlines);
            }
        }

        [Button]
        private void Clear()
        {

            foreach (var part in _outlines)
            {
                if(part!=null)
                    Destroy(part?.gameObject);
            }

            _outlines.Clear();
        }
    }
}