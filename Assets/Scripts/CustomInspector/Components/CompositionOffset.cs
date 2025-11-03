using System;
using System.Collections.Generic;
using System.Globalization;
using NaughtyAttributes;
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

        private TMP_InputField _xInputField;
        private TMP_InputField _yInputField;
        
        private DiContainer _container;

        [Inject]
        private void Construct(TrackObjectStorage trackObjectStorage, DiContainer container)
        {
            _trackObjectStorage = trackObjectStorage;
            _container = container;
            
            
        }

        [Button]
        private void PrintOffset()
        {
            print(XOffset.Value);
            print(YOffset.Value);
        }

        private void Start()
        {
            print(_trackObjectStorage);
             if(_trackObjectStorage == null) return;
            TrackObjectGroup trackObjectGroup = (TrackObjectGroup)_trackObjectStorage.GetTrackObjectData(gameObject);
            print(trackObjectGroup);
            if (trackObjectGroup != null)
            {
                Find(trackObjectGroup);
            }
        }


        private void Find(TrackObjectGroup trackObjectGroup = null)
        {
            components.Clear();

            if (trackObjectGroup == null)
                trackObjectGroup = (TrackObjectGroup)_trackObjectStorage.GetTrackObjectData(gameObject);

            if (trackObjectGroup == null) return;

            foreach (var variable in trackObjectGroup.TrackObjectDatas)
            {
                var transformComponent = variable.sceneObject.GetComponent<TransformComponent>();
                if (transformComponent != null)
                {//
                    components.Add(transformComponent);
                    transformComponent.XPositionOffset.Value = XOffset.Value;
                    transformComponent.YPositionOffset.Value = YOffset.Value;
                }
            }
        }

        internal void Unsubscribe()
        {
            if (_xInputField != null)
            {
                _xInputField.onEndEdit.RemoveListener(OnXOffsetChanged);
                _xInputField.onValueChanged.RemoveListener(OnXOffsetChanged);
                _xInputField = null;
            }

            if (_yInputField != null)
            {
                _yInputField.onEndEdit.RemoveListener(OnYOffsetChanged);
                _yInputField.onValueChanged.RemoveListener(OnXOffsetChanged);
                _yInputField = null;
            }
        }

        internal Vector2 Setup(TMP_InputField xOffset, TMP_InputField yOffset, TrackObjectGroup trackObjectGroup)
        {
            Unsubscribe(); // Отписываемся от старых полей, если были

            Find(trackObjectGroup);

            _xInputField = xOffset;
            _yInputField = yOffset;

            _xInputField.onValueChanged.AddListener(OnXOffsetChanged);
            _yInputField.onValueChanged.AddListener(OnYOffsetChanged);

            return new Vector2(XOffset.Value, YOffset.Value);
        }

        private void OnXOffsetChanged(string value)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                XOffset.Value = result;
                foreach (var comp in components)
                {
                    if (comp != null)
                        comp.XPositionOffset.Value = result;
                }
            }
        }

        private void OnYOffsetChanged(string value)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                YOffset.Value = result;
                foreach (var comp in components)
                {
                    if (comp != null)
                        comp.YPositionOffset.Value = result;
                }
            }
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
                other.XOffset.Value = XOffset.Value;
                other.YOffset.Value = YOffset.Value;
            }
            else
            {
                throw new ArgumentException("Target component must be of type CompositionOffset");
            }
        }

        public override Component Copy(GameObject targetGameObject)
        {
            var component = targetGameObject.AddComponent<CompositionOffset>();
            _container.Inject(component);
            CopyTo(component);
            return component;
        }
    }
}