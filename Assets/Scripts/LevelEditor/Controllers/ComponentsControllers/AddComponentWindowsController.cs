using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine.Components
{

    public class AddComponentWindowsController : MonoBehaviour
    {
        [SerializeField] private RectTransform componentWindow;
        [SerializeField] private RectTransform root;
        [SerializeField] private ComponentLine componentPrefab;
        
        private List<ComponentLine> _components = new();
        private GameObject _selected;
        private GameEventBus _gameEventBus;
        private TrackObjectStorage _trackObjectStorage;
        private DiContainer _container;

        [Inject]
        private void Construct(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage, DiContainer container)
        {
            _gameEventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
            _container = container;
        }
        internal void SetActiveComponentWindow(bool active) => componentWindow.gameObject.SetActive(active);

        private void AddComponent(string componentName, Action onClick)
        {
            ComponentLine componentLine = Instantiate(componentPrefab, root.transform);
            componentLine.Setup(componentName, onClick);
            _components.Add(componentLine);
        }
        
        public void UpdateComponents(GameObject _target)
        {
            foreach (var component in _components)
            {
                Destroy(component.gameObject);
            }
            
            _components.Clear();
            
            _selected = _target;
            Dictionary<string, Type> components = ComponentRules.GetAllComponents(_target);

            foreach (var component in components)
            {
                AddComponent(component.Key, () =>
                {
                    componentWindow.gameObject.SetActive(false);
                    Component comp = ComponentRules.AddComponentSafely(component.Value, _target, _container);
                    _gameEventBus.Raise(new AddComponentEvent(_trackObjectStorage.GetTrackObjectData(_target), comp));
                    UpdateComponents(_selected);
                });
            }
        }
    }
}
