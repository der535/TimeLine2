using System;
using EventBus;
using TimeLine.Components;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Input;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.Controllers;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.LevelEditor.Core.MusicLoader;
using TimeLine.LevelEditor.Core.MusicOffset;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.CustomInspector;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.GeneralEditor;
using TimeLine.LevelEditor.outline;
using TimeLine.LevelEditor.Select_composition;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using TimeLine.Parent;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

// Убедитесь, что все необходимые пространства имен подключены.
// Я сократил список using для краткости, так как IDE добавит их сама.

namespace TimeLine.Installers
{
    public class TimeLineInstaller2 : MonoInstaller
    {
        // --- Groups for Inspector Organization ---

        [SerializeField] private CoreReferences core;
        [SerializeField] private UIReferences ui;
        [SerializeField] private RenderReferences render;
        [SerializeField] private DataReferences data;
        [SerializeField] private ToolReferences tools;
        [SerializeField] private ConfigReferences config;
        [SerializeField] private SpawnReferences spawnReferences;

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

            Container.Bind<ObjectFactory>()
                .AsSingle()
                .WithArguments(spawnReferences.rootSceneObject, spawnReferences.sceneObjectPrefab,
                    spawnReferences.trackObjectPrefab);

            Container.Bind<ObjectLoader>()
                .AsSingle();

            Container.Bind<TrackObjectClipboard>()
                .AsSingle();
        }

        private void InstallCore()
        {
            Container.Bind<M_MusicData>().AsSingle();
            Container.Bind<M_MusicLoaderService>().AsSingle();
            Container.Bind<M_MusicOffsetData>().AsSingle();
            Container.Bind<M_AudioPlaybackService>().AsSingle().WithArguments(core.timeLineAudioSource);
            Container.Bind<M_PlaybackState>().AsSingle();
            
            Container.Bind<M_KeyframeSelectedStorage>().AsSingle();
            
            Container.BindInstance(core.musicDataController).AsSingle();
            Container.BindInstance(core.musicLoaderController).AsSingle();
            Container.BindInstance(core.musicOffsetController).AsSingle();
            Container.BindInstance(core.SpeedController).AsSingle();
            Container.BindInstance(core.playAndStopButton).AsSingle();

            Container.BindInstance(core.Main).AsSingle();
            Container.BindInstance(core.ParentMain).AsSingle();
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
            Container.BindInstance(ui.TreeViewUI).AsSingle();
            Container.BindInstance(ui.SettingPanel).AsSingle();
            Container.BindInstance(ui.GridUI).AsSingle();
            Container.BindInstance(ui.TimeLineScroll).AsSingle();
            Container.BindInstance(ui.TimeLineKeyframeScroll).AsSingle();
            Container.BindInstance(ui.AddComponentWindowsController).AsSingle();
            Container.BindInstance(ui.CustomInspectorController).AsSingle();
        }

        private void InstallRendering()
        {
            Container.BindInstance(render.TimeLineRenderer).AsSingle();
            Container.BindInstance(render.timeLineMarkerRenderer).AsSingle();
            Container.BindInstance(render.CurrentTimeMarkerRenderer).AsSingle();
            Container.BindInstance(render.CursorBeatPosition).AsSingle();
            Container.BindInstance(render.KeyfeameVizualizer).AsSingle();
            Container.BindInstance(render.TimeLineMarkersController).AsSingle();
            Container.BindInstance(render.SpriteOutlineBuffer).AsSingle();
            Container.BindInstance(render.ShakeComponent).AsSingle();
            Container.BindInstance(render.VerticalBezierScroll).AsSingle();
        }

        private void InstallData()
        {
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
        }

        private void InstallTools()
        {
            Container.BindInstance(tools.SelectObjectController).AsSingle();
            Container.BindInstance(tools.DeselectObject).AsSingle();
            Container.BindInstance(tools.TrackObjectRemover).AsSingle();
            Container.BindInstance(tools.GroupEdit).AsSingle();
            Container.BindInstance(tools.GroupCreater).AsSingle();
            Container.BindInstance(tools.InitializedComponentController).AsSingle();
            Container.BindInstance(tools.VerticalBezierZoom).AsSingle();
            Container.BindInstance(tools.BezierLineDrawer).AsSingle();
            Container.BindInstance(tools.BezierController).AsSingle();
            Container.BindInstance(tools.SelectSpriteController).AsSingle();
            Container.BindInstance(tools.SelectColorContoller).AsSingle();
            Container.BindInstance(tools.BezierSelectPointsController).AsSingle();
            Container.BindInstance(tools.SpriteLoadController).AsSingle();
            Container.BindInstance(tools.EditColliderState).AsSingle();
            Container.BindInstance(tools.EdgeColliderSelectionCube).AsSingle();
            Container.BindInstance(tools.SelectComposition).AsSingle();
            Container.BindInstance(tools.TimeLineRecorder).AsSingle();
            Container.BindInstance(tools.KeyframeCreator).AsSingle();
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

        public Main Main;
        public ParentMain ParentMain;
        public TimeLineSettings Settings;
        public TimeLineConverter TimeLineConverter;
        public BarBeatCounter BarBeatCounter;
        public TimeLineSpeedController SpeedController;

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
        public TreeViewUI TreeViewUI;
        public SettingPanel SettingPanel;
        [FormerlySerializedAs("gridSystem")] public GridUI GridUI;
        public TimeLineScroll TimeLineScroll;
        public TimeLineKeyframeScroll TimeLineKeyframeScroll;
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
        public KeyfeameVizualizer KeyfeameVizualizer;
        public TimeLineMarkersController TimeLineMarkersController;
        public SpriteOutlineBuffer SpriteOutlineBuffer;
        public ShakeCamera ShakeComponent;
        public VerticalBezierScroll VerticalBezierScroll;
    }

    [Serializable]
    public class DataReferences
    {
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
        public SelectObjectController SelectObjectController;
        public DeselectObject DeselectObject;
        public TrackObjectRemover TrackObjectRemover;
        public GroupEdit GroupEdit;
        public GroupCreater GroupCreater;
        public InitializedComponentController InitializedComponentController;

        [FormerlySerializedAs("verticalBezierPan")]
        public VerticalBezierZoom VerticalBezierZoom;

        public BezierLineDrawer BezierLineDrawer;
        public BezierController BezierController;
        public SelectSpriteController SelectSpriteController;
        public SelectColorContoller SelectColorContoller;
        public BezierSelectPointsController BezierSelectPointsController;
        public SpriteLoadController SpriteLoadController;
        public EditColliderState EditColliderState;
        public EdgeColliderSelectionCube EdgeColliderSelectionCube;
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
        public CircleCollider2DOutline CircleCollider2DPrefab;
        public CapsuleCollider2DOutline CapsuleCollider2DPrefab;
        public EdgeColliderEditor EdgeCollider2DPrefab;
    }

    [Serializable]
    public class SpawnReferences
    {
        public Transform rootSceneObject;
        public GameObject sceneObjectPrefab;
        public GameObject trackObjectPrefab;
    }
}