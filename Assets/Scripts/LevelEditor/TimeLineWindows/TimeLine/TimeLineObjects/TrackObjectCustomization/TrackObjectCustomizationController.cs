using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TrackObjectCustomizationController : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform rootCustomization;
        [SerializeField] private TrackObjectCustomizationRange rangePrefab;
        
        private List<object> _allCustomization;
        private DiContainer _container;

        [Inject]
        private void Constructor(DiContainer container)
        {
            _container = container;
        }

        internal TrackObjectCustomizationRange CreateRange(Color color, float size)
        {
            var range = _container.InstantiatePrefab(rangePrefab, root.transform).GetComponent<TrackObjectCustomizationRange>();
            range.Setup(color, size, root);
            return range;
        }
    }
}
