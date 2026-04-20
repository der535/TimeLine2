using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.EditorWindows.SceneView.TransformTools;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.InspectorTab.Components.BoxCollider;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TransformationSquare;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.LevelEditor.InspectorTab.Logic
{
    public class CustomInspectorController : MonoBehaviour
    {
        [SerializeField] private CustomInspectorDrawer inspectorDrawer;
        [SerializeField] private RectTransform rootObject;

        [FormerlySerializedAs("keyframeCreater")] [SerializeField]
        private KeyframeCreator keyframeCreator;

        [Space] [SerializeField] private ComponentUI componentUIPrefab;

        private List<IComponentDrawer> _componentDrawers = new();
        private List<IComponentDrawer> _structurDrawers = new();

        private GameEventBus _gameEventBus;

        private Entity _selectedObject;
        private TrackObjectStorage _selectedTransform;
        private ColliderDrawer _colliderDrawer;
        private ToolsController _toolsController;
        private TimeLineRecorder _timeLineRecorder;
        private CustomSpriteStorage _customInspectorDrawer;
        private TransformationSquareController _transformationSquareController;

        [Inject]
        private void Construct(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage,
            ColliderDrawer colliderDrawer, ToolsController toolsController, TimeLineRecorder timeLineRecorder, CustomSpriteStorage customSpriteStorage, TransformationSquareController transformationSquareController)
        {
            _gameEventBus = gameEventBus;
            _selectedTransform = trackObjectStorage;
            _colliderDrawer = colliderDrawer;
            _toolsController = toolsController;
            _timeLineRecorder = timeLineRecorder;
            _customInspectorDrawer = customSpriteStorage;
            _transformationSquareController = transformationSquareController;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                if(data.UpdateVisual)
                    Draw(data.Tracks[^1].entity);
            });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => Draw(data.SelectedObjects[^1].entity));
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => Clear());
            _gameEventBus.SubscribeTo((ref AddComponentEvent data) => { StartCoroutine(Redraw()); }, -1);
            _gameEventBus.SubscribeTo((ref RemoveComponentEvent data) => { StartCoroutine(Redraw()); }, -1);

            _componentDrawers.Add(new TransformComponentDrawer(_transformationSquareController));
            _componentDrawers.Add(new NameDrawer());
            _componentDrawers.Add(new SpriteRendererDrawer(_customInspectorDrawer));
            _componentDrawers.Add(new SunBurstMaterialDrawer());
            _componentDrawers.Add(new BoxCollider2DDrawer(_colliderDrawer));
            _componentDrawers.Add(new CircleCollider2DDrawer(_colliderDrawer));
            _componentDrawers.Add(new CapsuleCollider2DDrawer());
            _componentDrawers.Add(new EdgeCollider2DDrawer());
            _componentDrawers.Add(new ShakeDrawer());
            _componentDrawers.Add(new PolygonCollider2DDrawer(_colliderDrawer));
            _componentDrawers.Add(new RadialSunburstDrawer());
            _componentDrawers.Add(new ShakeCameraDrawer());
        }

        internal IEnumerator Redraw()
        {
            yield return new WaitForEndOfFrame();
            if (_selectedObject != null)
                Draw(_selectedObject);
        }

        private void Draw(Entity target)
        {
            _selectedObject = target;

            Clear();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //Получаем все компоненты
            using NativeArray<ComponentType> types = entityManager.GetComponentTypes(target);

            foreach (var drawer in _componentDrawers)
            {
                var checkResult = drawer.GetComponent(types.ToList());
                if (checkResult)
                {
                    drawer.Setup(inspectorDrawer, _selectedTransform, keyframeCreator, _toolsController,
                        _timeLineRecorder);
                    drawer.Draw(target);
                }
            }

            inspectorDrawer.CreateAddComponentButton(target);
        }

        private void Clear()
        {
            foreach (Transform child in rootObject)
                Destroy(child.gameObject);
        }
    }
}