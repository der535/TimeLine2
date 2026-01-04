using System;
using System.IO;
using UnityEngine;

namespace TimeLine
{
    public static class LevelDeleter
    {
        /// <summary>
        /// Удаляет папку уровня по указанному пути.
        /// </summary>
        /// <param name="levelPath">Полный путь к папке уровня.</param>
        public static void DeleteLevel(string levelName)
        {
            string levelPath = $"{Application.persistentDataPath}/Levels/{levelName}";
            
            if (string.IsNullOrWhiteSpace(levelPath))
                throw new ArgumentException("Путь к уровню не может быть пустым.", nameof(levelPath));

            if (!Directory.Exists(levelPath))
                throw new DirectoryNotFoundException($"Папка уровня не найдена: {levelPath}");

            try
            {
                Directory.Delete(levelPath, recursive: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new InvalidOperationException($"Не удалось удалить папку уровня: {ex.Message}", ex);
            }
        }
    }
}