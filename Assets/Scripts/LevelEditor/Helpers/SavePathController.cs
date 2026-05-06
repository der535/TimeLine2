using TimeLine.LevelEditor.Save;
using TimeLine.Select_levels;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor
{
    public static class SavePathController
    {
        /// <summary>
        /// Даёт базовый путь к папке уровня
        /// </summary>
        /// <returns></returns>
        private static string GetBasePath()
        {
            return $"{Application.persistentDataPath}/Levels/{LevelBaseInfoStorage.levelBaseInfo.levelName}";
        }

        /// <summary>
        /// Возвращает точный путь к файлу json
        /// </summary>
        /// <param name="fileName">Название json файла</param>
        public static string GetJsonPath(string fileName)
        {
            return $"{GetBasePath()}/{fileName}.json";
        }
    }
}