using System;
using TimeLine.CustomInspector.Logic.Parameter;
using TMPro;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class CustomInspectorDrawer : MonoBehaviour
    {
        [SerializeField] private RectTransform rootObject;
    
        [SerializeField] private ComponentUI componentUIPrefab;
    
        [Header("Fields")]
        [SerializeField] private FloatFieldUI floatFieldUIPrefab;
        [SerializeField] private DropDownFieldUI dropDownFieldUI;
        [SerializeField] private Vector2FieldUI vector2FieldUI;
        [SerializeField] private FieldSpace fieldSpace;
        [SerializeField] private BoolFieldUI boolField;
        [SerializeField] private StringFieldUI stringField;
    
        private ComponentUI _currentComponent;

        public void CreateComponent(string componentName)
        {
            _currentComponent = Instantiate(componentUIPrefab, rootObject);
            _currentComponent.SetName(componentName);
        }

        public void CreateStringField(StringParameter stringParameter)
        {
            Instantiate(stringField, _currentComponent.RootObject).Setup(stringParameter);
        }

        public void CreateFloatField(FloatParameter floatParameter, Action createKeyframe)
        {
            Instantiate(floatFieldUIPrefab, _currentComponent.RootObject).Setup(floatParameter, createKeyframe);
        }
        
        public TMP_Dropdown CreateDropDownField(string parameterName)
        {
           return Instantiate(dropDownFieldUI, _currentComponent.RootObject).Setup(parameterName);
        }
    
        public void CreateBoolField(BoolParameter field)
        {
            Instantiate(boolField, _currentComponent.RootObject).Setup(field);
        }
    
        public void AddSpace(float value)
        {
            Instantiate(fieldSpace, _currentComponent.RootObject).Setup(value);
        }

        public void CreateVector2Field(Vector2Parameter vector2Parameter)
        {
            Instantiate(vector2FieldUI, _currentComponent.RootObject).Setup(vector2Parameter);
        }
    }
}