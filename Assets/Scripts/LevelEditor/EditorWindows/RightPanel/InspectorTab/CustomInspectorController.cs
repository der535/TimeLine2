using System.Collections;
using System.Collections.Generic;
using EventBus;
using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.CustomInspector
{
    public class CustomInspectorController : MonoBehaviour
    {
        [SerializeField] private CustomInspectorDrawer inspectorDrawer;
        [SerializeField] private RectTransform rootObject;
        [FormerlySerializedAs("keyframeCreater")] [SerializeField] private KeyframeCreator keyframeCreator;
        
        [Space]
        [SerializeField] private ComponentUI componentUIPrefab;

        private List<IComponentDrawer> _componentDrawers = new();

        private GameEventBus _gameEventBus;

        private GameObject _selectedObject;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) => Draw(data.Tracks[^1].sceneObject));
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => Draw(data.SelectedObjects[^1].sceneObject));
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => Clear());
            _gameEventBus.SubscribeTo((ref AddComponentEvent data) =>
            {
                StartCoroutine(Redraw());
            }, -1);
            _gameEventBus.SubscribeTo((ref RemoveComponentEvent data) =>
            {
                StartCoroutine(Redraw());
            }, -1);
            
            _componentDrawers.Add(new TransformComponentDrawer());
            _componentDrawers.Add(new RandomTransformComponentDrawer());
            _componentDrawers.Add(new DynamicTransformDrawer());
            _componentDrawers.Add(new NameDrawer());
            _componentDrawers.Add(new SpriteRendererDrawer());
            _componentDrawers.Add(new BoxCollider2DDrawer());
            _componentDrawers.Add(new CircleCollider2DDrawer());
            _componentDrawers.Add(new CapsuleCollider2DDrawer());
            _componentDrawers.Add(new EdgeCollider2DDrawer());
            _componentDrawers.Add(new PressEventDrawer());
            _componentDrawers.Add(new ShakeDrawer());
        }

        internal IEnumerator Redraw()
        {
            yield return new WaitForEndOfFrame();
            print("Redraw".ToUpper());
            if(_selectedObject != null)
                Draw(_selectedObject);
        }
        
        private void Draw(GameObject target)
        {
            _selectedObject = target;
            
            Clear();

            var components = target.GetComponents<Component>();

            foreach (var component in components)
            {
                foreach (var drawer in _componentDrawers)
                {
                    if (drawer.GetComponent(component))
                    {
                        drawer.Setup(inspectorDrawer, keyframeCreator);
                        drawer.Draw(component, target);
                    }
                }
            }
            
            inspectorDrawer.CreateAddComponentButton(target);

        }

        private void Clear()
        {
            foreach (Transform child in rootObject)
                Destroy(child.gameObject);
        }
    }
}