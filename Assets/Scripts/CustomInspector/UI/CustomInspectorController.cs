using System.Collections.Generic;
using EventBus;
using TimeLine.CustomInspector.UI;
using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CustomInspectorController : MonoBehaviour
    {
        [SerializeField] private CustomInspectorDrawer inspectorDrawer;
        [SerializeField] private RectTransform rootObject;
        [SerializeField] private KeyframeCreater keyframeCreater;
        
        [Space]
        [SerializeField] private ComponentUI componentUIPrefab;

        private List<IComponentDrawer> _componentDrawers = new();

        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Awake()
        {
            // _gameEventBus.SubscribeTo((ref SelectSceneObject data) => Draw(data.GameObject));
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) => Draw(data.Track.sceneObject));
            
            
            _componentDrawers.Add(new TransformComponentDrawer());
            _componentDrawers.Add(new RandomTransformComponentDrawer());
            _componentDrawers.Add(new DynamicTransformDrawer());
            _componentDrawers.Add(new ParentDrawer());
            _componentDrawers.Add(new NameDrawer());
        }
        
        private void Draw(GameObject target)
        {
            foreach (Transform child in rootObject)
                Destroy(child.gameObject);

            var components = target.GetComponents<Component>();

            foreach (var component in components)
            {
                foreach (var drawer in _componentDrawers)
                {
                    if (drawer.GetComponent(component))
                    {
                        drawer.Setup(inspectorDrawer, keyframeCreater);
                        drawer.Draw(component, target);
                    }
                }
            }
        }
    }
}