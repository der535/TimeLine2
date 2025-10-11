using System;
using System.Globalization;
using System.IO;
using EventBus;
using NaughtyAttributes;
using SFB;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CreateLevel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _name;
        [SerializeField] private TMP_InputField _bpm;
        [Space] 
        [SerializeField] private GameObject createScreen;
        
        private LevelBaseInfo levelInfo;
        private string songName;
        private string tempFolderPath;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        public void CreateTempFolder()
        {
            string levelsPath = Path.Combine(Application.persistentDataPath, "Levels");
            Directory.CreateDirectory(levelsPath);

            TempFolder();
        }

        private void TempFolder()
        {
            Guid myuuid = Guid.NewGuid();
            tempFolderPath = myuuid.ToString();
            Directory.CreateDirectory($"{Application.persistentDataPath}/Levels/{tempFolderPath}");
        }

        public void SelectMusic()
        {
            var extensions = new [] {
                new ExtensionFilter("Sound Files", "mp3", "wav" )
            };

            var path = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false)[0];
            songName = Path.GetFileName(path);
            File.Copy(path, $"{Application.persistentDataPath}/Levels/{tempFolderPath}/{Path.GetFileName(path)}", overwrite: true);
        }

        public void Create()
        {
            Directory.Move($"{Application.persistentDataPath}/Levels/{tempFolderPath}", $"{Application.persistentDataPath}/Levels/{_name.text}");
            LevelBaseInfo levelInfo = new LevelBaseInfo(_name.text, songName, float.Parse(_bpm.text, NumberStyles.Float, CultureInfo.InvariantCulture));
            string info = JsonUtility.ToJson(levelInfo, true);
            File.WriteAllText($"{Application.persistentDataPath}/Levels/{_name.text}/LevelBaseInfo.json", info);
            _gameEventBus.Raise(new OpenEditorEvent(levelInfo));
            createScreen.gameObject.SetActive(false);
        }
    }
}

[Serializable]
public class LevelBaseInfo
{
    public LevelBaseInfo(string levelName, string songName, float bpm)
    {
        this.levelName = levelName;
        this.songName = songName;
        this.bpm = bpm;
    }
    
    public string levelName;
    public string songName;
    public float bpm;
}
