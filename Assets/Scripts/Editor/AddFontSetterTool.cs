// Assets/Editor/AddFontSetterTool.cs
using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using TimeLine; // Пространство имен, где, вероятно, находится ваш FontSetter

/// <summary>
/// Окно редактора для массового добавления компонента FontSetter на объекты с TextMeshProUGUI.
/// </summary>
public class AddFontSetterTool : EditorWindow
{
    // Добавляет пункт меню в верхнюю панель Unity: Tools -> Add FontSetter to TMP
    [MenuItem("Tools/Add FontSetter to TMP")]
    static void ShowWindow()
    {
        // Открывает или фокусирует существующее окно этого типа
        GetWindow<AddFontSetterTool>("FontSetter Tool");
    }

    // Отрисовка интерфейса окна
    void OnGUI()
    {
        GUILayout.Label("Add FontSetter where missing", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Кнопка для обработки только текущей открытой сцены
        if (GUILayout.Button("Process Scene Objects"))
        {
            ProcessSceneObjects();
        }

        // Кнопка для обработки всех префабов в папке Assets
        if (GUILayout.Button("Process All Prefabs"))
        {
            ProcessAllPrefabs();
        }
    }

    // ——— СЦЕНА ———
    /// <summary>
    /// Ищет объекты на текущей сцене и добавляет FontSetter там, где есть TMP, но нет сеттера.
    /// </summary>
    static void ProcessSceneObjects()
    {
        // Находим вообще все GameObject (включая неактивные и ассеты)
        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int addedCount = 0;

        foreach (var go in allObjects)
        {
            // Фильтр: пропускаем системные объекты и те, что являются частью файлов-префабов (нам нужны только экземпляры на сцене)
            if (go.hideFlags != HideFlags.None) continue;
            if (PrefabUtility.IsPartOfPrefabAsset(go)) continue;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            // Если текст есть, а нашего скрипта нет
            if (tmp != null && go.GetComponent<FontSetter>() == null)
            {
                // Позволяет отменить действие через Ctrl+Z
                Undo.RecordObject(go, "Add FontSetter (Scene)");
                
                go.AddComponent<FontSetter>();
                
                // Помечаем объект как "измененный", чтобы Unity предложила сохранить сцену
                EditorUtility.SetDirty(go);
                addedCount++;
            }
        }

        Debug.Log($"[Scene] Added FontSetter to {addedCount} TMP objects.");
    }

    // ——— ПРЕФАБЫ ———
    /// <summary>
    /// Сканирует все файлы префабов (.prefab) в проекте и добавляет FontSetter внутри них.
    /// </summary>
    static void ProcessAllPrefabs()
    {
        // Ищем GUID всех ассетов типа Prefab через базу данных проекта
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int modifiedCount = 0;

        foreach (string guid in prefabGuids)
        {
            // Превращаем GUID в понятный путь к файлу
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Загружаем префаб как объект
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool modified = false;
            // Ищем все компоненты TMP во вложенных объектах префаба
            var tmpComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var tmp in tmpComponents)
            {
                // Если на дочернем объекте префаба нет FontSetter
                if (tmp != null && tmp.GetComponent<FontSetter>() == null)
                {
                    Undo.RecordObject(prefab, "Add FontSetter (Prefab)");
                    tmp.gameObject.AddComponent<FontSetter>();
                    modified = true;
                }
            }

            // Если префаб был изменен, сохраняем изменения в файл
            if (modified)
            {
                EditorUtility.SetDirty(prefab);
                modifiedCount++;
            }
        }

        // Принудительно сохраняем все изменения ассетов на диск
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Prefabs] Processed {prefabGuids.Length} prefabs. Modified {modifiedCount}.");
    }
}