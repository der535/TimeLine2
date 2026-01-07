using System.Collections.Generic;
using TimeLine;
using TimeLine.CustomInspector.Logic;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.GeneralEditor;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;
using Zenject;
using KeyframeCreator = TimeLine.KeyframeCreator;

public interface IParameterComponent
{
    Dictionary<string, object> GetParameterData();
    void SetParameterData(Dictionary<string, object> data);
    string GetComponentTypeName(); // ← новое
}

public abstract class BaseParameterComponent : MonoBehaviour, IParameterComponent
{
    protected KeyframeCreator KeyframeCreator;
    protected TimeLineRecorder TimeLineRecorder;
    
    [Inject]
    private void BaseConstruct(KeyframeCreator keyframeCreator, TimeLineRecorder timeLineRecorder)
    {
        KeyframeCreator = keyframeCreator;
        TimeLineRecorder = timeLineRecorder;
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
            if (TimeLineRecorder.IsRecording() && UpdatingFromAnimation.isUpdatingFromAnimation == false)
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
        KeyframeCreator.CreateKeyframe(data, gameObject, GetType().Name, param);
    }
    
    protected abstract IEnumerable<InspectableParameter> GetParameters();

    public virtual Dictionary<string, object> GetParameterData()
    {
        var data = new Dictionary<string, object>();
        foreach (var param in GetParameters())
        {
            if (data.ContainsKey(param.Name))
            {
                Debug.LogError($"Duplicate parameter name: {param.Name} in {GetType().Name}");
                continue;
            }
            data[param.Name] = param.GetValue();
        }
        return data;
    }

    public virtual void SetParameterData(Dictionary<string, object> data)
    {
        foreach (var param in GetParameters())
        {
            if (data.TryGetValue(param.Name, out var value))
            {
                param.SetValue(value);
            }
        }
    }

    public virtual string GetComponentTypeName() => this.GetType().Name;
}

public class ParameterPacket
{
    public string Id;
    public object Value;
}