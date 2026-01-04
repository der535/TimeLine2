using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace TimeLine.LevelEditor.Select_levels
{
    public class ImportLevel  : MonoBehaviour
    {
        [SerializeField] private FileBrowserSelectZip selectZip;
        [SerializeField] private LevelCardController levelCardController;
        
        public void Import()
        {
            selectZip.OpenFilePanel((list =>
            {
                foreach (var item in list)
                {
                    string levelPath =
                        $"{Application.persistentDataPath}/Levels/{item}";
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(item);
                    ZipFile.ExtractToDirectory(item, $"{Application.persistentDataPath}/Levels/{fileNameWithoutExt}");
                    levelCardController.UpdateCards();
                }
            }));
        }
    }
}