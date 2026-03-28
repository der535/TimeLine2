using EventBus;
using NaughtyAttributes;
using TimeLine.LevelEditor.CopyComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class ComponentUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform rootObject;
        [Space] [SerializeField] private RectTransform componentTransform;
        [SerializeField] private Button button;
        [FormerlySerializedAs("copyComponentButton")] [SerializeField] private ComponentContextController componentContextController;

        [ShowNonSerializedField] private float _height;

        public RectTransform RootObject => rootObject;
        private TrackObjectStorage _trackObjectStorage;

        private ComponentVisiblyStorage _componentVisiblyStorage;
        private GameEventBus _gameEventBus;

        private bool _isVisible = true;
        private string _componentName;

        [Inject]
        private void Construct(GameEventBus gameEventBus, ComponentVisiblyStorage componentVisiblyStorage,
            TrackObjectStorage trackObjectStorage)
        {
            _gameEventBus = gameEventBus;
            _componentVisiblyStorage = componentVisiblyStorage;
            _trackObjectStorage = trackObjectStorage;
        }

        public void Setup(ComponentNames name,  Entity entity, bool isRemovable)
        {
            _height += text.rectTransform.sizeDelta.y;
            _componentName = name.ToString();
            text.text = name.ToString();
            
            componentContextController.Setup(name, entity, isRemovable);
            
            if (_componentVisiblyStorage.GetVisibility(name.ToString()) == null)
            {
                _componentVisiblyStorage.SetVisibility(name.ToString(), true);
            }
            else if (_componentVisiblyStorage.GetVisibility(name.ToString()) == true)
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
            _isVisible = false;
            rootObject.gameObject.SetActive(false);
            componentTransform.sizeDelta = new Vector2(componentTransform.sizeDelta.x, text.rectTransform.sizeDelta.y);
            _componentVisiblyStorage.SetVisibility(_componentName, _isVisible);
            print(_componentVisiblyStorage.GetVisibility(_componentName));
        }

        public void Show()
        {
            _isVisible = true;
            componentTransform.sizeDelta = new Vector2(componentTransform.sizeDelta.x, _height);
            rootObject.gameObject.SetActive(true);
            _componentVisiblyStorage.SetVisibility(_componentName, _isVisible);
        }

        public void AddHeight(float height)
        {
            _height += height;
            if (_isVisible) componentTransform.sizeDelta = new Vector2(componentTransform.sizeDelta.x, _height);
        }
    }
}