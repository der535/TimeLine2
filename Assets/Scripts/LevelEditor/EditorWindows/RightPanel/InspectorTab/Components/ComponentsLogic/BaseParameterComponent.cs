using System.Collections.Generic;
using TimeLine;
using TimeLine.CustomInspector.Logic;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.ComponentsLogic;
using TimeLine.LevelEditor.GeneralEditor;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;
using Zenject;
using KeyframeCreator = TimeLine.KeyframeCreator;



public abstract class BaseParameterComponent : MonoBehaviour, IParameterComponent
{
    private KeyframeCreator _keyframeCreator;
    private TimeLineRecorder _timeLineRecorder;
    
    [Inject]
    private void BaseConstruct(KeyframeCreator keyframeCreator, TimeLineRecorder timeLineRecorder)
    {
        _keyframeCreator = keyframeCreator;
        _timeLineRecorder = timeLineRecorder;
    }
    
    /// <summary>
    /// Связывает любой параметр с логикой Unity и автоматической записью ключей.
    /// </summary>
    /// <typeparam name="TValue">Тип значения (float, Color, и т.д.)</typeparam>
    /// <typeparam name="TParam">Тип класса параметра (FloatParameter, ColorParameter, и т.д.)</typeparam>
    protected void Bind<TValue, TParam>(
        TParam param, 
        System.Func<TValue, AnimationData> dataFactory, 
        System.Action<TValue> applyLogic) 
        where TParam : InspectableParameter
    {
        // Подписываемся на изменение значения
        param.OnValueChanged += () => 
        {
            // 1. Получаем текущее типизированное значение через рефлексию или приведение
            // Так как в InspectableParameter значение лежит в объекте, приводим его
            TValue currentVal = (TValue)param.GetValue();

            // 2. Применяем логику к объекту в Unity
            applyLogic?.Invoke(currentVal);
            
            // 3. Если идет запись — создаем ключ
            if (_timeLineRecorder.IsRecording() && UpdatingFromAnimation.isUpdatingFromAnimation == false)
            {
                CreateKeyframeManual(dataFactory(currentVal), param);
            }
        };
    }
    
    /// <summary>
    /// Метод для принудительного создания ключа (используется в Drawer-ах)
    /// </summary>
    public void CreateKeyframeManual(AnimationData data, InspectableParameter param)
    {
        _keyframeCreator.CreateKeyframe(data, gameObject, GetType().Name, param);
    }
    
    protected abstract IEnumerable<InspectableParameter> GetParameters();

    public virtual Dictionary<string, ParameterPacket> GetParameterData()
    {
        var data = new Dictionary<string, ParameterPacket>();
        foreach (var param in GetParameters())
        {
            if (data.ContainsKey(param.Name))
            {
                Debug.LogError($"Duplicate parameter name: {param.Name}");
                continue;
            }

            data[param.Name] = new ParameterPacket 
            { 
                Id = param.Id, 
                Value = param.GetValue() 
            };
        }
        return data;
    }

    public virtual void SetParameterData(Dictionary<string, ParameterPacket> data)
    {
        foreach (var param in GetParameters())
        {
            // Пытаемся найти данные по имени параметра
            if (data.TryGetValue(param.Name, out var packet))
            {
                // Опционально: проверяем соответствие ID, если это критично
                // if (param.Id != packet.Id) { ... }

                param.SetValue(packet.Value);
                param.Id = packet.Id;
            }
        }
    }

    public virtual string GetComponentTypeName() => this.GetType().Name;
}

