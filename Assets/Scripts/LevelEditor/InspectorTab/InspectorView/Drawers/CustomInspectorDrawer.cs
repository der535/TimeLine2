using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.InspectorView.FieldUI;
using TimeLine.LevelEditor.InspectorTab.InspectorView.FieldUI;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.FieldUI;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TMPro;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers
{
    public class CustomInspectorDrawer : MonoBehaviour
    {
        [SerializeField] private RectTransform rootObject;

        [SerializeField] private ComponentUI componentUIPrefab;

        [Header("Fields")] [SerializeField] private IntFieldUI intFieldUIPrefab;
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
        [Space] [SerializeField] private EditColliderFieldUI editColliderButton;
        [Space] [SerializeField] private AddComponentButton button;

        private ComponentUI _currentComponent;
        private DiContainer _container;
        private GetSpriteName _getSpriteName;

        [Inject]
        private void Construct(DiContainer container, GetSpriteName getSpriteName)
        {
            _container = container;
            _getSpriteName = getSpriteName;
        }

        /// <summary>
        /// Создаёт компонент
        /// </summary>
        /// <param name="name">Список типов структу которые будут удаляться</param>
        /// <param name="componentName">Имя компонента</param>
        /// <param name="entity">Сущность к которой эти компоненты будут привязанны</param>
        /// <param name="isRemovable">Компонента удаляемый?</param>
        public void CreateComponent(ComponentNames name, Entity entity, bool isRemovable)
        {
            _currentComponent = _container.InstantiatePrefab(componentUIPrefab, rootObject).GetComponent<ComponentUI>();
            _currentComponent.Setup(name, entity, isRemovable);
        }

        public void CreateStringField(StringParameter stringParameter)
        {
            var parameter = Instantiate(stringField, _currentComponent.RootObject);
            parameter.Setup(stringParameter);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateStringField(string stringParameter, string parameterName, Action<string> onValueChanged)
        {
            var parameter = Instantiate(stringField, _currentComponent.RootObject);
            parameter.Setup(stringParameter, parameterName, onValueChanged);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateSelectComposition(CompositionParameter compositionParameter)
        {
            var parameter = _container.InstantiatePrefab(compositionField, _currentComponent.RootObject)
                .GetComponent<CompositionFieldUI>();
            parameter.Setup(compositionParameter);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateKeyCode(KeyCodeParameter keyCodeParameter)
        {
            var parameter = _container.InstantiatePrefab(fieldUI, _currentComponent.RootObject)
                .GetComponent<KeyCodeFieldUI>();
            print(parameter);
            parameter.Setup(keyCodeParameter, () => print("create keyframe (placeholder)"));
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateAddComponentButton(Entity target)
        {
            var parameter = _container.InstantiatePrefab(button, rootObject).GetComponent<AddComponentButton>();
            parameter.Setup(target);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateFloatField(FloatParameter floatParameter, TrackObjectPacket trackObjectPacket,
            BaseParameterComponent component, string gameObjectID, Action createKeyframe, string fieldId)
        {
            var parameter = _container.InstantiatePrefab(floatFieldUIPrefab, _currentComponent.RootObject)
                .GetComponent<FloatFieldUI>();
            parameter.Setup(trackObjectPacket, component, floatParameter, gameObjectID, createKeyframe, fieldId);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateFloatField(float startValue, string parameterName, Action createKeyframe,
            Action<float> onValueChanged, TrackObjectPacket trackObjectPacket, string fieldID,
            FloatParameter onValueChangedSub = null)
        {
            var parameter = _container.InstantiatePrefab(floatFieldUIPrefab, _currentComponent.RootObject)
                .GetComponent<FloatFieldUI>();
            parameter.Setup(startValue, parameterName, createKeyframe, onValueChanged, trackObjectPacket, fieldID,
                onValueChangedSub);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateIntField(IntParameter intParameter, Action createKeyframe)
        {
            var parameter = Instantiate(intFieldUIPrefab, _currentComponent.RootObject);
            parameter.Setup(intParameter, createKeyframe);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateIntField(float startValue, string parameterName, Action<float> onValueChange,
            Action createKeyframe)
        {
            var parameter = Instantiate(intFieldUIPrefab, _currentComponent.RootObject);
            parameter.Setup(startValue, parameterName, onValueChange, createKeyframe);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }


        public void CreateColorField(Action<Color> changeColor, Color startColor, Action createKeyframe)
        {
            var parameter = _container.InstantiatePrefab(colorField, _currentComponent.RootObject)
                .GetComponent<ColorFieldUI>();
            parameter.Setup(changeColor, startColor, createKeyframe);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateEditColliderButton()
        {
            var button = _container.InstantiatePrefab(editColliderButton, _currentComponent.RootObject)
                .GetComponent<EditColliderFieldUI>();
            button.Setup();
        }

        // public void CreateBoolField(BoolParameter field)
        // {
        //     var parameter = Instantiate(boolField, _currentComponent.RootObject);
        //     parameter.Setup(field);
        //     _currentComponent.AddHeight(parameter.GetFieldHeight());
        // }

        /// <summary>
        /// Создаёт переключатель в инспекторе
        /// </summary>
        /// <param name="startValue">Стартовое значение</param>
        /// <param name="parameterName">Имя параметра</param>
        /// <param name="onValueChanged">Действие при смене значения</param>
        public void CreateBoolField(bool startValue, string parameterName, Action<bool> onValueChanged)
        {
            var parameter = Instantiate(boolField, _currentComponent.RootObject);
            parameter.Setup(startValue, parameterName, onValueChanged);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }


        public void CreateSpriteField(SpriteParameter field)
        {
            // print("CreateSpriteField");
            var parameter = _container.InstantiatePrefab(spriteField, _currentComponent.RootObject)
                .GetComponent<SpriteFieldUI>();
            parameter.Setup(field);
            _currentComponent.AddHeight(parameter.GetFieldHeight());
        }

        public void CreateSpriteField(string spriteName, Action<Texture> onValueChanged)
        {
            var parameter = _container.InstantiatePrefab(spriteField, _currentComponent.RootObject)
                .GetComponent<SpriteFieldUI>();
            parameter.Setup(_getSpriteName.GetSpriteFromName(spriteName), onValueChanged);
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