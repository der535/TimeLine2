using System;
using EventBus;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class WindowsFocus : MonoBehaviour
    {
        [SerializeField] private RectTransform focusPanel;
        [SerializeField] private GameObject focusObject;
        
        private ActionMap _actionMap;
        private MainObjects _mainObjects;

        private bool _active;

        public bool IsFocused => _active;
        
        [Inject]
        private void Construct(ActionMap actionMap, MainObjects mainObjects)
        {
            _actionMap = actionMap;
            _mainObjects = mainObjects;
        }
        
        private void Start()
        {
            focusObject.SetActive(false);
            _actionMap.Editor.MouseLeft.started += _ =>
            {
                _active = CheckMouseInWindow();
                focusObject.SetActive(_active);
            };
        }

        private bool CheckMouseInWindow()
        {
            return RectTransformUtility.RectangleContainsScreenPoint(focusPanel,  
                UnityEngine.Input.mousePosition, _mainObjects.MainCamera);  
        }
    }
}
