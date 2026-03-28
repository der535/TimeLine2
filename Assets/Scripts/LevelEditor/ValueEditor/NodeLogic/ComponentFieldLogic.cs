using System.Collections.Generic;
using Newtonsoft.Json;
using TimeLine;
using TimeLine.LevelEditor.Misk;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using Unity.Entities;
using UnityEngine;
using Zenject;

public class ComponentFieldLogic : NodeLogic
{
    public Entity Entity;
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

    public (Entity, MapParameterComponen) GetField()
    {
        return (Entity, _Map);
    }

    public override void OnLoad(Dictionary<string, object> data)
    {
        if (data.TryGetValue("Map", out var mapObj))
        {
            // Newtonsoft восстанавливает вложенные объекты как JObject
            if (mapObj is Newtonsoft.Json.Linq.JObject jObj)
                _Map = jObj.ToObject<MapParameterComponen>();
            else
                _Map = (MapParameterComponen)mapObj;
            
            var entity = _FindField.Find(_Map);
            
            if (_Map != null && entity != null)
                Entity = (Entity)entity;
        }
    }

    public void Load(Dictionary<string, object> data, List<TrackObjectPacket> objects = null)
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
            
            var entity = _FindField.Find(_Map, objects);
            
            if (_Map != null && entity != null)
                Entity = (Entity)entity;
        }
    }

    public override object GetValue(int outputIndex = 0) => AnimationDataResolver.GetValue(_Map.ParameterID, Entity,
        World.DefaultGameObjectInjectionWorld.EntityManager);
}