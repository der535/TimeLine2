using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;

public class FileBrowserSelectFolder : MonoBehaviour
{
    public void OpenFolderSelectionDialog(System.Action<string> onFolderSelected)
    {
        StartCoroutine(ShowFolderSelectionDialog(onFolderSelected));
    }

    private IEnumerator ShowFolderSelectionDialog(System.Action<string> onFolderSelected)
    {
        // Устанавливаем фильтры - для выбора папок можно оставить пустым или использовать специальный фильтр
        FileBrowser.SetFilters(true);
        
        // Добавляем быстрые ссылки на часто используемые папки
        FileBrowser.AddQuickLink("Documents", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), null);
        FileBrowser.AddQuickLink("Desktop", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), null);
        
        // Открываем диалог выбора папки
        yield return FileBrowser.WaitForLoadDialog(
            FileBrowser.PickMode.Folders, // Важно: режим выбора папок
            false, // Одиночный выбор
            null, // Начальный путь (null - последняя использованная папка)
            null, // Начальное имя файла/папки
            "Выберите папку", // Заголовок
            "Выбрать" // Текст кнопки
        );

        // Обработка результата
        if (FileBrowser.Success)
        {
            // В режиме выбора папок возвращается путь к выбранной папке
            if (FileBrowser.Result.Length > 0)
            {
                string selectedFolder = FileBrowser.Result[0];
                Debug.Log("Выбрана папка: " + selectedFolder);
                onFolderSelected?.Invoke(selectedFolder);
            }
        }
    }
}