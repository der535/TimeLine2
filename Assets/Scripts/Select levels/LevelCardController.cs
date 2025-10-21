using System;
using System.IO;
using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class LevelCardController : MonoBehaviour
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private LevelCard levelCardPrefab;
        [Space]
        [SerializeField] private GameObject canvasCreateLevel;
        
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus eventBus)
        {
            _gameEventBus = eventBus;
        }

        private void Start()
        {
            string levelsPath = Path.Combine(Application.persistentDataPath, "Levels");

            if (!Directory.Exists(levelsPath))
            {
                Debug.LogWarning($"Папка с уровнями не найдена: {levelsPath}");
                return;
            }

            string[] levelFolders = Directory.GetDirectories(levelsPath);

            foreach (string folder in levelFolders)
            {
                try
                {
                    string jsonContent = File.ReadAllText($"{folder}/LevelBaseInfo.json");

                    LevelBaseInfo levelData = JsonUtility.FromJson<LevelBaseInfo>(jsonContent);

                    CreateLevelCard(levelData);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Ошибка при загрузке уровня: {e}");
                }
            }
        }

        private void CreateLevelCard(LevelBaseInfo data)
        {
            LevelCard card = Instantiate(levelCardPrefab, _content);
            card.Setup(data.levelName, () =>
            {
                _gameEventBus.Raise(new OpenEditorEvent(data));
                canvasCreateLevel.gameObject.SetActive(false);
            }, null, null, null);
        }
    }
}