// using System;
// using EventBus;
// using TimeLine;
// using TimeLine.Components;
// using TimeLine.EventBus.Events.TrackObject;
// using TimeLine.Input;
// using TimeLine.Keyframe;
// using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.CustomInspector;
// using TimeLine.LevelEditor.General;
// using TimeLine.LevelEditor.GeneralEditor;
// using TimeLine.LevelEditor.outline;
// using TimeLine.LevelEditor.Select_composition;
// using TimeLine.LevelEditor.SpriteLoader;
// using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
// using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
// using TimeLine.TimeLine;
// using UnityEngine;
// using UnityEngine.Serialization;
// using Zenject;
//
// namespace TimeLine.Installers
// {
//     public class TimeLineInstaller : MonoInstaller
//     {
//         [SerializeField] private TimeLineConverter timeLineConverter;
//         [SerializeField] private Main main;
//         [SerializeField] private BarBeatCounter barBeatCounter;
//         [SerializeField] private TimeLineRenderer timeLineRenderer;
//         [FormerlySerializedAs("timeMarkerRenderer")] [SerializeField] private TimeLineMarkerRenderer timeLineMarkerRenderer;
//         [SerializeField] private CurrentTimeMarkerRenderer currentTimeMarkerRenderer;
//         [SerializeField] private CursorBeatPosition cursorBeatPosition;
//         [FormerlySerializedAs("objectSpawner")] [FormerlySerializedAs("trackObjectSpawner")] [SerializeField] private FacadeObjectSpawner facadeObjectSpawner;
//         [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;
//         [SerializeField] private SettingPanel settingPanel;
//         [SerializeField] private BranchCollection branchCollection;
//         [SerializeField] private TrackObjectStorage trackObjectStorage;
//         [SerializeField] private TrackStorage trackStorage;
//         [SerializeField] private TreeViewUI treeViewUI;
//         [SerializeField] private TimeLineSettings settings;
//         [SerializeField] private TimeLineScroll timeLineScroll;
//         [FormerlySerializedAs("gridSystem")] [SerializeField] private GridUI gridUI;
//         [SerializeField] private ParentMain parentMain;
//         [SerializeField] private TimeLineKeyframeScroll timeLineKeyframeScroll;
//         [SerializeField] private SelectObjectController selectObjectController;
//         [SerializeField] private TrackObjectRemover trackObjectRemover;
//         [SerializeField] private GroupEdit groupEdit;
//         [SerializeField] private GroupCreater groupCreater;
//         [SerializeField] private ComponentVisiblyStorage componentVisiblyStorage;
//         [SerializeField] private AddComponentWindowsController addComponentWindowsController;
//         [SerializeField] private CustomInspectorController customInspectorController;
//         [SerializeField] private InitializedComponentController initializedComponentController;
//         [FormerlySerializedAs("verticalBezierPan")] [SerializeField] private VerticalBezierZoom verticalBezierZoom;
//         [SerializeField] private BezierLineDrawer bezierLineDrawer;
//         [SerializeField] private BezierController bezierController;
//         [SerializeField] private SelectSpriteController selectSpriteController;
//         [SerializeField] private SelectColorContoller selectColorContoller;
//         [SerializeField] private CollidersPrefab collidersPrefab;
//         [SerializeField] private SaveComposition saveComposition;
//         [SerializeField] private KeyfeameVizualizer keyfeameVizualizer;
//         [SerializeField] private BezierSelectPointsController bezierSelectPointsController;
//         [SerializeField] private FinishLevelController finishLevelController;
//         [SerializeField] private TimeLineMarkersController timeLineMarkersController;
//         [SerializeField] private SaveLevel saveLevel;
//         [SerializeField] private GridScene gridScene;
//         [FormerlySerializedAs("spriteStorage")] [SerializeField] private CustomSpriteStorage customSpriteStorage;
//         [SerializeField] private SpriteLoadController spriteLoadController;
//         [SerializeField] private GetSpriteName getSpriteName;
//         [SerializeField] private BaseSpriteStorage baseSpriteStorage;
//         [SerializeField] private EditColliderState editColliderState;
//         [SerializeField] private EdgeColliderSelectionCube edgeColliderSelectionCube;
//         [SerializeField] private SelectComposition selectComposition;
//         [SerializeField] private ShakeCamera shakeComponent;
//         [SerializeField] private SceneToRawImageConverter sceneToRawImageConverter;
//         [SerializeField] private CoordinateSystem coordinateSystem;
//         [SerializeField] private SaveEditorSettings saveEditorSettings;
//         [SerializeField] private SpriteOutlineBuffer spriteOutlineBuffer;
//         [SerializeField] private TimeLineRecorder timeLineRecorder;
//         [SerializeField] private KeyframeCreator keyframeCreator;
//         
//         [SerializeField] private MainObjects mainObjects;
//         
//         private GameEventBus _gameEventBus;
//         
//         // ReSharper disable Unity.PerformanceAnalysis
//         public override void InstallBindings()
//         {
//             Container.Bind<BaseSpriteStorage>().FromInstance(baseSpriteStorage);
//             Container.Bind<GetSpriteName>().FromInstance(getSpriteName).AsSingle();
//
//             Container.Bind<TimeLineConverter>().FromInstance(timeLineConverter).AsSingle();
//             Container.Bind<BarBeatCounter>().FromInstance(barBeatCounter).AsSingle();
//             Container.Bind<TimeLineRenderer>().FromInstance(timeLineRenderer).AsSingle();
//             Container.Bind<TimeLineMarkerRenderer>().FromInstance(timeLineMarkerRenderer).AsSingle();
//             Container.Bind<CurrentTimeMarkerRenderer>().FromInstance(currentTimeMarkerRenderer).AsSingle();
//             Container.Bind<CursorBeatPosition>().FromInstance(cursorBeatPosition).AsSingle();
//             Container.Bind<FacadeObjectSpawner>().FromInstance(facadeObjectSpawner).AsSingle();
//             Container.Bind<KeyframeTrackStorage>().FromInstance(keyframeTrackStorage).AsSingle();
//             Container.Bind<SettingPanel>().FromInstance(settingPanel).AsSingle();
//             Container.Bind<BranchCollection>().FromInstance(branchCollection).AsSingle();
//             Container.Bind<TrackObjectStorage>().FromInstance(trackObjectStorage).AsSingle();
//             Container.Bind<TrackStorage>().FromInstance(trackStorage).AsSingle();
//             Container.Bind<TreeViewUI>().FromInstance(treeViewUI).AsSingle();
//             Container.Bind<TimeLineScroll>().FromInstance(timeLineScroll).AsSingle();
//             // Container.Bind<SceneObjectAddKeyFrame>().FromInstance(sceneObjectAddKeyFrame).AsSingle();
//             Container.Bind<GridUI>().FromInstance(gridUI).AsSingle();
//             Container.Bind<ParentMain>().FromInstance(parentMain).AsSingle();
//             Container.Bind<TimeLineKeyframeScroll>().FromInstance(timeLineKeyframeScroll);
//             Container.Bind<SelectObjectController>().FromInstance(selectObjectController).AsSingle();
//             Container.Bind<TrackObjectRemover>().FromInstance(trackObjectRemover);
//             Container.Bind<GroupEdit>().FromInstance(groupEdit).AsSingle();
//             Container.Bind<GroupCreater>().FromInstance(groupCreater).AsSingle();
//             Container.Bind<ComponentVisiblyStorage>().FromInstance(componentVisiblyStorage).AsSingle();
//             Container.Bind<AddComponentWindowsController>().FromInstance(addComponentWindowsController).AsSingle();
//             Container.Bind<CustomInspectorController>().FromInstance(customInspectorController).AsSingle();
//             Container.Bind<InitializedComponentController>().FromInstance(initializedComponentController).AsSingle();
//             Container.Bind<VerticalBezierZoom>().FromInstance(verticalBezierZoom);
//             Container.Bind<BezierLineDrawer>().FromInstance(bezierLineDrawer);
//             Container.Bind<BezierController>().FromInstance(bezierController).AsSingle();
//             Container.Bind<SelectSpriteController>().FromInstance(selectSpriteController).AsSingle();
//             Container.Bind<SelectColorContoller>().FromInstance(selectColorContoller).AsSingle();
//             Container.Bind<CollidersPrefab>().FromInstance(collidersPrefab).AsSingle();
//             Container.Bind<SaveComposition>().FromInstance(saveComposition).AsSingle();
//             Container.Bind<KeyfeameVizualizer>().FromInstance(keyfeameVizualizer).AsSingle();
//             Container.Bind<BezierSelectPointsController>().FromInstance(bezierSelectPointsController);
//             Container.Bind<FinishLevelController>().FromInstance(finishLevelController).AsSingle();
//             Container.Bind<TimeLineMarkersController>().FromInstance(timeLineMarkersController);
//             Container.Bind<SaveLevel>().FromInstance(saveLevel).AsSingle();
//             Container.Bind<GridScene>().FromInstance(gridScene).AsSingle();
//             Container.Bind<CustomSpriteStorage>().FromInstance(customSpriteStorage).AsSingle();
//             Container.Bind<SpriteLoadController>().FromInstance(spriteLoadController).AsSingle();
//             Container.Bind<EditColliderState>().FromInstance(editColliderState).AsSingle();
//             Container.Bind<EdgeColliderSelectionCube>().FromInstance(edgeColliderSelectionCube).AsSingle();
//             Container.Bind<SelectComposition>().FromInstance(selectComposition).AsSingle();
//             Container.Bind<ShakeCamera>().FromInstance(shakeComponent).AsSingle();
//             Container.Bind<SceneToRawImageConverter>().FromInstance(sceneToRawImageConverter).AsSingle();
//             Container.Bind<CoordinateSystem>().FromInstance(coordinateSystem).AsSingle();
//             Container.Bind<SaveEditorSettings>().FromInstance(saveEditorSettings).AsSingle();
//             Container.Bind<SpriteOutlineBuffer>().FromInstance(spriteOutlineBuffer).AsSingle();
//             Container.Bind<TimeLineRecorder>().FromInstance(timeLineRecorder).AsSingle();
//             Container.Bind<KeyframeCreator>().FromInstance(keyframeCreator).AsSingle();
//             
//             
//             
//             Container.Bind<ActionMap>().FromInstance(new ActionMap()).AsSingle();
//             
//             
//             // Сначала создаем и привязываем EventBus
//             _gameEventBus = new GameEventBus();
//             Container.Bind<GameEventBus>().FromInstance(_gameEventBus).AsSingle();
//             
//             // Затем привязываем MainObjects и внедряем зависимости
//             Container.Bind<MainObjects>().FromInstance(mainObjects).AsSingle();
//             
//             Container.Bind<TimeLineSettings>().FromInstance(settings).AsSingle();
//             Container.Bind<Main>().FromInstance(main).AsSingle();
//         }
//     }
// }
//
// [Serializable]
// class CollidersPrefab
// {
//     public BoxCollider2DOutline BoxCollider2DPrefab;
//     public CircleCollider2DOutline CircleCollider2DPrefab;
//     public CapsuleCollider2DOutline CapsuleCollider2DPrefab;
//     public EdgeColliderEditor EdgeCollider2DPrefab;
// }