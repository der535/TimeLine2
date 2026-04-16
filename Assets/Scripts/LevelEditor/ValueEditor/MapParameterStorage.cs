using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TimeLine.LevelEditor.LevelJson;
using UnityEngine;

namespace TimeLine.LevelEditor.ValueEditor
{
    public static class MapParameterStorage
    {
        static Dictionary<string, MapParameterComponen> _parameters = new Dictionary<string, MapParameterComponen>();

        public static void Add(string id, MapParameterComponen m)
        {
            _parameters.Add(id, m);
        }

        public static void Remove(string id)
        {
            _parameters.Remove(id);
        }

        public static MapParameterComponen Get(string id)
        {
            return _parameters.GetValueOrDefault(id);
        }

        public static void UpdateSceneObjectID(string oldID, string newID)
        {
            foreach (var pair in _parameters)
            {
                if( pair.Value.SceneObjectID == oldID) pair.Value.SceneObjectID = newID;
            }
        }
        
        public static void UpdateSceneObjectID(Dictionary<string, string> newIDMap)
        {
            foreach (var pair in newIDMap)
            {
                UpdateSceneObjectID(pair.Key, pair.Value);
            }
        }
        

        public static void Save()
        {
            File.WriteAllText(SavePathController.GetJsonPath(LevelJsonStorage.MapParameters),
                JsonConvert.SerializeObject(_parameters));
        }

        public static void Load()
        {
            if(File.Exists(SavePathController.GetJsonPath(LevelJsonStorage.MapParameters)))
                _parameters = JsonConvert.DeserializeObject<Dictionary<string, MapParameterComponen>>(File.ReadAllText(SavePathController.GetJsonPath(LevelJsonStorage.MapParameters)));
        }
    }
}