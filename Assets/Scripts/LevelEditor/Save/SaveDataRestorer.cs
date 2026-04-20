using System;
using System.Collections.Generic;
using TimeLine.LevelEditor.Save;

public static class SaveDataRestorer
{
    /// <summary>
    /// Основной метод для запуска восстановления GraphNew во всем DTO
    /// </summary>
    public static void RestoreAllGraphs(SaveLevelDTO saveLevelDTO)
    {
        if (saveLevelDTO == null) return;

        // 1. Обрабатываем список обычных объектов
        if (saveLevelDTO.gameObjectSaveData != null)
        {
            foreach (var obj in saveLevelDTO.gameObjectSaveData)
            {
                ProcessGameObject(obj);
            }
        }

        // 2. Обрабатываем список групп
        if (saveLevelDTO.groupGameObjectSaveData != null)
        {
            foreach (var group in saveLevelDTO.groupGameObjectSaveData)
            {
                ProcessGameObject(group);
            }
        }
    }

    /// <summary>
    /// Рекурсивная обработка объекта. 
    /// Если это группа, метод пойдет вглубь по детям.
    /// </summary>
    private static void ProcessGameObject(GameObjectSaveData data)
    {
        if (data == null) return;

        // Восстанавливаем данные в треках текущего объекта
        if (data.tracks != null)
        {
            foreach (var track in data.tracks)
            {
                RestoreTrackGraphs(track);
            }
        }

        // Если это группа (GroupGameObjectSaveData), обрабатываем вложенных детей
        if (data is GroupGameObjectSaveData groupData && groupData.children != null)
        {
            foreach (var child in groupData.children)
            {
                // Рекурсивный вызов для обработки детей любого уровня вложенности
                ProcessGameObject(child);
            }
        }
    }

    /// <summary>
    /// Проход по ключевым кадрам трека
    /// </summary>
    private static void RestoreTrackGraphs(TrackSaveData track)
    {
        if (track.keyframeSaveData == null) return;

        foreach (var keyframe in track.keyframeSaveData)
        {
            // Условие: если GraphNew пуст, а старый Graph содержит данные
            if (keyframe.GraphNew == null && !string.IsNullOrEmpty(keyframe.Graph))
            {
                keyframe.GraphNew = ConvertStringToGraph(keyframe.Graph);
            }
        }
    }

    /// <summary>
    /// Метод десериализации/конвертации строки в объект GraphSaveData
    /// </summary>
    private static GraphSaveData ConvertStringToGraph(string graphRawData)
    {
        try
        {
            // Здесь должна быть ваша логика десериализации. 
            // Если Graph — это JSON строка:
            return Newtonsoft.Json.JsonConvert.DeserializeObject<GraphSaveData>(graphRawData);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Ошибка при восстановлении Graph: {ex.Message}");
            return null;
        }
    }
}