using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using TimeLine.Components;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.ComponentsLogic;
using TimeLine.LevelEditor.GeneralServices;
using TimeLine.LevelEditor.MaxObjectIndex.Controller;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using TimeLine.LevelEditor.ValueEditor.Save;
using TimeLine.LevelEditor.ValueEditor.Test;
using TimeLine.Parent;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning
{
    public class ObjectLoader
    {
        private ObjectFactory _objectFactory;
        private TrackStorage _trackStorage;
        private BranchCollection _branchCollection;
        private TrackObjectStorage _trackObjectStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private Main _main;
        private DiContainer _container;
        private SaveComposition _saveComposition;
        private ParentLinkRestorer _parentLinkRestorer;
        private IMaxObjectIndexDataReading _maxObjectIndexDataReading;
        private SaveNodes _saveNodes;

        public ObjectLoader(ObjectFactory objectFactory, TrackStorage trackStorage,
            BranchCollection branchCollection, TrackObjectStorage trackObjectStorage,
            KeyframeTrackStorage keyframeTrackStorage, Main main, SaveComposition saveComposition,
            DiContainer container, ParentLinkRestorer parentLinkRestorer,
            IMaxObjectIndexDataReading maxObjectIndexDataReading, SaveNodes saveNodes)
        {
            _objectFactory = objectFactory;
            _trackStorage = trackStorage;
            _branchCollection = branchCollection;
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
            _saveComposition = saveComposition;
            _main = main;
            _container = container;
            _parentLinkRestorer = parentLinkRestorer;
            _maxObjectIndexDataReading = maxObjectIndexDataReading;
            _saveNodes = saveNodes;
        }

        public (TrackObjectPacket, GameObject, Branch, List<Track>) LoadObject(GameObjectSaveData data,
            bool addToStorage = true, bool generateNewSceneID = false, bool addToTitleCloneText = false,
            bool loadGraph = true, bool createTrackObject = true)
        {
            List<Track> tracks = new List<Track>();

            var oldId = data.sceneObjectID;

            string id = UniqueIDGenerator.GenerateUniqueID();
            string sceneId;
            if (generateNewSceneID == false) sceneId = data.sceneObjectID;
            else
            {
                sceneId = UniqueIDGenerator.GenerateUniqueID();
            }

            // Создаем сценный объект
            GameObject sceneObject = _objectFactory.CreateSceneObject().Item1;

            // Создаем трек-объект
            TrackObjectComponents trackObject;

            var name = addToTitleCloneText
                ? NameCloner.NameClonerService.Clone(data.gameObjectName, _maxObjectIndexDataReading)
                : data.gameObjectName;

            trackObject = _objectFactory.CreateTrackObject(data.duractionTime, name,
                data.lineIndex, data.startTime, createTrackObject);

            // Создаем ветку
            Branch branch = _branchCollection.AddBranch(id, data.branch.Name);

            // Добавляем ноды в ветку
            foreach (var node in data.branch.Nodes)
            {
                branch.AddNode(node.Path);
            }

            TrackObjectPacket trackObjectPacket;

            //Условие где мы добовляем в хранилище трекобжект или нет.
            //Не добовление может использоваться только в случае если мы сохдаём трек обжект который будет внутри группы
            if (addToStorage)
            {
                // Добавляем в хранилище
                trackObjectPacket =
                    _trackObjectStorage.Add(sceneObject.gameObject, trackObject, branch, sceneId);
            }
            else
            {
                trackObjectPacket = new TrackObjectPacket(sceneObject.gameObject, trackObject, branch, sceneId);
            }

            //Добавляем необходимые компоненты
            foreach (var component in data.Components)
            {
                IParameterComponent parameterComponent =
                    (IParameterComponent)ComponentRules.GetOrAddComponentSafely(component.ComponentType, sceneObject,
                        _container);
                parameterComponent.SetParameterData(component.Parameters);
                if (!string.IsNullOrEmpty(component.id)) parameterComponent.SetID(component.id);
            }

            // Добавляем трек и ключевые кадры
            foreach (var track in data.tracks)
            {
                Track trackm = new Track(sceneObject, track.branchPath, track.animationColor);
                _keyframeTrackStorage.AddTrack(branch.FindNode(track.branchPath).node, trackm,
                    trackObjectPacket.components.Data,
                    branch.ID);
                tracks.Add(trackm);


                foreach (var saveData in track.keyframeSaveData)
                {
                    if (!string.IsNullOrEmpty(saveData.Graph))
                    {
                        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                        var graph = JsonConvert.DeserializeObject<GraphSaveData>(saveData.Graph, settings);
                        foreach (var nodes in graph.Nodes)
                        {
                            foreach (var VARIABLE in nodes.AdditionalData)
                            {
                                if (VARIABLE.Value is MapParameterComponen map)
                                {
                                    if (map.SceneObjectID == oldId)
                                    {
                                        map.SceneObjectID = sceneId;
                                    }
                                }
                            }
                        }

                        saveData.Graph = JsonConvert.SerializeObject(graph, settings);
                    }

                    OutputLogic item1 = null;
                    List<IInitializedNode> item2 = null;
                    if (loadGraph)
                    {
                        (item1, item2) = _saveNodes.LoadLogicOnly(saveData.Graph,
                            TypeToDataType.Convert(saveData.DataType));
                    }

                    Keyframe.Keyframe keyframe = Keyframe.Keyframe.FromSaveData(saveData, item1, item2);
                    trackm.AddKeyframe(keyframe);
                }
            }

            sceneObject.GetComponent<NameComponent>().Name.Value = branch.Name;

            trackObjectPacket.components.Data.ParentID = data.parentObjectID;
            sceneObject.GetComponent<SceneObjectLink>().trackObjectPacket = trackObjectPacket;


            return (trackObjectPacket, sceneObject, branch, tracks);
        }

        internal void GenerateNewSceneIDs(GroupGameObjectSaveData rootData)
        {
            var idMap = new Dictionary<string, string>();
            // var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Debug.Log($"[SceneIDGen] Начало генерации новых ID для корня: {rootData.sceneObjectID}");

            // Шаг 1: Генерация
            BuildIdMap(rootData, idMap);

            // Шаг 2: Обновление ссылок
            UpdateParentReferences(rootData, idMap);

            //Шаг 3: Обновление карты
            UpdateMap(rootData, idMap);

            // stopwatch.Stop();
            // Debug.Log(
            // $"[SceneIDGen] Успешно завершено. Обработано объектов: {idMap.Count}. Время: {stopwatch.ElapsedMilliseconds}ms");
        }

        private void BuildIdMap(GameObjectSaveData data, Dictionary<string, string> idMap)
        {
            string oldId = data.sceneObjectID;
            string newId = UniqueIDGenerator.GenerateUniqueID();

            if (idMap.ContainsKey(oldId))
            {
                Debug.LogWarning($"[SceneIDGen] Обнаружен дубликат ID: {oldId}! Объект будет перезаписан в карте.");
            }

            idMap[oldId] = newId;
            data.sceneObjectID = newId;

            // Опционально: детальный лог для дебага (лучше закомментировать в продакшене)
            // Debug.Log($"[SceneIDGen] Переназначение: {oldId} -> {newId}");

            if (data is GroupGameObjectSaveData group && group.children != null)
            {
                foreach (var child in group.children)
                {
                    BuildIdMap(child, idMap);
                }
            }
        }

        private void UpdateParentReferences(GameObjectSaveData data, Dictionary<string, string> idMap)
        {
            if (!string.IsNullOrEmpty(data.parentObjectID))
            {
                if (idMap.TryGetValue(data.parentObjectID, out var newParentId))
                {
                    data.parentObjectID = newParentId;
                }
                else
                {
                    // Это важный момент: если родителя нет в карте, значит он вне текущей иерархии
                    Debug.LogWarning(
                        $"[SceneIDGen] Родительский ID {data.parentObjectID} для объекта {data.sceneObjectID} не найден в текущей сессии генерации.");
                }
            }

            if (data is GroupGameObjectSaveData group && group.children != null)
            {
                foreach (var child in group.children)
                {
                    UpdateParentReferences(child, idMap);
                }
            }
        }

        private void UpdateMap(GroupGameObjectSaveData data, Dictionary<string, string> idMap)
        {
            Debug.Log("UpdateMap");
            Debug.Log(data.tracks.Count);
            Debug.Log(JsonConvert.SerializeObject(data, Formatting.Indented));

            foreach (var VARIABLE in data.children)
            {
                foreach (var track in VARIABLE.tracks)
                {
                    Debug.Log(data.tracks.Count);
                    foreach (var kdata in track.keyframeSaveData)
                    {
                        Debug.Log(kdata);

                        if (!string.IsNullOrEmpty(kdata.Graph))
                        {
                            GraphSaveData graph = JsonConvert.DeserializeObject<GraphSaveData>(kdata.Graph);
                            foreach (var node in graph.Nodes)
                            {
                                Debug.Log(graph.Nodes.Count);

                                foreach (var keypair in node.AdditionalData)
                                {
                                    Debug.Log("AdditionalData");
                                    if (keypair.Value is MapParameterComponen map)
                                    {
                                        if (idMap.TryGetValue(map.SceneObjectID, out var newParentId))
                                        {
                                            Debug.Log("TryGetValue");
                                            map.SceneObjectID = newParentId;
                                        }
                                    }
                                }
                            }

                            kdata.Graph = JsonConvert.SerializeObject(graph, Formatting.Indented);
                        }
                    }
                }
            }

            foreach (var track in data.tracks)
            {
                Debug.Log(data.tracks.Count);
                foreach (var kdata in track.keyframeSaveData)
                {
                    Debug.Log(kdata);

                    if (!string.IsNullOrEmpty(kdata.Graph))
                    {
                        GraphSaveData graph = JsonConvert.DeserializeObject<GraphSaveData>(kdata.Graph);
                        foreach (var node in graph.Nodes)
                        {
                            Debug.Log(graph.Nodes.Count);

                            foreach (var keypair in node.AdditionalData)
                            {
                                Debug.Log("AdditionalData");
                                if (keypair.Value is MapParameterComponen map)
                                {
                                    if (idMap.TryGetValue(map.SceneObjectID, out var newParentId))
                                    {
                                        Debug.Log("TryGetValue");
                                        map.SceneObjectID = newParentId;
                                    }
                                }
                            }
                        }

                        kdata.Graph = JsonConvert.SerializeObject(graph, Formatting.Indented);
                    }
                }
            }
        }

        private double _savedCurrentTime;

        /// <summary>
        /// Позволяет загружать композицию из сохранения
        /// </summary>
        /// <param name="data">Сам объект сохранения</param>
        /// <param name="compositionID">Id загружаемой композиции</param>
        /// <param name="compositionData">Сохранение композиции из хранилища композиций, нужен потому что в основном объекте сохранения дочерние объекты не сохраняются</param>
        /// <param name="addToStorage">Флаг обозначающи добовлять ли композицию в обжее хранилище трекобжектов, отключение этой фунции нужно для вложенных композиций друг в друге</param>
        /// <param name="lastEditID">Id последнего редактирования</param>
        /// <returns></returns>
        internal (TrackObjectPacket, GameObject, Branch) LoadComposition(GroupGameObjectSaveData data,
            string compositionID,
            GroupGameObjectSaveData compositionData = null, bool addToStorage = true, string lastEditID = null,
            bool generateNewSceneID = false, bool addToTitleCloneText = false, bool currentTimeSaved = false, bool createTrackObject = true)
        {
            var (groupTrackObject, groupGameObject, _, _) = LoadObject(data, false, createTrackObject:createTrackObject); //Загружаем объект группы
            
            if (currentTimeSaved == false) _savedCurrentTime = TimeLineConverter.Instance.TicksCurrentTime();
            _main.SetTimeInTicks(groupTrackObject.components.Data
                .StartTimeInTicks); //Ставим время тамйлайна на старт группы что бы ничего не сьехало

            groupTrackObject.components.Data.ReducedLeft = data.reduceLeft;
            groupTrackObject.components.Data.ReducedRight = data.reduceRight;

            List<GameObjectSaveData> children; //Создаём список дочерных обьъектов
            if (compositionData == null)
            {
                children = data.children; //Если один объект сохранения то загружаем из основного
                Debug.Log(children.Count);
                if (generateNewSceneID) GenerateNewSceneIDs(data);
            }
            else
            {
                children = compositionData.children; //Если нет то загружаем из файла композиции
                if (generateNewSceneID) GenerateNewSceneIDs(compositionData);
            }

            List<TrackObjectPacket>
                trackObjectDatas = new List<TrackObjectPacket>(); //Пустой список дочерних объектов композиции
            List<Track> childAllTracks = new List<Track>();
            foreach (var childData in children.ToList()) //Перебираем детей из списка в сохранении
            {
                //Инициализируем пустые поля
                TrackObjectPacket childTrackObject = null;
                GameObject childSceneObject = null;
                Branch childBranch = null;
                List<Track> childTracks = new List<Track>();

                if (childData is GroupGameObjectSaveData childGroupData) //Если дочерний объект тоже композиция
                {
                    // Debug.Log("first");

                    GroupGameObjectSaveData groupChildData = _saveComposition.FindCompositionDataById(childGroupData
                        .compositionID); //Ищем по Id в галереии композиций файл сохранения где лежат сохраннёные дочерние объекты


                    childGroupData.lastEditID = groupChildData.lastEditID;


                    if (groupChildData != null) //Если нашли, загружаем
                    {
                        (childTrackObject, childSceneObject, childBranch) = LoadComposition(childGroupData,
                            childGroupData.compositionID,
                            groupChildData, generateNewSceneID: false,
                            addToTitleCloneText: addToTitleCloneText, currentTimeSaved: true,  createTrackObject:false); //Рекурсивная загружка
                    }
                }
                else //Если дочерний объект не композиция
                {
                    (childTrackObject, childSceneObject, childBranch, childTracks) =
                        LoadObject(childData, generateNewSceneID: false,
                            addToTitleCloneText: addToTitleCloneText, loadGraph: false, createTrackObject:false); //Загружаем обычный объект
                }

                if (childTrackObject != null) //Если дочерний объект не пустой
                {
                    trackObjectDatas.Add(childTrackObject); //Добовляем в список дочерних объектов

                    ///////////////////////////////
                    // Debug.Log($"childTrackObject StartTimeInTicks{ childTrackObject.trackObject.StartTimeInTicks}");
                    // Debug.Log($"data.reduceLeft{ data.reduceLeft}");
                    ////////////////////////////////
                    childTrackObject.components.Data.StartTimeInTicks += data.reduceLeft;
                    ////////////////////////////////


                    //Сохраняем трансформ дочернего объекта
                    Vector3 pos = childSceneObject.transform.localPosition;
                    Quaternion rot = childSceneObject.transform.localRotation;
                    Vector3 sca = childSceneObject.transform.localScale;

                    //Делаем парент дочернего объекта
                    childSceneObject.transform.SetParent(groupGameObject.gameObject.transform);

                    //Востанавливаем прежние значения
                    childSceneObject.transform.localPosition = pos;
                    childSceneObject.transform.localRotation = rot;
                    childSceneObject.transform.localScale = sca;

                    foreach (var node in childBranch.Nodes) //Перебираем ноды в ветке анимации
                    {
                        foreach (var node2 in node.Children) //Перебираем ноды в ветке анимации
                        {
                            _keyframeTrackStorage.GetTrack(node2)?.SetParent(groupTrackObject.components.Data);
                        }
                    }
                }

                childAllTracks.AddRange(childTracks);
            }


            foreach (var track in childAllTracks)
            {
                foreach (var saveData in track.Keyframes)
                {
                    (OutputLogic item1, List<IInitializedNode> item2) = _saveNodes.LoadLogicOnly(
                        saveData.GetData().Graph,
                        TypeToDataType.Convert(saveData.GetData().GetType()), trackObjectDatas);
                    saveData.GetData().Logic = item1;
                    saveData.GetData().initializedNodes = item2;
                }
            }

            ParentLinkRestorer.Restor(trackObjectDatas); //Восстанавливаем парент
            
            if (compositionData == null)
            {
                TrackObjectData TrackObjectData = new TrackObjectData((float)data.duractionTime, data.gameObjectName,
                    data.lineIndex,
                    data.parentObjectID, data.startTime, data.reduceLeft, data.reduceRight, true);
                
                
                groupTrackObject.components.Setup(TrackObjectData); //
            }
            else
            {
                TrackObjectData TrackObjectData = new TrackObjectData((float)data.duractionTime, compositionData.gameObjectName,
                    data.lineIndex,
                    data.parentObjectID, data.startTime, data.reduceLeft, data.reduceRight, true); 
                
                
                groupTrackObject.components.Setup(TrackObjectData);
                groupTrackObject.components.Data.UpdateDuraction(compositionData.duractionTime);
            }
            
            TrackObjectGroup trackObjectGroup =
                _trackObjectStorage.AddGroup(groupGameObject.gameObject, groupTrackObject.components,
                    groupTrackObject.branch, trackObjectDatas,
                    groupTrackObject.sceneObjectID, compositionID,
                    string.IsNullOrEmpty(data.lastEditID) ? compositionData?.lastEditID : data.lastEditID,
                    addToStorage); // ???????

            groupGameObject.GetComponent<NameComponent>().Name.Value = groupTrackObject.branch.Name;

            _main.SetTimeInTicks(_savedCurrentTime); // Возвращяем время назад

            groupGameObject.GetComponent<SceneObjectLink>().trackObjectPacket = trackObjectGroup;
            
            return (trackObjectGroup, groupGameObject, groupTrackObject.branch);
        }
    }
}