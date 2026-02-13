using System;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.InspectorView.FieldUI;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.FieldUI;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers
{
    public class CustomInspectorDrawer : MonoBehaviour
    {
        [SerializeField] private RectTransform rootObject;
    
        [SerializeField] private ComponentUI componentUIPrefab;
    
        [Header("Fields")]
        [SerializeField] private IntFieldUI intFieldUIPrefab;
        [SerializeField] private FloatFieldUI floatFieldUIPrefab;
        [SerializeField] private DropDownFieldUI dropDownFieldUI;
        [SerializeField] private Vector2FieldUI vector2FieldUI;
        [SerializeField] private FieldSpace fieldSpace;
        [SerializeField] private BoolFieldUI boolField;
        [SerializeField] private StringFieldUI stringField;
        [SerializeField] private SpriteFieldUI spriteField;
        [SerializeField] private ColorFieldUI colorField;
        [SerializeField] private CompositionFieldUI compositionField;
        [SerializeField] private KeyCodeFieldUI fieldUI;
        [Space] 
        [SerializeField] private EditColliderFieldUI editColliderButton;
        [Space]
        [SerializeField] private AddComponentButton button;
    
        private ComponentUI _currentComponent;
        private DiContainer _container;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;  
        }

        public void CreateComponent(Component component, bool isRemovable)
        {
            _currentComponent = _container.InstantiatePrefab(componentUIPrefab, rootObject).GetComponent<ComponentUI>();
            _currentComponent.Setup(component, isRemovable);
        }

        public void CreateStringField(StringParameter stringParameter)
        {
            var parameter = Instantiate(stringField, _currentComponent.RootObject);
            parameter.Setup(stringParameter);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateSelectComposition(CompositionParameter compositionParameter)
        {
            var parameter = _container.InstantiatePrefab(compositionField, _currentComponent.RootObject).GetComponent<CompositionFieldUI>();
            parameter.Setup(compositionParameter);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateKeyCode(KeyCodeParameter keyCodeParameter)
        {
            var parameter = _container.InstantiatePrefab(fieldUI, _currentComponent.RootObject).GetComponent<KeyCodeFieldUI>();
            print(parameter);
            parameter.Setup(keyCodeParameter, () => print("create keyframe (placeholder)"));
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateAddComponentButton(GameObject target)
        {
            var parameter = _container.InstantiatePrefab(button, rootObject).GetComponent<AddComponentButton>();
            parameter.Setup(target);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateFloatField(FloatParameter floatParameter, TrackObjectData trackObjectData, BaseParameterComponent component, string gameObjectID, Action createKeyframe)
        {
            var parameter = _container.InstantiatePrefab(floatFieldUIPrefab, _currentComponent.RootObject).GetComponent<FloatFieldUI>();
            parameter.Setup(trackObjectData, component, floatParameter, gameObjectID, createKeyframe);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }
        
        public void CreateIntField(IntParameter intParameter, Action createKeyframe)
        {
            var parameter = Instantiate(intFieldUIPrefab, _currentComponent.RootObject);
            parameter.Setup(intParameter, createKeyframe);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }
        
        public void CreateColorField(ColorParameter colorParameter, Action createKeyframe, string gameObjectID)
        {
            var parameter = _container.InstantiatePrefab(colorField, _currentComponent.RootObject).GetComponent<ColorFieldUI>();
            parameter.Setup(colorParameter, createKeyframe, gameObjectID);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }
        
        public TMP_Dropdown CreateDropDownField(string parameterName)
        {
            var parameter = Instantiate(dropDownFieldUI, _currentComponent.RootObject);
            var dropdown = parameter.Setup(parameterName);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
            return dropdown;
        }

        public void CreateEditColliderButton()
        {
            var button = _container.InstantiatePrefab(editColliderButton, _currentComponent.RootObject).GetComponent<EditColliderFieldUI>();
            button.Setup();
        }
    
        public void CreateBoolField(BoolParameter field)
        {
            var parameter = Instantiate(boolField, _currentComponent.RootObject);
            parameter.Setup(field);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }
        
        public void CreateSpriteField(SpriteParameter field)
        {
            // print("CreateSpriteField");
            var parameter = _container.InstantiatePrefab(spriteField, _currentComponent.RootObject).GetComponent<SpriteFieldUI>();
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