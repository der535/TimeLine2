using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine.Parent
{
    public class ParentComponent : MonoBehaviour
    {
        private ParentMain _parentMain;
        
        private TMP_Dropdown _dropdown;
        private GameObject _sceneObject;

        private TrackObjectData _currentParent; // Текущий выбранный родительский объект

        [Inject]
        private void Construct(ParentMain parentMain)
        {
            _parentMain = parentMain;
        }

        private void Start()
        {
            _parentMain.OnTrackObjectSelected += _ => InitializeDropdown();
        }

        internal void Setup(TMP_Dropdown dropdown, GameObject sceneObject)
        {
            _dropdown = dropdown;
            _sceneObject = sceneObject;
            dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            _parentMain.InvokeOnTrackObjectSelected();
            InitializeDropdown();
            
            print(sceneObject.gameObject.name);
        }

        private void InitializeDropdown()
        {
            _dropdown.ClearOptions();

            List<string> options = new List<string>();

            
            // Добавляем объекты в Dropdown
            foreach (var obj in _parentMain.sceneGameObjects)
            {
                if (obj != null && obj.sceneObject != _sceneObject)
                {
                    options.Add(obj.sceneObject != null ? obj.sceneObject.name : "Empty");
                }
            }
            
            _dropdown.AddOptions(options);

            // Устанавливаем текущее значение Dropdown
            if (_currentParent != null)
            {
                int index = _parentMain.sceneGameObjects.IndexOf(_currentParent);
                if (index >= 0)
                {
                    _dropdown.value = index;
                    _dropdown.captionText.text =
                        _currentParent.sceneObject != null ? _currentParent.sceneObject.name : "Empty";
                }
            }
            else if (_parentMain.sceneGameObjects.Count > 0)
            {
                // Устанавливаем первый элемент по умолчанию, если currentParent не задан
                _dropdown.value = 0;
                _currentParent = _parentMain.sceneGameObjects[0];
            }
        }

        private void OnDropdownValueChanged(int index)
        {
            // Получаем актуальный список объектов
            List<TrackObjectData> targetObjects = _parentMain.sceneGameObjects;

            if (index >= 0 && index < targetObjects.Count)
            {
                if (targetObjects[index].sceneObject != gameObject)
                {
                    _currentParent = targetObjects[index];
                    transform.parent = _currentParent.sceneObject.transform;
                }
                else
                {
                    _dropdown.value = 0;
                }
            }
        }
    }
}