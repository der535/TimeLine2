using System;
using System.IO;
using System.Linq;

public static class BackupManager
{
    public static void CreateRollingBackup(string sourcePath, int maxBackups = 5)
    {
        string backupRootDir = Path.Combine(sourcePath, "backup");

        try
        {
            // 1. Создаем корневую папку бэкапа, если её нет
            if (!Directory.Exists(backupRootDir))
            {
                Directory.CreateDirectory(backupRootDir);
            }

            // 2. Управляем ротацией (сдвигаем папки)
            // Идем с конца: 3 -> удалить, 2 -> 3, 1 -> 2
            for (int i = maxBackups; i >= 1; i--)
            {
                string currentDir = Path.Combine(backupRootDir, $"backup {i}");
                
                if (Directory.Exists(currentDir))
                {
                    if (i == maxBackups)
                    {
                        // Удаляем самую старую папку
                        Directory.Delete(currentDir, true);
                    }
                    else
                    {
                        // Переименовываем (сдвигаем) текущую в следующую
                        string nextDir = Path.Combine(backupRootDir, $"backup {i + 1}");
                        Directory.Move(currentDir, nextDir);
                    }
                }
            }

            // 3. Создаем свежий "backup 1"
            string targetDir = Path.Combine(backupRootDir, "backup 1");
            Directory.CreateDirectory(targetDir);

            // 4. Копируем данные
            CopyContents(sourcePath, targetDir, backupRootDir);

            Console.WriteLine("Бэкап успешно обновлен. Новый бэкап сохранен в 'backup 1'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании бэкапа: {ex.Message}");
        }
    }

    private static void CopyContents(string source, string target, string ignorePath)
    {
        // Копируем файлы
        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        }

        // Копируем папки (рекурсивно), пропуская саму папку с бэкапами
        foreach (var dir in Directory.GetDirectories(source))
        {
            // Важно: сравниваем полные пути, чтобы не войти в рекурсию
            if (dir.TrimEnd(Path.DirectorySeparatorChar) != ignorePath.TrimEnd(Path.DirectorySeparatorChar))
            {
                string dirName = Path.GetFileName(dir);
                CopyDirectoryRecursive(dir, Path.Combine(target, dirName));
            }
        }
    }

    private static void CopyDirectoryRecursive(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        
        foreach (var dir in Directory.GetDirectories(source))
            CopyDirectoryRecursive(dir, Path.Combine(target, Path.GetFileName(dir)));
    }
}