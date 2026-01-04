using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TimeLine.General.Installers
{
    /// <summary>
    /// Перечисление доступных типов шрифтов в проекте.
    /// Используется для исключения ошибок при поиске шрифта по строковому имени.
    /// </summary>
    public enum FontNames
    {
        Standard,  // Основной текст интерфейса
        GameName,  // Шрифт для названия игры / логотипа
        Arrows     // Специальный шрифт с символами стрелок
    }

    /// <summary>
    /// Хранилище шрифтов (Font Storage).
    /// Позволяет централизованно управлять шрифтами TextMeshPro и получать их по названию.
    /// </summary>
    public class FontStorage : MonoBehaviour
    {
        [Header("Настройки шрифтов")]
        [Tooltip("Список сопоставления логического имени и физического ассета шрифта")]
        [SerializeField] private List<FontItem> fontItems = new();

        /// <summary>
        /// Поиск и получение ассета шрифта по его имени в перечислении.
        /// </summary>
        /// <param name="fontName">Имя шрифта из списка FontNames</param>
        /// <returns>Ассет TMP_FontAsset или null, если шрифт не найден</returns>
        public TMP_FontAsset GetFont(FontNames fontName)
        {
            // Находим элемент списка, где имя совпадает с запрошенным
            var item = fontItems.Find(x => x.FontName == fontName);
            
            if (item == null)
            {
                Debug.LogWarning($"Шрифт с именем {fontName} не найден в FontStorage на объекте {gameObject.name}");
                return null;
            }

            return item.Asset;
        }
    }

    /// <summary>
    /// Вспомогательный класс для связи названия шрифта с файлом ассета.
    /// Отображается в инспекторе Unity благодаря атрибуту [Serializable].
    /// </summary>
    [Serializable]
    public class FontItem
    {
        public FontNames FontName; // Логическое имя (ключ)
        public TMP_FontAsset Asset; // Ссылка на файл шрифта
    }
}