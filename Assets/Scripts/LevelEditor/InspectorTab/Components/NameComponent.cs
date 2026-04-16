using System.Collections.Generic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using Zenject;

namespace TimeLine.LevelEditor.InspectorTab.Components
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
                TrackObjectPacket packet = _storage.GetTrackObjectData(gameObject);
                if (packet != null)
                {
                    packet.branch.Rename(Name.Value);
                    packet?.components?.View?.Rename(Name.Value);
                }
            };
        }

        public override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return new StringParameter("NameComponent", "empty");
        }
    }
}