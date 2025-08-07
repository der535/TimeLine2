using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CustomInspector : MonoBehaviour
    {
        // [SerializeField] private GameObject target;
        // [Space] 
        [SerializeField] private RectTransform rootObject;
        [Space]
        [SerializeField] private ComponentUI componentUIPrefab;
        [SerializeField] private IntFieldUI intFieldUI;
        [SerializeField] private FloatFieldUI floatFieldUI;
        [SerializeField] private Vector2FieldUI vector2FieldUI;

        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectSceneObject data) => Draw(data.GameObject));
        }

        // [Button]
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
                
        
                // Обрабатываем все поля провайдера
                foreach (var field in provider.GetFields())
                {
                    if (field is IField<int> nField) // INT
                    {
                        IntFieldUI fieldUI = Instantiate(intFieldUI, componentUI.RootObject);
                        fieldUI.Setup(nField, provider.OnChangeCustomInspector);
                    }                    
                    if (field is IField<float> floatField) // INT
                    {
                        FloatFieldUI fieldUI = Instantiate(floatFieldUI, componentUI.RootObject);
                        fieldUI.Setup(floatField,provider.OnChangeCustomInspector);
                    }
                    if (field is IField<Vector2> vector2Field) // Vector 2
                    {
                        Vector2FieldUI fieldUI = Instantiate(vector2FieldUI, componentUI.RootObject);
                        fieldUI.Setup(vector2Field, provider.OnChangeCustomInspector);
                    }
                    // Добавьте обработку других типов при необходимости
                }
            }
        }
    }
}