using System;
using System.IO;
using UnityEngine;

namespace TimeLine
{
    public static class LevelRenamer
    {
        /// <summary>
        /// Переименовывает папку уровня.
        /// </summary>
        /// <param name="levelPath">Текущий полный путь к папке уровня.</param>
        /// <param name="newName">Новое имя папки (без пути).</param>
        public static void RenameLevel(string levelName, string newName)
        {
            string levelPath = $"{Application.persistentDataPath}/Levels/{levelName}";
            
            if (string.IsNullOrWhiteSpace(levelPath))
                throw new ArgumentException("Путь к уровню не может быть пустым.", nameof(levelPath));

            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Новое имя уровня не может быть пустым.", nameof(newName));

            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException($"Имя \"{newName}\" содержит недопустимые символы.", nameof(newName));

            if (!Directory.Exists(levelPath))
                throw new DirectoryNotFoundException($"Папка уровня не найдена: {levelPath}");

            string parentDir = Path.GetDirectoryName(levelPath);
            string newFullPath = Path.Combine(parentDir, newName);

            if (Directory.Exists(newFullPath))
                throw new InvalidOperationException($"Папка с именем \"{newName}\" уже существует в той же директории.");

            try
            {
                Directory.Move(levelPath, newFullPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new InvalidOperationException($"Не удалось переименовать папку: {ex.Message}", ex);
            }
        }
    }
}