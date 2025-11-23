using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class SpriteParameter : InspectableParameter
    {
        private Sprite _value;
        public Sprite Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public SpriteParameter(string name, Sprite initialValue, Color animationColor) 
            : base(name, typeof(Sprite))
        {
            _value = initialValue;
            AnimationColor = animationColor;
        }
        public override object GetValue()
        {
            // Сохраняем ТОЛЬКО имя спрайта
            return _value?.name ?? string.Empty;
        }
        public override void SetValue(object value)
        {
            // Случай 1: получили сам Sprite (например, при копировании)
            if (value is Sprite spriteValue)
            {
                Value = spriteValue;
                return;
            }

            // Случай 2: получили имя спрайта как строку (из JSON)
            if (value is string spriteName)
            {
                if (string.IsNullOrEmpty(spriteName))
                {
                    Value = null;
                    return;
                }

                // Пытаемся найти спрайт по имени
                Sprite foundSprite = null;
                
                // Вариант B: используем глобальный синглтон (если нет resolver'а)
                if (BaseSpriteStorage.Instance != null)
                {
                    foundSprite = BaseSpriteStorage.Instance.GetSprite(spriteName);
                }

                if (foundSprite != null)
                {
                    Value = foundSprite;
                }
                else
                {
                    Debug.LogWarning($"Sprite '{spriteName}' not found in sprite storage!");
                    Value = null;
                }
                return;
            }

            // Случай 3: неизвестный тип
            Debug.LogWarning($"Cannot assign {value?.GetType()} to SpriteParameter. Expected Sprite or string.");
            Value = null;
        }
    }
}