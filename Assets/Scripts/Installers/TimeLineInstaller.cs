using System;
using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Input;
using TimeLine.Keyframe;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.Installers
{
    public class TimeLineInstaller : MonoInstaller
    {
        [SerializeField] private TimeLineConverter timeLineConverter;
        [SerializeField] private Main main;
        [SerializeField] private BarBeatCounter barBeatCounter;
        [SerializeField] private TimeLineRenderer timeLineRenderer;
        [SerializeField] private TimeMarkerRenderer timeMarkerRenderer;
        [SerializeField] private CurrentTimeMarkerRenderer currentTimeMarkerRenderer;
        [SerializeField] private Scroll scroll;
        [SerializeField] private CursorBeatPosition cursorBeatPosition;
        [SerializeField] private TrackObjectSpawner trackObjectSpawner;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;
        [SerializeField] private SettingPanel settingPanel;
        [SerializeField] private BranchCollection branchCollection;
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private TrackStorage trackStorage;
        [SerializeField] private TreeViewUI treeViewUI;
        [SerializeField] private TimeLineSettings settings;
        [SerializeField] private TimeLineScroll timeLineScroll;
        [SerializeField] private SceneObjectAddKeyFrame sceneObjectAddKeyFrame;
        [FormerlySerializedAs("gridSystem")] [SerializeField] private GridUI gridUI;
        [SerializeField] private ParentMain parentMain;
        [SerializeField] private TimeLineKeyframeScroll timeLineKeyframeScroll;
        [SerializeField] private SelectObjectController selectObjectController;
        [SerializeField] private TrackObjectRemover trackObjectRemover;
        [SerializeField] private GroupEdit groupEdit;
        [SerializeField] private GroupCreater groupCreater;
        [SerializeField] private ComponentVisiblyStorage componentVisiblyStorage;
        [SerializeField] private AddComponentWindowsData componentWindowsData;
        [SerializeField] private CustomInspectorController customInspectorController;
        [SerializeField] private InitializedComponentController initializedComponentController;
        [SerializeField] private VerticalBezierPan verticalBezierPan;
        [SerializeField] private BezierLineDrawer bezierLineDrawer;
        [SerializeField] private BezierController bezierController;
        [SerializeField] private SelectSpriteController selectSpriteController;
        [SerializeField] private SelectColorContoller selectColorContoller;
        [SerializeField] private CollidersPrefab collidersPrefab;
        [SerializeField] private SaveComposition saveComposition;
        [SerializeField] private KeyfeameVizualizer keyfeameVizualizer;
        
        [SerializeField] private MainObjects mainObjects;
        
        private GameEventBus _gameEventBus;
        
        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            Container.Bind<TimeLineConverter>().FromInstance(timeLineConverter).AsSingle();
            Container.Bind<BarBeatCounter>().FromInstance(barBeatCounter).AsSingle();
            Container.Bind<TimeLineRenderer>().FromInstance(timeLineRenderer).AsSingle();
            Container.Bind<TimeMarkerRenderer>().FromInstance(timeMarkerRenderer).AsSingle();
            Container.Bind<CurrentTimeMarkerRenderer>().FromInstance(currentTimeMarkerRenderer).AsSingle();
            Container.Bind<Scroll>().FromInstance(scroll).AsSingle();
            Container.Bind<CursorBeatPosition>().FromInstance(cursorBeatPosition).AsSingle();
            Container.Bind<TrackObjectSpawner>().FromInstance(trackObjectSpawner).AsSingle();
            Container.Bind<KeyframeTrackStorage>().FromInstance(keyframeTrackStorage).AsSingle();
            Container.Bind<SettingPanel>().FromInstance(settingPanel).AsSingle();
            Container.Bind<BranchCollection>().FromInstance(branchCollection).AsSingle();
            Container.Bind<TrackObjectStorage>().FromInstance(trackObjectStorage).AsSingle();
            Container.Bind<TrackStorage>().FromInstance(trackStorage).AsSingle();
            Container.Bind<TreeViewUI>().FromInstance(treeViewUI).AsSingle();
            Container.Bind<TimeLineScroll>().FromInstance(timeLineScroll).AsSingle();
            Container.Bind<SceneObjectAddKeyFrame>().FromInstance(sceneObjectAddKeyFrame).AsSingle();
            Container.Bind<GridUI>().FromInstance(gridUI).AsSingle();
            Container.Bind<ParentMain>().FromInstance(parentMain).AsSingle();
            Container.Bind<TimeLineKeyframeScroll>().FromInstance(timeLineKeyframeScroll);
            Container.Bind<SelectObjectController>().FromInstance(selectObjectController).AsSingle();
            Container.Bind<TrackObjectRemover>().FromInstance(trackObjectRemover);
            Container.Bind<GroupEdit>().FromInstance(groupEdit).AsSingle();
            Container.Bind<GroupCreater>().FromInstance(groupCreater).AsSingle();
            Container.Bind<ComponentVisiblyStorage>().FromInstance(componentVisiblyStorage).AsSingle();
            Container.Bind<AddComponentWindowsData>().FromInstance(componentWindowsData).AsSingle();
            Container.Bind<CustomInspectorController>().FromInstance(customInspectorController).AsSingle();
            Container.Bind<InitializedComponentController>().FromInstance(initializedComponentController).AsSingle();
            Container.Bind<VerticalBezierPan>().FromInstance(verticalBezierPan);
            Container.Bind<BezierLineDrawer>().FromInstance(bezierLineDrawer);
            Container.Bind<BezierController>().FromInstance(bezierController).AsSingle();
            Container.Bind<SelectSpriteController>().FromInstance(selectSpriteController).AsSingle();
            Container.Bind<SelectColorContoller>().FromInstance(selectColorContoller).AsSingle();
            Container.Bind<CollidersPrefab>().FromInstance(collidersPrefab).AsSingle();
            Container.Bind<SaveComposition>().FromInstance(saveComposition).AsSingle();
            Container.Bind<KeyfeameVizualizer>().FromInstance(keyfeameVizualizer).AsSingle();
            
            
            
            Container.Bind<ActionMap>().FromInstance(new ActionMap()).AsSingle();
            
            
            // Сначала создаем и привязываем EventBus
            _gameEventBus = new GameEventBus();
            Container.Bind<GameEventBus>().FromInstance(_gameEventBus).AsSingle();
            
            // Затем привязываем MainObjects и внедряем зависимости
            Container.Bind<MainObjects>().FromInstance(mainObjects).AsSingle();
            mainObjects.Init(_gameEventBus);
            
            Container.Bind<TimeLineSettings>().FromInstance(settings).AsSingle();
            Container.Bind<Main>().FromInstance(main).AsSingle();
        }
    }
}

[Serializable]
class CollidersPrefab
{
    public BoxCollider2DOutline BoxCollider2DPrefab;
}