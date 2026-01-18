using System.Collections.Generic;
using System.Linq;
using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine.Parent
{
    public class ParentLinkRestorer : MonoBehaviour
    {
        private TrackObjectStorage _trackObjectStorage;

        [Inject]
        private void Constructor(TrackObjectStorage trackObjectStorage)
        {
            _trackObjectStorage = trackObjectStorage;
        }

        public void Restor()
        {
            List<TrackObjectData> trackObjectData = new List<TrackObjectData>();
            trackObjectData.AddRange(_trackObjectStorage.TrackObjects);
            trackObjectData.AddRange(_trackObjectStorage.TrackObjectGroups);
            Restor(trackObjectData);
        }

        public void Restor(List<TrackObjectData> trackObjectData)
        {
            // print("=== НАЧАЛО ВОССТАНОВЛЕНИЯ ===");
            // print($"Получено объектов для восстановления: {trackObjectData.Count}");

            // Проверка на дубликаты
            var duplicates = trackObjectData
                .Where(x => x != null)
                .GroupBy(x => x.sceneObjectID)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicates.Any())
            {
                // Debug.LogError($"Обнаружены дублирующиеся ID объектов!");
                foreach (var group in duplicates)
                {
                    // Debug.LogError($"ID: {group.Key} встречается {group.Count()} раз(а)");
                    foreach (var item in group)
                    {
                        // Debug.LogError(
                            // $"  - Объект: {item.branch?.Name ?? "Неизвестно"}, SceneObject: {item.sceneObject?.name ?? "null"}");
                    }
                }
            }

            int processedCount = 0;
            int nullItemsSkipped = 0;
            int nullSceneObjectsSkipped = 0;
            int parentSetCount = 0;
            int parentNotFoundCount = 0;
            int selfParentAttempts = 0;

            // Создаем словарь для быстрого поиска по ID
            var objectsById = new Dictionary<string, TrackObjectData>();
            foreach (var item in trackObjectData)
            {
                if (item != null && !string.IsNullOrEmpty(item.sceneObjectID))
                {
                    if (objectsById.ContainsKey(item.sceneObjectID))
                    {
                        // Debug.LogWarning($"Дубликат ID {item.sceneObjectID} уже в словаре. Пропускаем.");
                    }
                    else
                    {
                        objectsById[item.sceneObjectID] = item;
                    }
                }
            }

            // print($"Уникальных объектов в словаре: {objectsById.Count}");

            foreach (var item in trackObjectData)
            {
                processedCount++;

                if (item == null)
                {
                    // Debug.LogWarning($"Элемент #{processedCount} в списке равен null. Пропускаем.");
                    nullItemsSkipped++;
                    continue;
                }

                // print($"\nОбработка элемента #{processedCount}:");
                // print($"  ID объекта: {item.sceneObjectID}");
                // print($"  Имя ветки: {item.branch?.Name ?? "Не указано"}");

                if (item.sceneObject == null)
                {
                    // Debug.LogWarning($"  SceneObject равен null для ветки: {item.branch?.Name ?? "Неизвестно"}");
                    nullSceneObjectsSkipped++;
                    continue;
                }

                // print($"  Имя объекта на сцене: {item.sceneObject.name}");

                // Проверяем trackObject на null
                if (item.trackObject == null)
                {
                    // Debug.LogWarning($"  TrackObject равен null! Пропускаем установку родителя.");
                    continue;
                }

                string parentId = item.trackObject._parentID;
                // print($"  Родительский ID: {parentId ?? "null"}");

                if (!string.IsNullOrEmpty(parentId))
                {
                    // Проверка на циклическую ссылку (объект ссылается сам на себя)
                    if (parentId == item.sceneObjectID)
                    {
                        // Debug.LogError(
                            // $"  ОШИБКА: Объект '{item.sceneObject.name}' пытается стать родителем самому себе!");
                        selfParentAttempts++;
                        // print($"  Очистка некорректного parentID...");
                        item.trackObject._parentID = string.Empty;
                        continue;
                    }

                    // print($"  Поиск родительского объекта с ID: {parentId}");

                    // Ищем родителя в словаре по ID
                    if (objectsById.TryGetValue(parentId, out var parentItem))
                    {
                        // print($"  Родительский объект найден в словаре!");

                        if (parentItem.sceneObject != null)
                        {
                            // Проверяем, не пытаемся ли установить уже существующего родителя
                            if (item.sceneObject.transform.parent != parentItem.sceneObject.transform)
                            {
                                // print(
                                    // $"  Установка родителя для '{item.sceneObject.name}' -> '{parentItem.sceneObject.name}'");
                                item.sceneObject.transform.SetParent(parentItem.sceneObject.transform, false);
                                parentSetCount++;

                                // Проверяем результат
                                if (item.sceneObject.transform.parent == parentItem.sceneObject.transform)
                                {
                                    // print($"  Родитель успешно установлен!");
                                }
                                else
                                {
                                    // Debug.LogError($"  ОШИБКА: Родитель не был установлен!");
                                }
                            }
                            else
                            {
                                // print($"  Родитель уже установлен ранее, пропускаем.");
                            }
                        }
                        else
                        {
                            // Debug.LogWarning($"  Найден родительский item, но его sceneObject равен null!");
                            // print($"  Очистка поля _parentID...");
                            item.trackObject._parentID = string.Empty;
                            parentNotFoundCount++;
                        }
                    }
                    else
                    {
                        // Попробуем найти в исходном списке (на случай если объект не попал в словарь)
                        var parentItemFromList = trackObjectData
                            .Find(x => x != null && x.sceneObjectID == parentId);

                        if (parentItemFromList != null && parentItemFromList.sceneObject != null)
                        {
                            // print($"  Родитель найден в исходном списке!");
                            item.sceneObject.transform.SetParent(parentItemFromList.sceneObject.transform, false);
                            parentSetCount++;
                        }
                        else
                        {
                            // Debug.LogWarning($"  Родительский объект с ID '{parentId}' не найден!");
                            // print($"  Объект: {item.branch?.Name ?? "Неизвестно"} (ID: {item.sceneObjectID})");
                            // print($"  Очистка поля _parentID...");
                            item.trackObject._parentID = string.Empty;
                            parentNotFoundCount++;
                        }
                    }
                }
                else
                {
                    // print($"  Родительский ID не указан, объект остаётся на корневом уровне");

                    // Если у объекта уже есть родитель, но в данных он указан как корневой
                    if (item.sceneObject.transform.parent != null)
                    {
                        // print(
                            // $"  Внимание: объект имеет родителя '{item.sceneObject.transform.parent.name}', но в данных parentID пустой");
                        // Можно раскомментировать следующую строку, чтобы сбрасывать родителя:
                        // item.sceneObject.transform.SetParent(null, false);
                    }
                }
            }

            // print("\n=== СТАТИСТИКА ВОССТАНОВЛЕНИЯ ===");
            // print($"Всего обработано элементов: {processedCount}");
            // print($"Пропущено null элементов: {nullItemsSkipped}");
            // print($"Пропущено null sceneObject: {nullSceneObjectsSkipped}");
            // print($"Установлено родительских связей: {parentSetCount}");
            // print($"Не найдено родителей: {parentNotFoundCount}");
            // print($"Попыток стать родителем самому себе: {selfParentAttempts}");
            // print(
                // $"Осталось корневых объектов: {trackObjectData.Count - nullItemsSkipped - nullSceneObjectsSkipped - parentSetCount}");

            // Дополнительная проверка результатов
            // print("\n=== ПРОВЕРКА РЕЗУЛЬТАТОВ ===");
            int hasParentCount = 0;
            int noParentCount = 0;
            foreach (var item in trackObjectData.Where(x => x?.sceneObject != null))
            {
                if (item.sceneObject.transform.parent != null)
                {
                    hasParentCount++;
                    // print($"  {item.sceneObject.name} -> родитель: {item.sceneObject.transform.parent.name}");
                }
                else
                {
                    noParentCount++;
                    // print($"  {item.sceneObject.name} -> корневой объект");
                }
            }

            // print($"Объектов с родителем: {hasParentCount}");
            // print($"Корневых объектов: {noParentCount}");

            // print("=== ВОССТАНОВЛЕНИЕ ЗАВЕРШЕНО ===\n");
        }
    }
}