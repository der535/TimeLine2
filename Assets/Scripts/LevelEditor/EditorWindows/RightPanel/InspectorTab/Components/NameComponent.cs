using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
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
            Name.OnValueChanged += () =>
            {
                gameObject.name = Name.Value;
                TrackObjectData data = _storage.GetTrackObjectData(gameObject);
                if (data != null)
                {
                    data.branch.Rename(Name.Value);
                    data.trackObject.Rename(Name.Value);
                }
            };
        }

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return new StringParameter("NameComponent", "empty");
        }
    }
}