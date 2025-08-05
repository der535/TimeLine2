using System;
using EventBus;
using TimeLine.Keyframe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public abstract class FieldUIBase<T> : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI text;
        [SerializeField] protected Button keyframeButton; // Добавленная кнопка

        private SceneObjectAddKeyFrame _sceneObjectAddKeyFrame;

        private IField<T> _field;
        private GameObject _targetObject;
        private string _componentName;
        private GameEventBus _eventBus;

        public virtual void Setup(
            IField<T> field, 
            Action onValueChanged, 
            GameObject target, 
            string component,
            SceneObjectAddKeyFrame sceneObjectAddKeyFrame,
            GameEventBus eventBus)
        {
            this._field = field;
            this._targetObject = target;
            this._eventBus = eventBus;
            text.text = field.Name;
            _componentName = component;
            _sceneObjectAddKeyFrame = sceneObjectAddKeyFrame;

            keyframeButton.onClick.AddListener(CreateKeyframe);
            print("AddListener(CreateKeyframe);");
        }

        protected virtual void CreateKeyframe()
        {
            _sceneObjectAddKeyFrame.AddKeyframe(_targetObject, new CustomParameterData(_field.Name, _field.Value), _componentName, _field.Name);
            // var data = new CustomParameterData(field.Name, field.Value);
            // eventBus.Raise(new AddKeyframeRequestEvent
            // {
            //     targetObject = targetObject,
            //     data = data
            // });
        }
    }
}