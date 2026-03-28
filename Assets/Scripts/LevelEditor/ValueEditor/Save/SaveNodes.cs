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
        

        public string SaveGraphToJson(List<Node> activeNodes)
        {
            var graphData = new GraphSaveData();

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

            return JsonConvert.SerializeObject(graphData, settings);
        }

        public OutputLogic LoadGraph(string json, DataType type, List<IInitializedNode> initializedNodes, List<TrackObjectPacket> objects = null)
        {
            OutputLogic outputLogic = null;
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto // Позволяет восстанавливать типы, если они указаны в JSON
            };
            
            _nodeCreator.SetListIInitializedNodes(initializedNodes);

            var data = JsonConvert.DeserializeObject<GraphSaveData>(json, settings);
            var idToNode = new Dictionary<string, Node>();

            foreach (var nEntry in data.Nodes)
            {
                // 1. Создаем логику через Reflection
                Type logicType = Type.GetType(nEntry.TypeFullName);
                global::NodeLogic logic = (global::NodeLogic)Activator.CreateInstance(logicType);
                _container.Inject(logic);
                logic.Id = nEntry.Id;

                if (logic is OutputLogic output)
                {
                    output.Initialize(type);
                    outputLogic = output;
                }

                if (logic is ComponentFieldLogic fieldLogic)
                {
                    fieldLogic.Load(nEntry.AdditionalData, objects);
                    var parameter = fieldLogic.GetField();
                    fieldLogic.AddOutputDefinition(parameter.Item2.ParameterID, TypeToDataType.Convert(typeof(float)));
                }

                // 2. Исправляем типы в ManualValues
                FixManualValues(nEntry.ManualValues, logic);

                // 3. Создаем визуальную ноду (через твой Creator)
                Node newNode = _nodeCreator.CreateNode(logic, logicType.Name, nEntry.Position);
                idToNode[logic.Id] = newNode;
            }
            
            // 4. Восстанавливаем связи (как обсуждали ранее)
            _nodeConnection.RestoreConnections(data.Connections, idToNode);
            return outputLogic;
        }

        public (OutputLogic, List<IInitializedNode>) LoadLogicOnly(string json, DataType type, List<TrackObjectPacket> objects = null)
        {
            if (string.IsNullOrEmpty(json))
            {
                var outputLogic = new OutputLogic();
                outputLogic.Initialize(type);
                return (_nodeCreator.CreateNode(outputLogic), null);
            }
            
            
            
            List<IInitializedNode> listNodes = new List<IInitializedNode>();
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var data = JsonConvert.DeserializeObject<GraphSaveData>(json, settings);

            Dictionary<string, global::NodeLogic> idToLogic = new();
            OutputLogic finalNode = null;

            // 1. Создаем только объекты логики
            foreach (var nEntry in data.Nodes)
            {
                Type logicType = Type.GetType(nEntry.TypeFullName);
                global::NodeLogic logic = (global::NodeLogic)Activator.CreateInstance(logicType);
                _container.Inject(logic);


                logic.Id = nEntry.Id;

                // Если это наш Output — запоминаем его как точку входа для вычислений
                if (logic is OutputLogic output)
                {
                    output.Initialize(type); // Тот самый метод инициализации
                    finalNode = output;
                }

                if (logic is ComponentFieldLogic fieldLogic)
                {
                    fieldLogic.Load(nEntry.AdditionalData,objects);
                }

                if (logic is IInitializedNode initialized)
                {
                    listNodes.Add(initialized);
                }

                // Заполняем ManualValues (не забудь FixManualValues для типов)
                FixManualValues(nEntry.ManualValues, logic);

                idToLogic.Add(logic.Id, logic);
            }

            // 2. Связываем логику (без визуальных линий!)
            foreach (var cData in data.Connections)
            {
                if (idToLogic.TryGetValue(cData.OutNodeId, out var outL) &&
                    idToLogic.TryGetValue(cData.InNodeId, out var inL))
                {
                    inL.ConnectInput(cData.InIndex, outL, cData.OutIndex);
                }
            }

            return (finalNode, listNodes); // Возвращаем "голову" графа
        }


        private void FixManualValues(Dictionary<int, object> values, global::NodeLogic logic)
        {
            foreach (var key in new List<int>(values.Keys))
            {
                object val = values[key];

                // Newtonsoft часто парсит float как double
                if (val is double d) val = (float)d;
                if (val is long l) val = (int)l;

                // Если в JSON это пришло как JObject (например, Color), 
                // нам нужно вручную конвертировать его обратно в тип Unity
                if (val is Newtonsoft.Json.Linq.JObject jObject)
                {
                    // Определяем, какой тип ожидается в этом порту
                    var portType = logic.InputDefinitions[key].type;

                    if (portType == DataType.Color)
                        val = jObject.ToObject<Color>();
                    else if (portType == DataType.Vector2)
                        val = jObject.ToObject<Vector2>();
                }

                logic.ManualValues[key] = val;
            }
        }
    }
}