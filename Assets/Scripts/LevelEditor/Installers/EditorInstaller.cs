using System;
using EventBus;
using TimeLine.Components;
using TimeLine.EdgeColliderEditor;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Input;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.ContextMenu;
using TimeLine.LevelEditor.Controllers;
using TimeLine.LevelEditor.CopyComponent;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.LevelEditor.Core.MusicLoader;
using TimeLine.LevelEditor.Core.MusicOffset;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Controller;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Data;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Service;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.View;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeType;
using TimeLine.LevelEditor.EditorWindows.SceneView.Outline;
using TimeLine.LevelEditor.EditorWindows.SceneView.TransformTools;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.GeneralEditor;
using TimeLine.LevelEditor.InspectorTab.Components.EdgeCollider;
using TimeLine.LevelEditor.InspectorTab.Logic;
using TimeLine.LevelEditor.LevelEffects;
using TimeLine.LevelEditor.LoadingScreen.Controllers;
using TimeLine.LevelEditor.MaxObjectIndex.Controller;
using TimeLine.LevelEditor.outline;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.Select_composition;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using TimeLine.LevelEditor.TrackObjectSize.Data;
using TimeLine.LevelEditor.UIAnimation;
using TimeLine.Parent;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.LevelEditor.Core
{
    public class EditorInstaller : MonoInstaller
    {
        // --- Groups for Inspector Organization ---

        [SerializeField] private CoreReferences core;
        [SerializeField] private UIReferences ui;
        [SerializeField] private RenderReferences render;
        [SerializeField] private DataReferences data;
        [SerializeField] private ToolReferences tools;
        [SerializeField] private ConfigReferences config;
        [SerializeField] private SpawnReferences spawnReferences;
        [SerializeField] private CameraReferences cameraReferences;
        [SerializeField] private KeyframeReferences keyframeReferences;

        // Отдельные объекты, которые не вошли в группы
        [Space] [SerializeField] private MainObjects mainObjects;
        [SerializeField] private CollidersPrefab collidersPrefab;

        public override void InstallBindings()
        {
            // Биндинг самого EventBus (чистый C# класс)
            Container.Bind<GameEventBus>().AsSingle();

            // Биндинг карты действий
            Container.Bind<ActionMap>().AsSingle();

            // Биндинг конфигураций
            Container.BindInstance(collidersPrefab).AsSingle();
            Container.BindInstance(mainObjects).AsSingle();

            // Вызов под-методов для чистоты кода
            InstallCore();
            InstallUI();
            InstallRendering();
            InstallData();
            InstallTools();
            InstallCameras();

            Container.Bind<ObjectFactory>()
                .AsSingle()
                .WithArguments(spawnReferences.rootSceneObject, spawnReferences.sceneObjectPrefab,
                    spawnReferences.trackObjectPrefab);

            Container.Bind<ObjectLoader>()
                .AsSingle();

            Container.Bind<TrackObjectClipboard>()
                .AsSingle();
            
            Container.BindInstance(keyframeReferences).AsSingle();
            Container.Bind<BezierVerticalPosition>().AsSingle();
            Container.Bind<BezierCursorValue>().AsSingle().NonLazy();;
            Container.Bind<BezierVerticalPositionController>().AsSingle().NonLazy();;
            Container.Bind<SetPositionInTimeline>().AsSingle().NonLazy();;

        }

        private void InstallCore()
        {
            Container.Bind<M_MusicData>().AsSingle();
            Container.Bind<M_MusicLoaderService>().AsSingle();
            Container.Bind<M_MusicOffsetData>().AsSingle();
            Container.Bind<M_AudioPlaybackService>().AsSingle().WithArguments(core.timeLineAudioSource);
            Container.Bind<M_PlaybackState>().AsSingle();

            Container.Bind<M_KeyframeActiveTypeData>().AsSingle();
            Container.Bind<KeyframeSelectedStorage>().AsSingle();
            Container.Bind<CopyComponentController>().AsSingle();
            
            Container.BindInstance(core.musicDataController).AsSingle();
            Container.BindInstance(core.musicLoaderController).AsSingle();
            Container.BindInstance(core.musicOffsetController).AsSingle();
            Container.BindInstance(core.SpeedController).AsSingle();
            Container.BindInstance(core.PlayModeController).AsSingle();
            Container.BindInstance(core.playAndStopButton).AsSingle();

            Container.BindInstance(core.contextMenuController).AsSingle();
            
            Container.BindInstance(core.Main).AsSingle();
            // Container.BindInstance(core.ParentMain).AsSingle();
            Container.BindInstance(core.Settings).AsSingle();
            Container.BindInstance(core.TimeLineConverter).AsSingle();
            Container.BindInstance(core.BarBeatCounter).AsSingle();
            Container.BindInstance(core.facadeObjectSpawner).AsSingle();
            Container.BindInstance(core.GridScene).AsSingle();
            Container.BindInstance(core.SceneToRawImageConverter).AsSingle();
            Container.BindInstance(core.CoordinateSystem).AsSingle();
            Container.BindInstance(core.FinishLevelController).AsSingle();
            Container.BindInstance(core.SaveLevel).AsSingle();
            Container.BindInstance(core.EditingCompositionController).AsSingle();
            Container.BindInstance(core.ParentLinkRestorer).AsSingle();
        }

        private void InstallUI()
        {
            Container.BindInstance(ui.ScrollTimeLineKeyframe).AsSingle();
            Container.BindInstance(ui.TabStorage).AsSingle();
            Container.BindInstance(ui.SaveButtonAnimation).AsSingle();
            Container.BindInstance(ui.LoadingScreenController).AsSingle();
            Container.BindInstance(ui.TreeViewUI).AsSingle();
            Container.BindInstance(ui.SettingPanel).AsSingle();
            Container.BindInstance(ui.GridUI).AsSingle();
            Container.BindInstance(ui.TimeLineScroll).AsSingle();
            Container.BindInstance(ui.timeLineKeyframeZoom).AsSingle();
            Container.BindInstance(ui.AddComponentWindowsController).AsSingle();
            Container.BindInstance(ui.CustomInspectorController).AsSingle();
        }

        private void InstallRendering()
        {
            Container.BindInstance(render.TimeLineRenderer).AsSingle();
            Container.BindInstance(render.timeLineMarkerRenderer).AsSingle();
            Container.BindInstance(render.CurrentTimeMarkerRenderer).AsSingle();
            Container.BindInstance(render.CursorBeatPosition).AsSingle();
            Container.BindInstance(render.keyframeVizualizer).AsSingle();
            Container.BindInstance(render.TimeLineMarkersController).AsSingle();
            Container.BindInstance(render.SpriteOutlineBuffer).AsSingle();
            Container.BindInstance(render.VerticalBezierScroll).AsSingle();
        }

        private void InstallData()
        {
            Container.BindInstance(data.ThemeStorage).AsSingle();
            Container.BindInstance(data.KeyframeTrackStorage).AsSingle();
            Container.BindInstance(data.KeyframeSelectController).AsSingle();
            Container.BindInstance(data.BranchCollection).AsSingle();
            Container.BindInstance(data.TrackObjectStorage).AsSingle();
            Container.BindInstance(data.TrackStorage).AsSingle();
            Container.BindInstance(data.ComponentVisiblyStorage).AsSingle();
            Container.BindInstance(data.CustomSpriteStorage).AsSingle();
            Container.BindInstance(data.BaseSpriteStorage).AsSingle();
            Container.BindInstance(data.GetSpriteName).AsSingle();
            Container.BindInstance(data.SaveComposition).AsSingle();
            Container.BindInstance(data.SaveEditorSettings).AsSingle();

            var trackObjectSizeData = new TrackObjectSizeData();
            Container.Bind<ITrackObjectSizeReader>().FromInstance(trackObjectSizeData).AsSingle();
            Container.Bind<TrackObjectSizeData>().FromInstance(trackObjectSizeData).AsSingle();
            Container.Bind<MaxObjectIndexController>().AsSingle();
            Container.Bind(typeof(ActiveBezierPointsData), typeof(IReadActiveBezierPointsData))
                .To<ActiveBezierPointsData>()
                .AsSingle();
            Container.Bind(typeof(MaxObjectIndexData), typeof(IMaxObjectIndexDataReading))
                .To<MaxObjectIndexData>()
                .AsSingle();
        }

        private void InstallTools()
        {
            Container.BindInstance(tools.PolygonColliderEditor).AsSingle();
            Container.BindInstance(tools.AddAnEntitySprite).AsSingle();
            Container.BindInstance(tools.KeyframeRemover).AsSingle();
            Container.BindInstance(tools.OutlineController).AsSingle();
            Container.BindInstance(tools.EdgeColliderEditor).AsSingle();
            Container.BindInstance(tools.SelectObjectController).AsSingle();
            Container.BindInstance(tools.DeselectObject).AsSingle();
            Container.BindInstance(tools.TrackObjectRemover).AsSingle();
            Container.BindInstance(tools.GroupEdit).AsSingle();
            Container.BindInstance(tools.GroupCreater).AsSingle();
            Container.BindInstance(tools.InitializedComponentController).AsSingle();
            Container.BindInstance(tools.toolsController).AsSingle();
            Container.BindInstance(tools.VerticalBezierZoom).AsSingle();
            Container.BindInstance(tools.BezierLineDrawer).AsSingle();
            Container.BindInstance(tools.BezierController).AsSingle();
            Container.BindInstance(tools.SelectSpriteController).AsSingle();
            Container.BindInstance(tools.SelectColorContoller).AsSingle();
            Container.BindInstance(tools.BezierSelectPointsController).AsSingle();
            Container.BindInstance(tools.SpriteLoadController).AsSingle();
            Container.BindInstance(tools.cEditColliderState).AsSingle();
            Container.BindInstance(tools.cEdgeColliderSelectionCube).AsSingle();
            Container.BindInstance(tools.SelectComposition).AsSingle();
            Container.BindInstance(tools.TimeLineRecorder).AsSingle();
            Container.BindInstance(tools.KeyframeCreator).AsSingle();
        }

        private void InstallCameras()
        {
            Container.BindInstance(cameraReferences).AsSingle();
        }
    }

    // --- Serializable Group Classes ---
    // Это позволит видеть в инспекторе аккуратные выпадающие списки, вместо 60 полей подряд.


    [Serializable]
    public class CoreReferences
    {
        public AudioSource timeLineAudioSource;

        public C_MusicDataController musicDataController;
        public C_MusicLoaderController musicLoaderController;
        public C_MusicOffsetController musicOffsetController;

        public PlayAndStopButton playAndStopButton;

        public ContextMenuController contextMenuController;

        public Main Main;
        // public ParentMain ParentMain;
        public TimeLineSettings Settings;
        public TimeLineConverter TimeLineConverter;
        public BarBeatCounter BarBeatCounter;
        public TimeLineSpeedController SpeedController;
        public PlayModeController PlayModeController;

        [FormerlySerializedAs("objectSpawner")] [FormerlySerializedAs("TrackObjectSpawner")]
        public FacadeObjectSpawner facadeObjectSpawner;

        public GridScene GridScene;
        public SceneToRawImageConverter SceneToRawImageConverter;
        public CoordinateSystem CoordinateSystem;
        public FinishLevelController FinishLevelController;
        public SaveLevel SaveLevel;
        public EditingCompositionController EditingCompositionController;
        public ParentLinkRestorer ParentLinkRestorer;
    }

    [Serializable]
    public class UIReferences
    {
        public ScrollTimeLineKeyframe ScrollTimeLineKeyframe;
        public TabStorage TabStorage;
        public SaveButtonAnimation SaveButtonAnimation;
        public LoadingScreenController LoadingScreenController;
        public TreeViewUI TreeViewUI;
        public SettingPanel SettingPanel;
        [FormerlySerializedAs("gridSystem")] public GridUI GridUI;
        public TimeLineScroll TimeLineScroll;
        [FormerlySerializedAs("TimeLineKeyframeScroll")] public TimeLineKeyframeZoom timeLineKeyframeZoom;
        public AddComponentWindowsController AddComponentWindowsController;
        public CustomInspectorController CustomInspectorController;
    }

    [Serializable]
    public class RenderReferences
    {
        public TimeLineRenderer TimeLineRenderer;

        [FormerlySerializedAs("TimeMarkerRenderer")]
        public TimeLineMarkerRenderer timeLineMarkerRenderer;

        public CurrentTimeMarkerRenderer CurrentTimeMarkerRenderer;
        public CursorBeatPosition CursorBeatPosition;
        [FormerlySerializedAs("KeyfeameVizualizer")] public KeyframeVizualizer keyframeVizualizer;
        public TimeLineMarkersController TimeLineMarkersController;
        public SpriteOutlineBuffer SpriteOutlineBuffer;
        public VerticalBezierScroll VerticalBezierScroll;
    }

    [Serializable]
    public class DataReferences
    {
        public ThemeStorage ThemeStorage;
        public KeyframeSelectController KeyframeSelectController;
        public KeyframeTrackStorage KeyframeTrackStorage;
        public BranchCollection BranchCollection;
        public TrackObjectStorage TrackObjectStorage;
        public TrackStorage TrackStorage;
        public ComponentVisiblyStorage ComponentVisiblyStorage;

        [FormerlySerializedAs("spriteStorage")]
        public CustomSpriteStorage CustomSpriteStorage;

        public BaseSpriteStorage BaseSpriteStorage;
        public GetSpriteName GetSpriteName;
        public SaveComposition SaveComposition;
        public SaveEditorSettings SaveEditorSettings;
    }

    [Serializable]
    public class ToolReferences
    {
        public AddAnEntitySprite   AddAnEntitySprite;
        public KeyframeRemover   KeyframeRemover;
        public OutlineController OutlineController;
        public EdgeColliderEditorHost EdgeColliderEditor;
        public PolygonColliderEditorHost PolygonColliderEditor;
        public SelectObjectController SelectObjectController;
        public DeselectObject DeselectObject;
        public TrackObjectRemover TrackObjectRemover;
        public GroupEdit GroupEdit;
        public GroupCreater GroupCreater;
        public InitializedComponentController InitializedComponentController;
        public ToolsController toolsController;

        [FormerlySerializedAs("verticalBezierPan")]
        public VerticalBezierZoom VerticalBezierZoom;

        public BezierLineDrawer BezierLineDrawer;
        public BezierController BezierController;
        public SelectSpriteController SelectSpriteController;
        public SelectColorContoller SelectColorContoller;
        public BezierSelectPointsController BezierSelectPointsController;
        public SpriteLoadController SpriteLoadController;

        [FormerlySerializedAs("EditColliderState")]
        public C_EditColliderState cEditColliderState;

        [FormerlySerializedAs("EdgeColliderSelectionCube")]
        public C_EdgeColliderSelectionCube cEdgeColliderSelectionCube;

        public SelectComposition SelectComposition;
        public TimeLineRecorder TimeLineRecorder;
        public KeyframeCreator KeyframeCreator;
    }

    // Пустой класс-заглушка на случай будущих расширений
    [Serializable]
    public class ConfigReferences
    {
    }

    [Serializable]
    public class CollidersPrefab
    {
        public BoxCollider2DOutline BoxCollider2DPrefab;
        // public CircleCollider2DOutline CircleCollider2DPrefab;
        public CapsuleCollider2DOutline CapsuleCollider2DPrefab;
        public EdgeCollider2D edgeCollider2DPrefab;
        public PolygonCollider2D polygonCollider2DPrefab;
    }


    [Serializable]
    public class CameraReferences
    {
        public Camera playCamera;
        public Camera editUICamera;
        public Camera editSceneCamera;
    }

    [Serializable]
    public class SpawnReferences
    {
        public Transform rootSceneObject;
        public GameObject sceneObjectPrefab;
        public GameObject trackObjectPrefab;
    }

    [Serializable]
    public class KeyframeReferences
    {
        /// <summary>
        /// Родительский объект внутри которого хранятся точки кривой
        /// </summary>
        public RectTransform rootPoints;
        /// <summary>
        /// Объект content в котором находится основная часть окна ключевых кадров
        /// </summary>
        public RectTransform generalPart;
        /// <summary>
        /// Родительский объект внутри которого маркеры времени (горизонтальные), текущее время, и треки к
        /// </summary>
        public RectTransform rootObjects;
        public RectTransform treePanelAnimations;
    }
}