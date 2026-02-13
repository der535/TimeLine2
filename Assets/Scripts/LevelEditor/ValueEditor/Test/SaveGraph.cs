using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TimeLine.LevelEditor.ValueEditor.Test
{
    public static class SaveGraph
    {
        public static string ToJson(OutputLogic cogic)
        {
            var graphData = new GraphSaveData();


            // 1. Формируем запись о ноде
            var nEntry = new NodeSaveEntry
            {
                Id = Guid.NewGuid().ToString(),
                TypeFullName = cogic.GetType().AssemblyQualifiedName,
                Position = new Vector2(0,0),
                // Просто копируем словарь. Newtonsoft сам разберется с object (float, Color и т.д.)
                ManualValues = new Dictionary<int, object>(cogic.ManualValues),
            };

            graphData.Nodes.Add(nEntry);
            

            // Настройки для красивого JSON и поддержки типов
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto // Это сохранит информацию о типах внутри object
            };

            return JsonConvert.SerializeObject(graphData, settings);
        }
    }
}