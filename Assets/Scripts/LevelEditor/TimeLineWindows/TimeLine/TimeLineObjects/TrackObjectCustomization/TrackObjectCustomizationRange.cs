using EventBus;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class TrackObjectCustomizationRange : MonoBehaviour
    {
        [SerializeField] private RectTransform trackObjectRect;
        [SerializeField] private Image image;
        
        [SerializeField, Range(0, 100)] private float range = 0;

        private RectTransform _root;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
            _gameEventBus.SubscribeTo((ref PanEvent data) =>
            {
                UpdateImage();
                print("pan update");
            });
        }

        public void Setup(Color color, float range, RectTransform root)
        {
            image.color = color;
            this.range = range;
            print("setup root");
            print(root);
            _root = root;
        }

        private void UpdateImage()
        {
            var value = Mathf.Lerp(_root.sizeDelta.x /2, 0, range / 100);
            trackObjectRect.offsetMin = new Vector2(value, trackObjectRect.offsetMin.y);
            trackObjectRect.offsetMax = new Vector2(-value, trackObjectRect.offsetMin.y);
        }
        
        internal void UpdateImage(float _range)
        {
            range = _range;
            var value = Mathf.Lerp(_root.sizeDelta.x /2, 0, range / 100);
            trackObjectRect.offsetMin = new Vector2(value, trackObjectRect.offsetMin.y);
            trackObjectRect.offsetMax = new Vector2(-value, trackObjectRect.offsetMin.y);
        }
    }
}
