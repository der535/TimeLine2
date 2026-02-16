using System.Collections.Generic;
using Newtonsoft.Json;
using TimeLine;
using TimeLine.LevelEditor.Misk;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;
using Zenject;

public class ComponentFieldLogic : NodeLogic
{
    public InspectableParameter _parameter;
    public MapParameterComponen _Map;

    private FindField _FindField;

    [Inject]
    private void Constructor(FindField findField)
    {
        _FindField = findField;
    }

    public override void OnSave(Dictionary<string, object> data)
    {
        data["Map"] = _Map; // Сохраняем наш объект под ключом "Map"
    }

    public InspectableParameter GetField()
    {
        return _parameter;
    }

    public override void OnLoad(Dictionary<string, object> data)
    {
        Debug.Log(JsonConvert.SerializeObject(data));
        if (data.TryGetValue("Map", out var mapObj))
        {
            // Newtonsoft восстанавливает вложенные объекты как JObject
            if (mapObj is Newtonsoft.Json.Linq.JObject jObj)
                _Map = jObj.ToObject<MapParameterComponen>();
            else
                _Map = (MapParameterComponen)mapObj;
            
            if (_Map != null)
                _parameter = _FindField.Find(_Map);
        }
    }

    public void Load(Dictionary<string, object> data, List<TrackObjectData> objects = null)
    {
        if (objects == null)
        {
            OnLoad(data);
            return;
        }

        if (data.TryGetValue("Map", out var mapObj))
        {
            // Newtonsoft восстанавливает вложенные объекты как JObject
            if (mapObj is Newtonsoft.Json.Linq.JObject jObj)
                _Map = jObj.ToObject<MapParameterComponen>();
            else
                _Map = (MapParameterComponen)mapObj;

            if (_Map != null)
                _parameter = _FindField.Find(_Map, objects);
        }
    }

    public override object GetValue(int outputIndex = 0) => _parameter.GetValue();
}