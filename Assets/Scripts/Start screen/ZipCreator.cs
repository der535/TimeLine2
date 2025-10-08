using System.IO;
using System.IO.Compression;
using NaughtyAttributes;
using UnityEngine;

public class ZipCreator : MonoBehaviour
{
    [Button]
    public void CreateZip(string zipPath, string[] filePaths)
    {
        try
        {
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        string fileName = Path.GetFileName(filePath);
                        archive.CreateEntryFromFile(filePath, fileName);
                    }
                }
            }
            Debug.Log("Архив создан: " + zipPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при создании архива: " + e.Message);
        }
    }
}