using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.Services;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject
{
    public class TrackObjectComponents
    {
        public TrackObjectCustomizationController CustomizationController;
        public TrackObjectVisual Visual;
        public TrackObjectSelect Select;
        public ITrackObjectView View { get; private set; } = new NullTrackObjectView();
        // private TrackObjectView _trackObjectView;


        public TrackObject trackObject;
        public TrackObjectData Data;
        public TrackObjectGlobalTime GlobalTime;
        public TrackObjectState State;

        private DiContainer _container;
        private TickableManager _tickableManager;

        private bool isSetuped = false;

        [Inject]
        private void Construct(DiContainer container, TickableManager tickableManager)
        {
            _container = container;
            _tickableManager = tickableManager;
        }

        public void Setup(TrackObjectData data, ITrackObjectView view, TrackObjectSelect select,
            TrackObjectVisual visual, TrackObjectCustomizationController customizationController)
        {
            View = view;
            Select = select;
            Visual = visual;
            CustomizationController = customizationController;
            
            if (!isSetuped)
            {
                isSetuped = true;
            }
            else
            {
                Debug.Log("Already setup");
                return;
            }

            Data = data;
            State = new TrackObjectState();
            trackObject = new TrackObject();
            _container.Inject(trackObject);
            _container.BindInterfacesAndSelfTo<TrackObject>().FromInstance(trackObject).NonLazy();
            // ВРУЧНУЮ добавляем в менеджер обновлений, если объект реализует ITickable
            if (trackObject is ITickable tickable && _tickableManager != null)
            {
                _tickableManager.Add(tickable);
            }

            trackObject.Setup(data, view, State);
            GlobalTime = new TrackObjectGlobalTime(this, Data);
            Select.Setup(trackObject, State, Data, view);
        }

        public void Setup(TrackObjectData data)
        {
            if (!isSetuped)
            {
                isSetuped = true;
            }
            else
            {
                Debug.Log("Already setup");
                return;
            }

            Data = data;
            State = new TrackObjectState();
            trackObject = new TrackObject();
            _container.Inject(trackObject);
            _container.BindInterfacesAndSelfTo<TrackObject>().FromInstance(trackObject).NonLazy();
            // ВРУЧНУЮ добавляем в менеджер обновлений, если объект реализует ITickable
            if (trackObject is ITickable tickable && _tickableManager != null)
            {
                _tickableManager.Add(tickable);
            }

            trackObject.Setup(data, null, State);
            GlobalTime = new TrackObjectGlobalTime(this, Data);
        }

        public void OnDestroy()
        {
            Data = null;
            trackObject.Dispose();
        }
    }
}