using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
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
        private Entity _selected;
        private GameEventBus _gameEventBus;
        private TrackObjectStorage _trackObjectStorage;
        private EntityComponentController _controller;

        [Inject]
        private void Construct(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage,
            EntityComponentController entityComponentController)
        {
            _controller = entityComponentController;
            _gameEventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
        }

        internal void SetActiveComponentWindow(bool active) => componentWindow.gameObject.SetActive(active);

        private void AddComponent(string componentName, Action onClick)
        {
            ComponentLine componentLine = Instantiate(componentPrefab, root.transform);
            componentLine.Setup(componentName, onClick);
            _components.Add(componentLine);
        }

        public void UpdateComponents(Entity _target)
        {
            foreach (var component in _components)
            {
                Destroy(component.gameObject);
            }

            _components.Clear();

            _selected = _target;
            List<ComponentNames> components = _controller.GetAllTheComponentsThatCanBeAdded(_target);

            foreach (var component in components)
            {
                AddComponent(component.ToString(), () =>
                {
                    componentWindow.gameObject.SetActive(false);
                    _controller.AddComponentSafely(component, _target);
                    _gameEventBus.Raise(new AddComponentEvent(_trackObjectStorage.GetTrackObjectData(_target),
                        component, _target));
                    UpdateComponents(_selected);
                });
            }
        }
    }
}