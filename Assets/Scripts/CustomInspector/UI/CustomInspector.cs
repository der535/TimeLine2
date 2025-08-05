using System;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CustomInspector : MonoBehaviour
    {
        [SerializeField] private RectTransform rootObject;
        [Space]
        [SerializeField] private ComponentUI componentUIPrefab;
        [SerializeField] private IntFieldUI intFieldUI;
        [SerializeField] private Vector2FieldUI vector2FieldUI;
        private SceneObjectAddKeyFrame _sceneObjectAddKeyFrame;

        private GameEventBus _gameEventBus;
        
        [Inject]
        void Construct(GameEventBus eventBus, SceneObjectAddKeyFrame sceneObjectAddKeyFrame)
        {
            _gameEventBus = eventBus;
            _sceneObjectAddKeyFrame = sceneObjectAddKeyFrame;
        }


        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectSceneObject data) => Draw(data.GameObject));
        }

        private void Draw(GameObject target)
        {
            foreach (Transform child in rootObject)
                Destroy(child.gameObject);

            // Получаем все провайдеры полей
            var providers = target.GetComponents<IFieldProvider>();
    
            foreach (var provider in providers)
            {
                // Создаем UI для каждого провайдера
                ComponentUI componentUI = Instantiate(componentUIPrefab, rootObject);
                componentUI.SetName(provider.GetType().Name);
                MonoBehaviour component = provider as MonoBehaviour;
        
                // Обрабатываем все поля провайдера
                foreach (var field in provider.GetFields())
                {
                    if (field is IField<int> intField)
                    {
                        var fieldUI = Instantiate(intFieldUI, componentUI.RootObject);
                        fieldUI.Setup(intField, 
                            provider.OnValueChanged, 
                            target, // GameObject владельца
                            component.name,
                            _sceneObjectAddKeyFrame,
                            _gameEventBus);
                    }
                    // Добавьте обработку других типов при необходимости
                }
            }
        }
    }
}
