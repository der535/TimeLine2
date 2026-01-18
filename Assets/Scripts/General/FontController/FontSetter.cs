using TimeLine.General.Installers;
using TMPro;
using UnityEngine;
using Zenject;

// Пространство имен TimeLine, содержащее классы для работы с временной шкалой
namespace TimeLine
{
    /// Йо На свзи Aru
    /// <summary>
    /// Компонент для установки шрифта текстовому элементу в Unity
    /// Наследуется от MonoBehaviour для работы в Unity-сцене
    /// Использует Dependency Injection для получения зависимости
    /// </summary>
    public class FontSetter : MonoBehaviour
    {
        // Поле для выбора названия шрифта через Inspector
        // Сериализуемое поле, отображается в редакторе Unity
        [SerializeField] private FontNames fontNames;

        // Приватное поле для хранения ссылки на хранилище шрифтов
        // Заполняется через Dependency Injection
        private FontStorage _fontStorage;

        /// <summary>
        /// Метод-конструктор для внедрения зависимостей (Dependency Injection)
        /// Помечен атрибутом [Inject] для использования в DI-контейнере
        /// </summary>
        /// <param name="fontStorage">Экземпляр хранилища шрифтов, предоставляемый DI-контейнером</param>
        [Inject]
        private void Constructor(FontStorage fontStorage)
        {
            // Сохраняем переданное хранилище шрифтов в приватное поле
            _fontStorage = fontStorage;

            // Получаем компонент TextMeshProUGUI с текущего GameObject
            // Используем полученное хранилище для получения нужного шрифта
            // Устанавливаем шрифт компоненту TextMeshProUGUI
            gameObject.GetComponent<TextMeshProUGUI>().font = _fontStorage.GetFont(fontNames);
        }
    }
}
