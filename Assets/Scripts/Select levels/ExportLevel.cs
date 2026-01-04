using System.IO.Compression;
using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Select_levels
{
    public class ExportLevel : MonoBehaviour
    {
        [SerializeField] private FileBrowserSelectFolder selectFolderBrowser;
        private GameEventBus _gameEventBus;
        private SaveLevel _saveLevel;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, SaveLevel saveLevel)
        {
            _gameEventBus = gameEventBus;
            _saveLevel = saveLevel;
        }
        
        public void Export()
        {
            selectFolderBrowser.OpenFolderSelectionDialog(s =>
            {
                Export(_saveLevel.LevelBaseInfo.levelName, s);
            });
        }

        private void Export(string levelName, string outputPath)
        {
            string levelPath =
                $"{Application.persistentDataPath}/Levels/{levelName}";
            
            ZipFile.CreateFromDirectory(levelPath, $"{outputPath}/{levelName}.zip");
        }
    }
}