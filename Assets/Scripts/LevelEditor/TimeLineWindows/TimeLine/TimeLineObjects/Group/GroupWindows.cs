using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ActionHistory;
using TimeLine.LevelEditor.ActionHistory.Commands;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.Group
{
    public class GroupWindows : MonoBehaviour
    {
        [SerializeField] private RectTransform _groupWindow;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _createButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _cancelButton;

        private ActionMap _actionMap;
        private GroupCreater _groupCreater;
        private string _outputName;
        private SelectObjectController _selectObjectController;
        private SaveLevel _saveLevel;
        private TrackObjectRemover _trackObjectRemover;
        private FacadeObjectSpawner _facadeObjectSpawner;
        private TrackObjectStorage _trackObjectStorage;
        private SaveComposition _saveComposition;

        [Inject]
        private void Construct(ActionMap actionMap, SelectObjectController selectObjectController, SaveLevel saveLevel,
            TrackObjectRemover trackObjectRemover, FacadeObjectSpawner facadeObjectSpawner, TrackObjectStorage trackObjectStorage, SaveComposition saveComposition)
        {
            _actionMap = actionMap;
            _selectObjectController = selectObjectController;
            _saveLevel = saveLevel;
            _trackObjectRemover = trackObjectRemover;
            _facadeObjectSpawner = facadeObjectSpawner;
            _trackObjectStorage = trackObjectStorage;
            _saveComposition = saveComposition;
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => { ClosePanel(); });
            _cancelButton.onClick.AddListener(() => { ClosePanel(); });
            StringInputValidator stringInputValidator = new StringInputValidator(_inputField, s =>
            {
                _outputName = s;
                _createButton.interactable = !string.IsNullOrEmpty(s);
            });
            _createButton.onClick.AddListener(() =>
            {
                Create();
                _groupWindow.gameObject.SetActive(false);
            });
        }

        [Inject]
        private void Constructor(GroupCreater groupCreater)
        {
            _groupCreater = groupCreater;
        }

        public void ClosePanel()
        {
            _actionMap.Editor.Enable();
            _groupWindow.gameObject.SetActive(false);
        }

        public void Open()
        {
            _actionMap.Editor.Disable();
            _groupWindow.gameObject.SetActive(true);
            _outputName = string.Empty;
            _inputField.text = "";
            _createButton.interactable = false;
        }

        private void Create()
        {
            _actionMap.Editor.Enable();
            CommandHistory.AddCommand(new CreateCompositionCommand(
                _saveLevel,
                _saveComposition,
                _trackObjectRemover,
                _facadeObjectSpawner,
                _trackObjectStorage,
                _groupCreater,
                _outputName,
                _selectObjectController.SelectObjects,
                ""), true);
        }
    }
}