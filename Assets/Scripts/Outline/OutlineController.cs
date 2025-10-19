using System;
using System.Collections.Generic;
using EventBus;
using JetBrains.Annotations;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class OutlineController : MonoBehaviour
    {
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
        private void DrawOutline(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                var outlines = new List<SpriteRenderer>();
                
                SpriteRenderer outlinePart =
                    Instantiate(outlinePrefab, gameObject.transform).GetComponent<SpriteRenderer>();
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