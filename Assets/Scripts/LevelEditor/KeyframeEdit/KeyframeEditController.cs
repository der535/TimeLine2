using System;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.KeyframeEdit
{
    public class KeyframeEditController : MonoBehaviour
    {
        [SerializeField] private KeyframeEditView keyframeEditView;

        private GameEventBus _gameEventBus;
        private FloatInputValidator _floatInputValidators;
        private KeyframeSelectedStorage _selectedKeyframesStorage;

        [Inject]
        private void Construct(GameEventBus gameEventBus, KeyframeSelectedStorage selectedKeyframesStorage)
        {
            _gameEventBus = gameEventBus;
            _selectedKeyframesStorage = selectedKeyframesStorage;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo<SelectKeyframeEvent>(Setup);
            _gameEventBus.SubscribeTo<DeselectAllKeyframeEvent>(DeselectKeyframes);
        }
        

        private void DeselectKeyframes(ref DeselectAllKeyframeEvent _)
        {
            keyframeEditView.Clear();
        }

        private void Setup(ref SelectKeyframeEvent _)
        {
            if (_selectedKeyframesStorage.Keyframes == null || _selectedKeyframesStorage.Keyframes.Count == 0)
            {
                keyframeEditView.SetInteractable(false);
                return;
            }

            keyframeEditView.SetInteractable(true);
            

            // --- 1. Логика проверки идентичности данных ---
            var firstKey = _selectedKeyframesStorage.Keyframes[0];
            double sameTime = firstKey.Ticks;
            // var sameValue = firstKey.GetData().GetValue();
            // var sameTypeValue = firstKey.GetData().GetValue().GetType();
            var sameInterpolation = firstKey.Interpolation;

            bool isSameTime = true;
            // bool isSameValue = true;
            // bool isSameType = true;
            bool isSameInterpolation = true;

            foreach (var wrapper in _selectedKeyframesStorage.Keyframes)
            {
                var k = wrapper;
                if (Math.Abs(sameTime - k.Ticks) > 0.001) isSameTime = false;
                // if (sameValue != k.GetData().GetValue()) isSameValue = false;
                // if (sameTypeValue != k.GetData().GetValue().GetType()) isSameType = false;
                if (sameInterpolation != k.Interpolation) isSameInterpolation = false;
            }

            // --- 2. Настройка поля Времени (Ticks) ---
            if (isSameTime)
            {
                keyframeEditView.SetValueToTimeInput(string.Empty, ((int)sameTime).ToString());
            }
            else
            {
                keyframeEditView.SetValueToTimeInput("---", string.Empty);
            }
            
            keyframeEditView.TimeInputOnEdit((value) =>
            {
                foreach (var k in _selectedKeyframesStorage.Keyframes)
                    k.Ticks = value;
            });
            

            // // --- 3. Настройка поля Значения ---
            // if (!isSameType)
            // {
            //     keyframeEditView.SetTextInputValue("Different types");
            //     keyframeEditView.SetValueInputInteractable(false);
            // }
            // else
            // {
            //     keyframeEditView.SetValueInputInteractable(true);
            //     if (isSameValue)
            //     {
            //         keyframeEditView.SetTextInputValuePlaceHolder( string.Empty);
            //         keyframeEditView.SetTextInputValue(sameValue);
            //     }
            //     else
            //     {
            //         keyframeEditView.SetTextInputValuePlaceHolder("---");
            //         keyframeEditView.SetTextInputValue(string.Empty);
            //     }
            //
            //     keyframeEditView.InputValueOnChange((value) =>
            //     {
            //         foreach (var k in _selectedKeyframesStorage.Keyframes)
            //             k.GetData().SetValue(value);
            //     });
            //     
            // }

            // --- 4. Настройка Интерполяции (Dropdown) ---
            if (isSameInterpolation)
            {
                keyframeEditView.SelectType(sameInterpolation);
            }
            else
            {
                keyframeEditView.SetValueDropdown(0); // Показываем "---"
            }

            keyframeEditView.DropDownOnValueChanged((data) =>
            {
                foreach (var k in _selectedKeyframesStorage.Keyframes)
                    k.Interpolation = data;
            });
        }

        private void OnDestroy()
        {
            _floatInputValidators?.Dispose();
        }
    }
}