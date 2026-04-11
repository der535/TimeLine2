
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class RenameComposition : MonoBehaviour
    {
        [SerializeField] private RectTransform renameCompositionPanel;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private SaveComposition saveComposition;
        [Space]
        [SerializeField] private Button ok;
        [SerializeField] private Button close;
        [SerializeField] private Button cancel;

        public RectTransform RenameCompositionPanel => renameCompositionPanel;

        private ActionMap _actionMap;
        
        [Inject]
        private void Construct(ActionMap actionMap)
        {
            _actionMap = actionMap;
        }

        private void Start()
        {
            close.onClick.AddListener(ClosePanel);
            cancel.onClick.AddListener(ClosePanel);
        }

        internal void Setup(string compositionID, string compositionName)
        {
            _actionMap.Editor.Disable();
            ok.onClick.RemoveAllListeners();
            inputField.text = compositionName;
            ok.onClick.AddListener(() =>
            {
                ClosePanel();
                saveComposition.Rename(inputField.text, compositionID);
                saveComposition.UpdateCompositionCards();
            });
        }

        private void ClosePanel()
        {
            RenameCompositionPanel.gameObject.SetActive(false);
            _actionMap.Editor.Enable();
        }
    }
}
