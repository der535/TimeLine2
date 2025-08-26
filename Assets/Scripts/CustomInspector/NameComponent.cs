using System;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class NameComponent : MonoBehaviour, ICopyableComponent
    {
        public StringParameter Name = new("Object name", "");

        private TrackObjectStorage _storage;
        
        [Inject]
        private void Construct(TrackObjectStorage storage)
        {
            _storage = storage;
        }
        
        private void Awake()
        {
            Name.Value = gameObject.name;

            Name.OnValueChanged += () =>
            {
                print("Изменил");
                gameObject.name = Name.Value;
                TrackObjectData data = _storage.GetTrackObjectData(gameObject);
                data.branch.Rename(Name.Value);
                data.trackObject.Rename(Name.Value);
            };
        }

        public void CopyTo(Component targetComponent)
        {
            if (targetComponent is NameComponent other)
            {
                other.Name.Value = Name.Value;
            }
            else
            {
                throw new ArgumentException("Target component must be of type NameComponent");
            }
        }

        public Component Copy(GameObject targetGameObject)
        {
            var component = targetGameObject.GetComponent<NameComponent>();
            CopyTo(component);
            return component;
        }
    }
}