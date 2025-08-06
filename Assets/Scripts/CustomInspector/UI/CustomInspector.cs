using NaughtyAttributes;
using UnityEngine;

namespace TimeLine
{
    public class CustomInspector : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [Space] 
        [SerializeField] private RectTransform rootObject;
        [Space]
        [SerializeField] private ComponentUI componentUIPrefab;
        [SerializeField] private IntFieldUI intFieldUI;
        [SerializeField] private Vector2FieldUI vector2FieldUI;

        [Button]
        private void Draw()
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
                    if (field is IField<int> intField) // INT
                    {
                        IntFieldUI fieldUI = Instantiate(intFieldUI, componentUI.RootObject);
                        fieldUI.Setup(intField, provider.OnValueChanged);
                    }
                    if (field is IField<Vector2> vector2Field) // Vector 2
                    {
                        Vector2FieldUI fieldUI = Instantiate(vector2FieldUI, componentUI.RootObject);
                        fieldUI.Setup(vector2Field, provider.OnValueChanged);
                    }
                    // Добавьте обработку других типов при необходимости
                }
            }
        }
    }
}