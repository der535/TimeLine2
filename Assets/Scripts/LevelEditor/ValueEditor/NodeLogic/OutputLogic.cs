using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.ValueEditor;
using TimeLine.LevelEditor.ValueEditor.Test;
using UnityEngine;

public class OutputLogic : NodeLogic
{
    public DataType DataType;

    public OutputLogic()
    {
    } // Теперь Activator не будет ругаться

    public void Initialize(DataType type)
    {
        DataType = type;
        InputDefinitions = new List<(string name, DataType type)>
        {
            ("Result", type)
        };
    }

    public override object GetValue(int outputIndex = 0)
    {
        var rawValue = GetInputValue(0, 0);
        if (rawValue is JObject jObject)
        {
            return GetValue(jObject);
        }
        else
        {
            return rawValue;
        }
    }

    public object GetValue(JObject jObject)
    {
        var rawValue = jObject;
        // Если это JObject (пришло из JSON графа)
        if (rawValue is JToken token)
        {
            return token.ToObject<Color>();
        }

        return jObject;
    }
}