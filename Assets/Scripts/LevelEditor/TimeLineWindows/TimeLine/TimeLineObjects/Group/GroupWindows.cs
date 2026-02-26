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

        private GroupCreater _groupCreater;
        private string _outputName;

        private void Awake()
        {
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

        public void Open()
        {
            _groupWindow.gameObject.SetActive(true);
            _outputName = string.Empty;
            _inputField.text = "";
            _createButton.interactable = false;
        }

        private void Create()
        {
            _groupCreater.Create(_outputName);
        }
    }
}