using EventBus;
using TimeLine.EventBus.Events.Bezier;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.Bezier_curve
{
    public class BezierSelectPoint : MonoBehaviour
    {
        [SerializeField] private RectTransform leftTangle;
        [SerializeField] private RectTransform leftLineTangle;
        [SerializeField] private RectTransform rightTangle;
        [SerializeField] private RectTransform rightLineTangle;
        [Space]
        [SerializeField] private Image pointImage;
        [Space]
        [SerializeField] private BezierPoint bezierPoint;
        [Space]
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color deselectedColor;

        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        public void Select()
        {
            bezierPoint.Select(true);
            pointImage.color = selectedColor;
            _gameEventBus.Raise(new BezierSelectPointEvent(bezierPoint));
        }

        internal void Deselect()
        {
            bezierPoint.Select(false);
            pointImage.color = deselectedColor;
            leftTangle.gameObject.SetActive(false);
            leftLineTangle.gameObject.SetActive(false);
            rightLineTangle.gameObject.SetActive(false);
            rightTangle.gameObject.SetActive(false);
        }
    }
}
