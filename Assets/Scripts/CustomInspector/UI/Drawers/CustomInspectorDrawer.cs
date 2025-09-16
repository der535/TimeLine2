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
            var parameter = Instantiate(stringField, _currentComponent.RootObject);
            parameter.Setup(stringParameter);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateFloatField(FloatParameter floatParameter, Action createKeyframe)
        {
            var parameter = Instantiate(floatFieldUIPrefab, _currentComponent.RootObject);
            parameter.Setup(floatParameter, createKeyframe);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }
        
        public TMP_Dropdown CreateDropDownField(string parameterName)
        {
            var parameter = Instantiate(dropDownFieldUI, _currentComponent.RootObject);
            var dropdown = parameter.Setup(parameterName);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
            return dropdown;
        }
    
        public void CreateBoolField(BoolParameter field)
        {
            var parameter = Instantiate(boolField, _currentComponent.RootObject);
            parameter.Setup(field);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }
    
        public void AddSpace(float value)
        {
            var parameter = Instantiate(fieldSpace, _currentComponent.RootObject);
            parameter.Setup(value);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateVector2Field(Vector2Parameter vector2Parameter)
        {
            var parameter = Instantiate(vector2FieldUI, _currentComponent.RootObject);
            parameter.Setup(vector2Parameter);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }
    }
}