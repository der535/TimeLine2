using System;
using System.Collections.Generic;
using System.Globalization;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CompositionOffset : BaseParameterComponent
    {
        private TrackObjectStorage _trackObjectStorage;
        private List<TransformComponent> components = new();
        
        public FloatParameter XOffset = new("X-Composition-Offset", 0, Color.red);
        public FloatParameter YOffset = new("Y-Composition-Offset", 0, Color.red);

        [Inject]
        private void Construct(TrackObjectStorage trackObjectStorage)
        {
            _trackObjectStorage = trackObjectStorage;
        }

        private void Start()
        {
            Find();
        }

        private void Find(TrackObjectGroup trackObjectGroup = null)
        {
            if (components.Count != 0) return;

            if (trackObjectGroup == null) 
                trackObjectGroup = (TrackObjectGroup)_trackObjectStorage.GetTrackObjectData(gameObject);

            foreach (var variable in trackObjectGroup.TrackObjectDatas)
            {
                TransformComponent transformComponent = variable.sceneObject.GetComponent<TransformComponent>();
                
                components.Add(transformComponent);
                transformComponent.XPositionOffset.Value = XOffset.Value;
                transformComponent.YPositionOffset.Value = YOffset.Value;
            }
        }

        internal Vector2 Setup(TMP_InputField xOffset, TMP_InputField yOffset, TrackObjectGroup trackObjectGroup)
        {
            Find(trackObjectGroup);

            foreach (var component in components)
            {
                xOffset.onEndEdit.AddListener(value =>
                {
                    XOffset.Value = float.Parse(value, CultureInfo.InvariantCulture);
                    component.XPositionOffset.Value = XOffset.Value;
                });
                yOffset.onEndEdit.AddListener(value =>
                {
                    YOffset.Value = float.Parse(value, CultureInfo.InvariantCulture);
                    component.YPositionOffset.Value = YOffset.Value;
                });
            }
            
            return new Vector2(XOffset.Value, YOffset.Value);
        }

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return XOffset;
            yield return YOffset;
        }

        public override void CopyTo(Component targetComponent)
        {
            if (targetComponent is CompositionOffset other)
            {
                other.XOffset = XOffset;
                other.YOffset = YOffset;
            }
            else
            {
                throw new ArgumentException("Target component must be of type CompositionOffset");
            }
        }

        public override Component Copy(GameObject targetGameObject)
        {
            var component = targetGameObject.AddComponent<CompositionOffset>();
            CopyTo(component);
            return component;
        }
    }
}