using System;
using System.Collections.Generic;
using System.Globalization;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Tabs.InspectorTab.Keyframe.KeyframeTimeLine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class KeyframeEditWindow : MonoBehaviour
    {
        [FormerlySerializedAs("_timeInpute")] [SerializeField]
        private TMP_InputField _timeInput;

        [FormerlySerializedAs("_timeInpute")] [SerializeField]
        private TextMeshProUGUI _timePlaceHolder;

        [FormerlySerializedAs("_valueInpute")] [SerializeField]
        private TMP_InputField _valueInput;

        [FormerlySerializedAs("_valueInpute")] [SerializeField]
        private TextMeshProUGUI _valuePlaceHolder;

        [Space] [SerializeField] private TMP_Dropdown _dropDown;
        [Space] [SerializeField] private KeyframeSelectController keyframeSelectController;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _dropDown.ClearOptions();
            _dropDown.AddOptions(new List<string>()
            {
                "---",
                Keyframe.Keyframe.InterpolationType.Linear.ToString(),
                Keyframe.Keyframe.InterpolationType.Bezier.ToString(),
                Keyframe.Keyframe.InterpolationType.Hold.ToString(),
            });

            _gameEventBus.SubscribeTo<SelectKeyframeEvent>(Setup);
            TMP_Text placeholderText = _timeInput.placeholder.GetComponent<TMP_Text>();
        }

        private void SelectType(Keyframe.Keyframe.InterpolationType type)
        {
            for (int i = 0; i < _dropDown.options.Count; i++)
            {
                if (_dropDown.options[i].text == type.ToString())
                {
                    _dropDown.value = i;
                    return;
                }
            }
        }

        private void Setup(ref SelectKeyframeEvent _)
        {
            double sameTime = keyframeSelectController.SelectedKeyframe[0].Keyframe.Ticks;
            bool isSameTime = true;

            var sameValue = keyframeSelectController.SelectedKeyframe[0].Keyframe.GetData().GetValue().ToString();
            bool isSameValue = true;

            var sameTypeValue = keyframeSelectController.SelectedKeyframe[0].Keyframe.GetData().GetValue().GetType()
                .ToString();
            bool isSameType = true;

            var sameInterpolationTypeValue = keyframeSelectController.SelectedKeyframe[0].Keyframe.Interpolation;
            bool isSameInterpolationType = true;

            foreach (var keyframe in keyframeSelectController.SelectedKeyframe)
            {
                if (sameTime != keyframe.Keyframe.Ticks)
                {
                    isSameTime = false;
                }

                if (sameValue != keyframe.Keyframe.GetData().GetValue().ToString())
                {
                    isSameValue = false;
                }

                if (sameTypeValue != keyframe.Keyframe.GetData().GetValue().GetType().ToString())
                {
                    isSameType = false;
                }

                if (sameInterpolationTypeValue != keyframe.Keyframe.Interpolation)
                {
                    isSameInterpolationType = false;
                }
            }

            _timeInput.onEndEdit.RemoveAllListeners();
            _valueInput.onEndEdit.RemoveAllListeners();

            if (isSameTime)
            {
                _timePlaceHolder.text = string.Empty;
                _timeInput.text = ((int)sameTime).ToString();
            }
            else
            {
                _timePlaceHolder.text = "---";
                _timeInput.text = string.Empty;
            }


            foreach (var keyframe in keyframeSelectController.SelectedKeyframe)
            {
                _timeInput.onEndEdit.AddListener(arg0 => keyframe.Keyframe.Ticks = int.Parse(arg0));
            }

            if (isSameType == false)
            {
                _valueInput.text = "Different data types";
                return;
            }

            if (isSameValue)
            {
                _valuePlaceHolder.text = string.Empty;
                _valueInput.text = sameValue;
            }
            else
            {
                _valuePlaceHolder.text = "---";
                _valueInput.text = string.Empty;
            }

            foreach (var keyframe in keyframeSelectController.SelectedKeyframe)
            {
                _valueInput.onEndEdit.AddListener(arg0 => keyframe.Keyframe.GetData().SetValue((float)int.Parse(arg0)));
            }

            if (isSameInterpolationType)
            {
                SelectType(sameInterpolationTypeValue);
            }
            else
            {
                _dropDown.value = 0;
                return;
            }
            
            _dropDown.onValueChanged.RemoveAllListeners();
            foreach (var keyframe in keyframeSelectController.SelectedKeyframe)
            {
                _dropDown.onValueChanged.AddListener(value =>
                {
                    keyframe.Keyframe.Interpolation = Enum.Parse<Keyframe.Keyframe.InterpolationType>(_dropDown.options[value].text);
                });
            }
            
        }
    }
}