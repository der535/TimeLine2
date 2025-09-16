using EventBus;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class ComponentUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform rootObject;
        [Space]
        [SerializeField] private RectTransform componentTransform;

        [ShowNonSerializedField] private float _height;

        public RectTransform RootObject => rootObject;
        
        private ComponentVisiblyStorage _componentVisiblyStorage;
        private GameEventBus _gameEventBus;
        
        private bool _isVisible = true;
        private Component _component;

        [Inject]
        private void Construct(GameEventBus gameEventBus, ComponentVisiblyStorage componentVisiblyStorage)
        {
            _gameEventBus = gameEventBus;
            _componentVisiblyStorage = componentVisiblyStorage;
        }

        private void Awake()
        {
            _height += text.rectTransform.sizeDelta.y;
        }
        
        public void Setup(Component component)
        {
            _component = component;
            text.text = component.GetType().Name;
            if (_componentVisiblyStorage.GetVisibility(component.GetType()) == null)
            {
                _componentVisiblyStorage.SetVisibility(component.GetType(), true);
            }
            else if(_componentVisiblyStorage.GetVisibility(component.GetType()) == true)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void Hide()
        {
            print("Hide");
            _isVisible = false;
            rootObject.gameObject.SetActive(false);
            componentTransform.sizeDelta = new Vector2(componentTransform.sizeDelta.x, text.rectTransform.sizeDelta.y); 
            _componentVisiblyStorage.SetVisibility(_component.GetType(), _isVisible);
            print(_componentVisiblyStorage.GetVisibility(_component.GetType()));
        }

        public void Show()
        {
            print("Show");
            _isVisible = true;
            componentTransform.sizeDelta = new Vector2(componentTransform.sizeDelta.x, _height); 
            rootObject.gameObject.SetActive(true);
            _componentVisiblyStorage.SetVisibility(_component.GetType(), _isVisible);
        }

        public void AddHeight(float height)
        {
            _height += height;
            if(_isVisible) componentTransform.sizeDelta = new Vector2(componentTransform.sizeDelta.x, _height); 
        }
    }
}
