using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using TimeLine.LevelEditor.ValueEditor.Test;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.Save
{
    public class SaveNodes : MonoBehaviour
    {
        private NodeCreator _nodeCreator;
        private NodeConnector _nodeConnection;
        private DiContainer _container;

        private string _save;

        [Inject]
        private void Construct(NodeCreator nodeCreator, NodeConnector nodeConnector, DiContainer container)
        {
            _nodeCreator = nodeCreator;
            _nodeConnection = nodeConnector;
            _container = container;
        }
        private static readonly Dictionary<string, Type> _typeCache = new();

        private Type GetCachedType(string typeName)
        {
            if (!_typeCache.TryGetValue(typeName, out var type))
            {
                type = Type.GetType(typeName);
                _typeCache[typeName] = type;
            }
            return type;
        }

        public GraphSaveData SaveGraphToJson(List<Node> activeNodes)
        {
            var graphData = new GraphSaveData(DataType.Float, new List<NodeSaveEntry>(), new List<ConnectionSaveEntry>());

            
            foreach (var node in activeNodes)
            {
                // 1. Формируем запись о ноде
                var nEntry = new NodeSaveEntry
                {
                    Id = node.Logic.Id,
                    TypeFullName = node.Logic.GetType().AssemblyQualifiedName,
                    Position = node.GetComponent<RectTransform>().anchoredPosition,
                    // Просто копируем словарь. Newtonsoft сам разберется с object (float, Color и т.д.)
                    ManualValues = new Dictionary<int, object>(node.Logic.ManualValues),
                };

                if (node.Logic is ComponentFieldLogic componentFieldLogic)
                {
                    componentFieldLogic.OnSave(nEntry.AdditionalData);
                }

                if (node.Logic is OutputLogic nodeLogic)
                {
                    graphData.OutputType = nodeLogic.DataType;
                }
                
                graphData.Nodes.Add(nEntry);

                // 2. Сохраняем только входящие связи ноды (чтобы не дублировать их)
                foreach (var connPair in node.Logic.ConnectedInputs)
                {
                    graphData.Connections.Add(new ConnectionSaveEntry
                    {
                        InNodeId = node.Logic.Id,
                        InIndex = connPair.Key,
                        OutNodeId = connPair.Value.node.Id,
                        OutIndex = connPair.Value.outputIndex
                    });
                }
            }

            // Настройки для красивого JSON и поддержки типов
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto // Это сохранит информацию о типах внутри object
            };

            return graphData;
        }

        public OutputLogic LoadGraph(GraphSaveData json, DataType type, List<IInitializedNode> initializedNodes, List<TrackObjectPacket> objects = null)
        {
            OutputLogic outputLogic = null;
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto // Позволяет восстанавливать типы, если они указаны в JSON
            };
            
            if(initializedNodes != null)
                _nodeCreator.SetListIInitializedNodes(initializedNodes);

            // var data = JsonConvert.DeserializeObject<GraphSaveData>(json, settings);
            var idToNode = new Dictionary<string, Node>();

            foreach (var nEntry in json.Nodes)
            {
                // 1. Создаем логику через Reflection
                Type logicType = Type.GetType(nEntry.TypeFullName);
                global::NodeLogic logic = (global::NodeLogic)Activator.CreateInstance(logicType);
                _container.Inject(logic);
                logic.Id = nEntry.Id;

                if (logic is OutputLogic output)
                {
                    output.Initialize(json.OutputType);
                    outputLogic = output;
                }

                if (logic is ComponentFieldLogic fieldLogic)
                {
                    fieldLogic.Load(nEntry.AdditionalData, objects);
                    var parameter = fieldLogic.GetField();
                    fieldLogic.AddOutputDefinition(MapParameterStorage.Get(parameter).ParameterID, TypeToDataType.Convert(typeof(float)));
                }

                // 2. Исправляем типы в ManualValues
                FixManualValues(nEntry.ManualValues, logic);

                // 3. Создаем визуальную ноду (через твой Creator)
                Node newNode = _nodeCreator.CreateNode(logic, logicType.Name, nEntry.Position);
                idToNode[logic.Id] = newNode;
            }
            
            // 4. Восстанавливаем связи (как обсуждали ранее)
            _nodeConnection.RestoreConnections(json.Connections, idToNode);
            return outputLogic;
        }

        public (OutputLogic, List<IInitializedNode>) LoadLogicOnly(GraphSaveData json, DataType type, List<IInitializedNode> initializedNodes = null, List<TrackObjectPacket> objects = null)
        {
            if (json?.Nodes == null)
            {
                var outputLogic = new OutputLogic();
                outputLogic.Initialize(type);
                return (_nodeCreator.CreateNode(outputLogic), null);
            }

            int nodeCount = json.Nodes.Count;
            var listNodes = new List<IInitializedNode>(nodeCount); // Задаем емкость заранее!
            var idToLogic = new Dictionary<string, global::NodeLogic>(nodeCount);
            OutputLogic finalNode = null;

            // 1. Создаем объекты логики
            foreach (var nEntry in json.Nodes)
            {
                Type logicType = GetCachedType(nEntry.TypeFullName);
                if (logicType == null) continue;

                var logic = (global::NodeLogic)Activator.CreateInstance(logicType);
                _container.Inject(logic);
                logic.Id = nEntry.Id;

                // Pattern Matching (C# 7+) работает быстрее, чем множественные as/is
                switch (logic)
                {
                    case OutputLogic output:
                        output.Initialize(type);
                        finalNode = output;
                        break;
                
                    case ComponentFieldLogic fieldLogic:
                        fieldLogic.Load(nEntry.AdditionalData, objects);
                        break;
                }

                if (logic is IInitializedNode initialized)
                    listNodes.Add(initialized);

                FixManualValues(nEntry.ManualValues, logic);
                idToLogic.Add(logic.Id, logic);
            }

            // 2. Связываем логику
            // Используем for вместо foreach для массивных данных, если Connections - это List
            var connections = json.Connections;
            for (int i = 0; i < connections.Count; i++)
            {
                var cData = connections[i];
                if (idToLogic.TryGetValue(cData.OutNodeId, out var outL) &&
                    idToLogic.TryGetValue(cData.InNodeId, out var inL))
                {
                    inL.ConnectInput(cData.InIndex, outL, cData.OutIndex);
                }
            }

            return (finalNode, listNodes);
        }


        private void FixManualValues(Dictionary<int, object> values, global::NodeLogic logic)
        {
            if (values == null) return;

            // Итерируемся напрямую по KeyValuePair, чтобы не создавать лишних списков
            foreach (var kvp in values)
            {
                int key = kvp.Key;
                object val = kvp.Value;

                // Быстрое приведение базовых типов
                if (val is double d) val = (float)d;
                else if (val is long l) val = (int)l;
                // Если это JObject (сложный тип вроде Color/Vector)
                else if (val is Newtonsoft.Json.Linq.JObject jObject)
                {
                    var portType = logic.InputDefinitions[key].type;
                    val = ConvertJObject(jObject, portType);
                }

                logic.ManualValues[key] = val;
            }
        }

        private object ConvertJObject(Newtonsoft.Json.Linq.JObject jObject, DataType type)
        {
            // Ручное извлечение из JObject работает быстрее, чем .ToObject<T>()
            switch (type)
            {
                case DataType.Color:
                    return new Color(
                        (float)(jObject["r"] ?? 0),
                        (float)(jObject["g"] ?? 0),
                        (float)(jObject["b"] ?? 0),
                        (float)(jObject["a"] ?? 1)
                    );
                case DataType.Vector2:
                    return new Vector2(
                        (float)(jObject["x"] ?? 0),
                        (float)(jObject["y"] ?? 0)
                    );
                // Добавьте другие типы по необходимости
                default:
                    return jObject; 
            }
        }
    }
}