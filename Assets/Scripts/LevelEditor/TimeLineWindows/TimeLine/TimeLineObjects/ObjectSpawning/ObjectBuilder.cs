// using System.Collections.Generic;
// using TimeLine.Components;
// using TimeLine.Keyframe;
// using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.ComponentsLogic;
// using TimeLine.Parent;
// using UnityEngine;
// using Zenject;
//
// namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning
// {
//     public class ObjectBuilder
//     {
//         private ObjectFactory _objectFactory;
//         private TrackStorage _trackStorage;
//         private BranchCollection _branchCollection;
//         private TrackObjectStorage _trackObjectStorage;
//         private KeyframeTrackStorage _keyframeTrackStorage;
//         private Main _main;
//         private DiContainer _container;
//         private SaveComposition _saveComposition;
//         private ParentLinkRestorer _parentLinkRestorer;
//
//         //State
//         private GameObject _sceneObject;
//         private TrackObject _trackObject;
//         private Branch _branch;
//         private TrackObjectData _trackObjectData;
//         private List<ComponentData> _componentDatas;
//         private List<TreeNodeSaveData> _branchNodes;
//         private List<TrackSaveData> tracks;
//
//         public ObjectBuilder(ObjectFactory objectFactory, TrackStorage trackStorage,
//             BranchCollection branchCollection, TrackObjectStorage trackObjectStorage,
//             KeyframeTrackStorage keyframeTrackStorage, Main main, SaveComposition saveComposition,
//             DiContainer container, ParentLinkRestorer parentLinkRestorer)
//         {
//             _objectFactory = objectFactory;
//             _trackStorage = trackStorage;
//             _branchCollection = branchCollection;
//             _trackObjectStorage = trackObjectStorage;
//             _keyframeTrackStorage = keyframeTrackStorage;
//             _saveComposition = saveComposition;
//             _main = main;
//             _container = container;
//             _parentLinkRestorer = parentLinkRestorer;
//         }
//
//         public ObjectBuilder Reset()
//         {
//             //Clear
//             return this;
//         }
//
//         public ObjectBuilder CreateSceneObject()
//         {
//             _sceneObject = _objectFactory.CreateSceneObject();
//             return this;
//         }
//
//         public ObjectBuilder AddComponents(List<ComponentData> componentDatas)
//         {
//             _componentDatas = componentDatas;
//             return this;
//         }
//
//         public ObjectBuilder CreateTrackObject(double startTime, double duractionTime, string gameObjectName,
//             int lineIndex)
//         {
//             _trackObject = _objectFactory.CreateTrackObject(duractionTime, gameObjectName,
//                 _trackStorage.GetTrackLineByIndex(lineIndex), startTime);
//             return this;
//         }
//
//         public ObjectBuilder SetNodesBranch(List<TreeNodeSaveData> Nodes)
//         {
//             _branchNodes = Nodes;
//             return this;
//         }
//
//         public ObjectBuilder CreateBranch(string branchId, string branchName)
//         {
//             _branch = _branchCollection.AddBranch(branchId, branchName);
//
//             return this;
//         }
//
//         public ObjectBuilder Bild(string sceneObjectID, bool addToStorage)
//         {
//             //Добавляем необходимые компоненты
//             foreach (var component in _componentDatas)
//             {
//                 IParameterComponent parameterComponent =
//                     (IParameterComponent)ComponentRules.GetOrAddComponentSafely(component.ComponentType, _sceneObject,
//                         _container);
//                 parameterComponent.SetParameterData(component.Parameters);
//             }
//             
//             if (addToStorage)
//             {
//                 // Добавляем в хранилище
//                 _trackObjectData = _trackObjectStorage.Add(_sceneObject, _trackObject, _branch, sceneObjectID);
//             }
//             else
//             {
//                 _trackObjectData = new TrackObjectData(_sceneObject, _trackObject, _branch, sceneObjectID);
//             }
//
//
//         }
//     }
// }