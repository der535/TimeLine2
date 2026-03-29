using System;
using EventBus;
using TimeLine.EventBus.Events.Misc;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.EditorWindows.SceneView.TransformTools.Position;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class ToolsController : MonoBehaviour
    {
        [SerializeField] private GameObject positionTool;
        [SerializeField] private GameObject rotateTool;
        [SerializeField] private GameObject scaleTool;
        [Space] [SerializeField] private Button positionButton;
        [SerializeField] private Button rotateButton;
        [SerializeField] private Button scaleButton;
        [Space] [SerializeField] private PositionController positionController;
        [SerializeField] private RotationController rotationController;
        [SerializeField] private ScaleController scaleController;
        [SerializeField] private SelectLock selectLock;

        public ActiveTool _activeTool;
        private GameEventBus _gameEventBus;
        private SelectObjectController _selectObjectController;

        public Action OnValueChanged;
        public Action OnStopPositionX;
        public Action OnStopPositionY;
        public Action OnStopRotation;
        public Action OnStopScaleX;
        public Action OnStopScaleY;

        public enum ActiveTool
        {
            Position,
            Rotate,
            Scale
        }

        [Inject]
        private void Construct(GameEventBus eventBus, SelectObjectController selectObjectController)
        {
            _gameEventBus = eventBus;
            _selectObjectController = selectObjectController;
        }

        public void ChangeTool(ActiveTool tool)
        {
            _activeTool = tool;

            if (_selectObjectController.SelectObjects.Count > 0)
                SetActiveTool();
        }

        private void Start()
        {
            positionController.OnValueChanged += () => OnValueChanged?.Invoke();
            rotationController.OnValueChanged +=  () => OnValueChanged?.Invoke();
            scaleController.OnValueChanged +=  () => OnValueChanged?.Invoke();
            
            positionController.OnStopPositionX += () => OnStopPositionX?.Invoke();
            positionController.OnStopPositionY += () => OnStopPositionY?.Invoke();
            rotationController.OnStopRotation += () => OnStopRotation?.Invoke();
            scaleController.OnStopScaleX += () => OnStopScaleX?.Invoke();
            scaleController.OnStopScaleY += () => OnStopScaleY?.Invoke();



            positionButton.onClick.AddListener(() => ChangeTool(ActiveTool.Position));
            rotateButton.onClick.AddListener(() => ChangeTool(ActiveTool.Rotate));
            scaleButton.onClick.AddListener(() => ChangeTool(ActiveTool.Scale));

            _activeTool = ActiveTool.Position;
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => { SetActiveTool(); });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                if (data.SelectedObjects.Count <= 0)
                {
                    DisableTool();
                }
            });
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent _) => { DisableTool(); });
            _gameEventBus.SubscribeTo((ref TurnEditColliderEvent data) =>
            {
                if (data.IsEditing)
                    DisableTool();
                else
                    SetActiveTool();
            });
        }

        private void SetActiveTool()
        {
            // Debug.Log(_activeTool);
            DisableTool();

            switch (_activeTool)
            {
                case ActiveTool.Position:
                    positionTool.SetActive(true);
                    positionController.EnableTool();
                    break;
                case ActiveTool.Rotate:
                    rotateTool.SetActive(true);
                    rotationController.EnableTool();
                    break;
                case ActiveTool.Scale:
                    scaleTool.SetActive(true);
                    scaleController.EnableTool();
                    break;
            }
        }

        private void DisableTool()
        {
            positionTool.SetActive(false);
            rotateTool.SetActive(false);
            scaleTool.SetActive(false);
            selectLock.IsLocked = false;
        }
    }
}