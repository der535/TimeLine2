using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

namespace TimeLine
{
    public static class LevelActions
    {
        public static void Copy(string levelName)
        {
            string levelPath = $"{Application.persistentDataPath}/Levels/{levelName}";

            if (string.IsNullOrWhiteSpace(levelPath))
                throw new ArgumentException("Укажите корректный путь к уровню.", nameof(levelPath));

            if (!Directory.Exists(levelPath))
                throw new DirectoryNotFoundException($"Папка уровня не найдена: {levelPath}");

            string parentDir = Path.GetDirectoryName(levelPath);
            string folderName = Path.GetFileName(levelPath);

            // Пытаемся найти шаблон: "<база> <число>"
            var match = Regex.Match(folderName, @"^(.+?)\s+(\d+)$");

            string baseName;
            int nextNumber;

            if (match.Success)
            {
                baseName = match.Groups[1].Value; // всё до последнего пробела и числа
                nextNumber = int.Parse(match.Groups[2].Value) + 1;
            }
            else
            {
                baseName = folderName;
                nextNumber = 2;
            }

            // Находим первое свободное имя вида "<baseName> <номер>"
            string newFolderName;
            string newFolderPath;
            do
            {
                newFolderName = $"{baseName} {nextNumber}";
                newFolderPath = Path.Combine(parentDir, newFolderName);
                nextNumber++;
            } while (Directory.Exists(newFolderPath));

            // Копируем папку
            CopyDirectory(levelPath, newFolderPath);
            
            LevelBaseInfo baseInfo = JsonConvert.DeserializeObject<LevelBaseInfo>(
                File.ReadAllText($"{Application.persistentDataPath}/Levels/{newFolderName}/LevelBaseInfo.json"));
            baseInfo.levelName = newFolderName;
            File.WriteAllText($"{Application.persistentDataPath}/Levels/{newFolderName}/LevelBaseInfo.json", JsonConvert.SerializeObject(baseInfo));
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(targetDir, fileName), true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                CopyDirectory(subDir, Path.Combine(targetDir, dirName));
            }
        }

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
                throw new InvalidOperationException(
                    $"Папка с именем \"{newName}\" уже существует в той же директории.");

            try
            {
                Directory.Move(levelPath, newFullPath);
                LevelBaseInfo baseInfo = JsonConvert.DeserializeObject<LevelBaseInfo>(
                    File.ReadAllText($"{Application.persistentDataPath}/Levels/{newName}/LevelBaseInfo.json"));
                baseInfo.levelName = newName;
                File.WriteAllText($"{Application.persistentDataPath}/Levels/{newName}/LevelBaseInfo.json", JsonConvert.SerializeObject(baseInfo));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new InvalidOperationException($"Не удалось переименовать папку: {ex.Message}", ex);
            }
        }
    }
}