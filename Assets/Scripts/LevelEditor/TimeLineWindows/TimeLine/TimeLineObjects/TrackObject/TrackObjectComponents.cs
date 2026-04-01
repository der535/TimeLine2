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


        public TrackObject TrackObject;
        public TrackObjectData Data;
        public TrackObjectGlobalTime GlobalTime;
        public TrackObjectState State;

        private DiContainer _container;
        private TickableManager _tickableManager;

        private bool _isSetuped;

        [Inject]
        private void Construct(DiContainer container, TickableManager tickableManager)
        {
            _container = container;
            _tickableManager = tickableManager;
        }

        public void Setup(TrackObjectData data, ITrackObjectView view, TrackObjectSelect select,
            TrackObjectVisual visual, TrackObjectCustomizationController customizationController)
        {
            if(view != null)
                View = view;
            Select = select;
            Visual = visual;
            CustomizationController = customizationController;
            
            if (!_isSetuped)
            {
                _isSetuped = true;
            }
            else
            {
                Debug.Log("Already setup");
                return;
            }

            Data = data;
            State = new TrackObjectState();
            TrackObject = new TrackObject();
            _container.Inject(TrackObject);
            _container.BindInterfacesAndSelfTo<TrackObject>().FromInstance(TrackObject).NonLazy();
            // ВРУЧНУЮ добавляем в менеджер обновлений, если объект реализует ITickable
            if (TrackObject is ITickable tickable && _tickableManager != null)
            {
                _tickableManager.Add(tickable);
            }

            TrackObject.Setup(data, view, State);
            GlobalTime = new TrackObjectGlobalTime(this, Data);
            Select.Setup(TrackObject, State, Data, view);
        }

        public void Setup(TrackObjectData data)
        {
            if (!_isSetuped)
            {
                _isSetuped = true;
            }
            else
            {
                return;
            }

            Data = data;
            State = new TrackObjectState();
            TrackObject = new TrackObject();
            _container.Inject(TrackObject);
            _container.BindInterfacesAndSelfTo<TrackObject>().FromInstance(TrackObject).NonLazy();
            // ВРУЧНУЮ добавляем в менеджер обновлений, если объект реализует ITickable
            if (TrackObject is ITickable tickable && _tickableManager != null)
            {
                _tickableManager.Add(tickable);
            }

            TrackObject.Setup(data,  new NullTrackObjectView(), State);
            GlobalTime = new TrackObjectGlobalTime(this, Data);
        }

        public void Dispose()
        {
            Data = null;
            TrackObject.Dispose();
        }
    }
}