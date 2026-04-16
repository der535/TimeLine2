using System;
using System.Collections.Generic;
using System.Linq;
using TimeLine;
using TimeLine.LevelEditor.GeneralServices;
using TimeLine.LevelEditor.Misk;
using TimeLine.LevelEditor.ValueEditor;
using Unity.Entities;
using UnityEngine;
using Zenject;

public class ComponentFieldLogic : NodeLogic
{
    public Entity Entity;
    public string idMap; //Сохраняемый 
    
    bool isInitialized = false;

    private FindField _FindField;

    [Inject]
    private void Constructor(FindField findField)
    {
        if (isInitialized == false)
        {
            _FindField = findField;
            idMap = UniqueIDGenerator.GenerateUniqueID();
            Debug.Log($"Start Id {idMap}");
            isInitialized = true;
        }
    }

    public override void OnSave(Dictionary<string, object> data)
    {
        data["IDMap"] = idMap; // Сохраняем наш объект под ключом "IDMap"
    }

    public string GetField()
    {
        return idMap;
    }

    public void Load(Dictionary<string, object> data, List<TrackObjectPacket> objects = null)
    {
        // Безопасное получение значения из словаря (игнорируя регистр для надежности)
        var key = data.Keys.FirstOrDefault(k => k.Equals("IDMap", StringComparison.OrdinalIgnoreCase));
    
        if (key != null && data.TryGetValue(key, out var mapObj))
        {
            // 1. Извлекаем строку максимально надежно
            if (mapObj is Newtonsoft.Json.Linq.JToken jToken)
            {
                idMap = jToken.ToString(); // JToken сам знает, как превратиться в строку
            }
            else
            {
                idMap = mapObj?.ToString();
            }

            Debug.Log($"Loaded Id: {idMap}");

            // 2. Логика поиска сущности
            if (!string.IsNullOrEmpty(idMap))
            {
                var storageValue = MapParameterStorage.Get(idMap);
                if (storageValue != null)
                {
                    var foundEntity = _FindField.Find(storageValue, objects);
                    if (foundEntity != null)
                    {
                        Entity = foundEntity.Value;
                    }
                }
            }
        }
        else 
        {
            Debug.LogError("Key 'IDMap' not found in dictionary!");
        }
    }

    public override object GetValue(int outputIndex = 0) => AnimationDataResolver.GetValue(MapParameterStorage.Get(idMap).ParameterID, Entity,
        World.DefaultGameObjectInjectionWorld.EntityManager);
}