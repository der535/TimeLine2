// Assets/Editor/AddFontSetterToPrefabs.cs
using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using TimeLine;

public class AddFontSetterToPrefabs : EditorWindow
{
    [MenuItem("Tools/Add FontSetter to TMP in Prefabs")]
    static void ShowWindow()
    {
        GetWindow<AddFontSetterToPrefabs>("Add FontSetter");
    }

    void OnGUI()
    {
        GUILayout.Label("Process all Prefabs with TMP Texts", EditorStyles.boldLabel);
        if (GUILayout.Button("Apply FontSetter to Prefabs"))
        {
            ProcessAllPrefabs();
        }
    }

    static void ProcessAllPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int modifiedCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            bool modified = false;
            var tmpComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var tmp in tmpComponents)
            {
                if (tmp != null && tmp.GetComponent<FontSetter>() == null)
                {
                    Undo.RecordObject(prefab, "Add FontSetter");
                    tmp.gameObject.AddComponent<FontSetter>();
                    modified = true;
                }
            }

            if (modified)
            {
                EditorUtility.SetDirty(prefab);
                modifiedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Processed {prefabGuids.Length} prefabs. Modified {modifiedCount}.");
    }
}