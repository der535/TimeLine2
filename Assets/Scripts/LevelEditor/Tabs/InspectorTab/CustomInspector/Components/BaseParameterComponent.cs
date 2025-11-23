using System.Collections.Generic;
using TimeLine;
using TimeLine.CustomInspector.Logic;
using UnityEngine;

public interface IParameterComponent
{
    Dictionary<string, object> GetParameterData();
    void SetParameterData(Dictionary<string, object> data);
    string GetComponentTypeName(); // ← новое
}

public abstract class BaseParameterComponent : MonoBehaviour, ICopyableComponent, IParameterComponent
{
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
    public abstract void CopyTo(Component targetComponent);
    public abstract Component Copy(GameObject targetGameObject);
}