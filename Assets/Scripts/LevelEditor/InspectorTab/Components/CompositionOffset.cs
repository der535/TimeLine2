using System;
using System.Collections.Generic;
using System.Globalization;
using NaughtyAttributes;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
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
        private void Construct(TrackObjectStorage trackObjectStorage)
        {
            _trackObjectStorage = trackObjectStorage;
        }

        [Button]
        private void PrintOffset()
        {
            print(XOffset.Value);
            print(YOffset.Value);
        }

        private void Start()
        {
            List<TransformComponent> components = new();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.TryGetComponent<TransformComponent>(out var comp))
                {
                    components.Add(comp);
                }
            }

            var save = gameObject.GetComponent<TransformComponent>();
            var values = (save.XPositionOffset.Value, save.YPositionOffset.Value);

            foreach (var comp in components)
            {
                comp.XPositionOffset.Value = XOffset.Value;
                comp.YPositionOffset.Value = YOffset.Value;
            }

            save.XPositionOffset.Value = values.Item1;
            save.YPositionOffset.Value = values.Item2;
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

        public override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return XOffset;
            yield return YOffset;
        }
    }
}