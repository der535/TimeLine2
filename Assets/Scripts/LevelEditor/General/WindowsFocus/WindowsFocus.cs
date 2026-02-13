using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.EventSystems;
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
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = UnityEngine.Input.mousePosition;

            // Список для попаданий по ВСЕМ канвасам в сцене
            List<RaycastResult> results = new List<RaycastResult>();
    
            // Глобальная проверка через EventSystem
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                // Нас интересует только самый первый (верхний) объект
                GameObject topObject = results[0].gameObject;

                // Проверяем, принадлежит ли самый верхний объект нашей панели
                return topObject == focusPanel.gameObject || topObject.transform.IsChildOf(focusPanel);
            }

            return false;
        }
    }
}
