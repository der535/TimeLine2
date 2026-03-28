using TimeLine.LevelEditor.Save;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor
{
    public class SavePathController
    {
        private SaveLevel _saveLevel;

        [Inject]
        private void Construct(SaveLevel saveLevel)
        {
            _saveLevel = saveLevel;
        }

        /// <summary>
        /// Даёт базовый путь к папке уровня
        /// </summary>
        /// <returns></returns>
        public string GetBasePath()
        {
            return $"{Application.persistentDataPath}/Levels/{_saveLevel.LevelBaseInfo.levelName}";
        }

        /// <summary>
        /// Возвращает точный путь к файлу json
        /// </summary>
        /// <param name="fileName">Название json файла</param>
        public string GetJsonPath(string fileName)
        {
            return $"{GetBasePath()}/{fileName}.json";
        }
    }
}