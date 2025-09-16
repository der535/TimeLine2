using System;
using System.Collections.Generic;
using TimeLine.Components;
using TimeLine.CustomInspector.UI.FieldUI;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    public class AddComponentButton : MonoBehaviour, IGetFieldHeight
    {
         [SerializeField] private RectTransform rectTransform;
         [SerializeField] private TMP_Dropdown dropdown;
         
         private GameObject _target;
         private Dictionary<string, Type> components;
         
         internal void Setup(GameObject target)
         {
             _target = target;

             UpdateDropdown();
             
             dropdown.onValueChanged.AddListener(arg0 =>
             {
                 Type myType = components[dropdown.options[arg0].text];
                 ComponentRules.AddComponentSafely(myType, _target);
                 UpdateDropdown();
             });
         }

         private void UpdateDropdown()
         {
             Dictionary<string, Type> components = ComponentRules.GetAllComponents(_target);
             dropdown.ClearOptions();

             List<string> options = new List<string>();
             foreach (var component in components)
             {
                 options.Add(component.Key);
             }
             
             dropdown.AddOptions(options);
         }
         
         public float GetFieldHeight()
         {
             return rectTransform.rect.height;
         }
    }
}