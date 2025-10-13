using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class NameComponent : BaseParameterComponent
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
                print(data);
                data.branch.Rename(Name.Value);
                data.trackObject.Rename(Name.Value);
            };
        }

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return Name;
        }

        public override void CopyTo(Component targetComponent)
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

        public override Component Copy(GameObject targetGameObject)
        {
            var component = targetGameObject.GetComponent<NameComponent>();
            CopyTo(component);
            return component;
        }
    }
}