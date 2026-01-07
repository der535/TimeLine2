using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class KeyframeEditWindow : MonoBehaviour
    {
        [SerializeField] private TMP_InputField timeInput;
        [SerializeField] private TextMeshProUGUI timePlaceHolder;
        [SerializeField] private TMP_InputField valueInput;
        [SerializeField] private TextMeshProUGUI valuePlaceHolder;

        [Space] [SerializeField] private TMP_Dropdown _dropDown;
        [Space] [SerializeField] private KeyframeSelectController keyframeSelectController;

        private GameEventBus _gameEventBus;
        private FloatInputValidator _floatInputValidators;
        private M_KeyframeSelectedStorage _selectedKeyframesStorage;

        [Inject]
        private void Construct(GameEventBus gameEventBus, M_KeyframeSelectedStorage selectedKeyframesStorage)
        {
            _gameEventBus = gameEventBus;
            _selectedKeyframesStorage = selectedKeyframesStorage;
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
            if (_selectedKeyframesStorage.Keyframes == null || _selectedKeyframesStorage.Keyframes.Count == 0)
                return;

            // Очищаем старые слушатели перед настройкой
            timeInput.onEndEdit.RemoveAllListeners();
            valueInput.onEndEdit.RemoveAllListeners();
            _dropDown.onValueChanged.RemoveAllListeners();

            // --- 1. Логика проверки идентичности данных ---
            var firstKey = _selectedKeyframesStorage.Keyframes[0];
            double sameTime = firstKey.Ticks;
            var sameValue = firstKey.GetData().GetValue().ToString();
            var sameTypeValue = firstKey.GetData().GetValue().GetType();
            var sameInterpolation = firstKey.Interpolation;

            bool isSameTime = true;
            bool isSameValue = true;
            bool isSameType = true;
            bool isSameInterpolation = true;

            foreach (var wrapper in _selectedKeyframesStorage.Keyframes)
            {
                var k = wrapper;
                if (Math.Abs(sameTime - k.Ticks) > 0.001) isSameTime = false;
                if (sameValue != k.GetData().GetValue().ToString()) isSameValue = false;
                if (sameTypeValue != k.GetData().GetValue().GetType()) isSameType = false;
                if (sameInterpolation != k.Interpolation) isSameInterpolation = false;
            }

            // --- 2. Настройка поля Времени (Ticks) ---
            if (isSameTime)
            {
                timePlaceHolder.text = string.Empty;
                timeInput.text = ((int)sameTime).ToString();
            }
            else
            {
                timePlaceHolder.text = "---";
                timeInput.text = string.Empty;
            }

            timeInput.onEndEdit.AddListener(val =>
            {
                if (int.TryParse(val, out int newTicks))
                {
                    foreach (var k in _selectedKeyframesStorage.Keyframes)
                        k.Ticks = newTicks;
                }
            });

            // --- 3. Настройка поля Значения ---
            if (!isSameType)
            {
                valueInput.text = "Different types";
                valueInput.interactable = false;
            }
            else
            {
                valueInput.interactable = true;
                if (isSameValue)
                {
                    valuePlaceHolder.text = string.Empty;
                    valueInput.text = sameValue;
                }
                else
                {
                    valuePlaceHolder.text = "---";
                    valueInput.text = string.Empty;
                }

                _floatInputValidators?.Dispose();
                _floatInputValidators = new FloatInputValidator(valueInput, f =>
                {
                    foreach (var k in _selectedKeyframesStorage.Keyframes)
                        k.GetData().SetValue(f);
                });
            }

            // --- 4. Настройка Интерполяции (Dropdown) ---
            if (isSameInterpolation)
            {
                SelectType(sameInterpolation);
            }
            else
            {
                _dropDown.SetValueWithoutNotify(0); // Показываем "---"
            }

            _dropDown.onValueChanged.AddListener(index =>
            {
                if (index == 0) return; // Не меняем ничего, если выбрано "---"
                
                string selectedText = _dropDown.options[index].text;
                if (Enum.TryParse(selectedText, out Keyframe.Keyframe.InterpolationType newType))
                {
                    foreach (var k in _selectedKeyframesStorage.Keyframes)
                        k.Interpolation = newType;
                }
            });
        }

        private void OnDestroy()
        {
            _floatInputValidators?.Dispose();
        }
    }
}