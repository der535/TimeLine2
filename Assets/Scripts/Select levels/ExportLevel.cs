using System.IO.Compression;
using EventBus;
using TimeLine.LevelEditor.Save;
using TimeLine.Select_levels;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Select_levels
{
    public class ExportLevel : MonoBehaviour
    {
        [SerializeField] private FileBrowserSelectFolder selectFolderBrowser;
        
        public void Export()
        {
            selectFolderBrowser.OpenFolderSelectionDialog(s =>
            {
                Export(LevelBaseInfoStorage.levelBaseInfo.levelName, s);
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