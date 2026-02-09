using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine.LevelEditor.Parent.New
{
    public class ParentView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textSelectedObject;
        [SerializeField] private TextMeshProUGUI parentObjectObject;
        [Space]
        [SerializeField] private TextMeshProUGUI rightButtonText;
        [SerializeField] private TextMeshProUGUI leftButtonText;
        [Space]
        [SerializeField] private Button rightButton;
        [SerializeField] private Button leftButton;
        [Space] 
        [SerializeField] private RectTransform panel;

        public Action SelectNewParent;
        public Action ApplyNewParent;
        public Action CancelSelectNewParent;
        public Action ClearParent;
        
        //<color=grey> <color>
        
        public void SetMode_SelectNewParent()
        {
            SetRightButton("Select new parent", SelectNewParent);
            SetLeftButton("Clear parent", ClearParent);
        }

        public void SetMode_ApplyNewParent()
        {
            SetRightButton("Apply new parent", ApplyNewParent);
            SetLeftButton("Cancel", CancelSelectNewParent);
        }

        public void SelectObject(string objectName, string parentName)
        {
            textSelectedObject.text = $"Selected: {objectName}";
            parentObjectObject.text = $"Parent: {parentName}";
        }

        public void NewParent(string parentName, string newParentName)
        {
            parentObjectObject.text = $"Parent: {parentName} ---> {newParentName}";
        }

        public void SetActivePanel(bool active)
        {
            panel.gameObject.SetActive(active);
        }
        

        private void SetRightButton(string text, Action action)
        {
            rightButtonText.text = text;
            rightButton.onClick.RemoveAllListeners();
            rightButton.onClick.AddListener(() =>
            {
                print(text);
                print(action);
                action?.Invoke();
            });
        }

        private void SetLeftButton(string text, Action action)
        {
            leftButtonText.text = text;
            leftButton.onClick.RemoveAllListeners();
            leftButton.onClick.AddListener(() =>
            {
                action?.Invoke();
            });
        }
    }
}