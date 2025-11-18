using System;
using System.Globalization;
using System.IO;
using EventBus;
using SFB;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class CreateLevel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _name;
        [SerializeField] private TMP_InputField _bpm;
        [SerializeField] private TextMeshProUGUI _textOnButtonLoadSong;
        [SerializeField] private Button _createButton;

        [FormerlySerializedAs("createScreen")] [Space] [SerializeField]
        private GameObject levels;

        [SerializeField] private GameObject selectLevelScreen;
        [SerializeField] private GameObject createLevelScreen;

        private LevelBaseInfo levelInfo;
        private string songName;
        private string tempFolderPath;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _createButton.interactable = false;
            _name.onValueChanged.AddListener(_ => CheckFields());
            _bpm.onValueChanged.AddListener(_ => CheckFields());
        }
        
        public void CreateTempFolder()
        {
            string levelsPath = Path.Combine(Application.persistentDataPath, "Levels");
            Directory.CreateDirectory(levelsPath);

            TempFolder();
        }

        private void CheckFields()
        {
            _createButton.interactable = !string.IsNullOrEmpty(_name.text) &&
                                         !string.IsNullOrEmpty(_bpm.text) &&
                                         !string.IsNullOrEmpty(songName);
        }

        private void TempFolder()
        {
            Guid myuuid = Guid.NewGuid();
            tempFolderPath = myuuid.ToString();
            Directory.CreateDirectory($"{Application.persistentDataPath}/Levels/{tempFolderPath}");
        }

        public void SelectMusic()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Sound Files", "mp3", "wav")
            };

            var path = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

            if (path.Length == 0) return;

            var musicPath = path[0];

            songName = Path.GetFileName(musicPath);

            _textOnButtonLoadSong.text = songName;
            File.Copy(musicPath,
                $"{Application.persistentDataPath}/Levels/{tempFolderPath}/{Path.GetFileName(musicPath)}",
                overwrite: true);
            
            CheckFields();
        }

        public void Create()
        {
            Directory.Move($"{Application.persistentDataPath}/Levels/{tempFolderPath}",
                $"{Application.persistentDataPath}/Levels/{_name.text}");
            LevelBaseInfo levelInfo = new LevelBaseInfo(
                _name.text, 
                songName,
                float.Parse(_bpm.text, NumberStyles.Float, CultureInfo.InvariantCulture), 
                0);
            string info = JsonUtility.ToJson(levelInfo, true);
            File.WriteAllText($"{Application.persistentDataPath}/Levels/{_name.text}/LevelBaseInfo.json", info);
            _gameEventBus.Raise(new OpenEditorEvent(levelInfo));
            levels.gameObject.SetActive(false);
        }

        public void Cancel()
        {
            string folderPath = $"{Application.persistentDataPath}/Levels/{tempFolderPath}";
    
            // Проверяем, существует ли папка
            if (Directory.Exists(folderPath))
            {
                // Удаляем все файлы в папке
                string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
        
                // Удаляем все подпапки и их содержимое
                string[] directories = Directory.GetDirectories(folderPath);
                foreach (string directory in directories)
                {
                    Directory.Delete(directory, true);
                }
        
                // Удаляем саму папку
                Directory.Delete(folderPath);
            }
    
            createLevelScreen.gameObject.SetActive(false);
            selectLevelScreen.gameObject.SetActive(true);
        }
    }
}

[Serializable]
public class LevelBaseInfo
{
    public LevelBaseInfo(string levelName, string songName, float bpm, float offset)
    {
        this.levelName = levelName;
        this.songName = songName;
        this.bpm = bpm;
        this.offset = offset;
    }

    public string levelName;
    public string songName;
    public float bpm;
    public float offset;
}