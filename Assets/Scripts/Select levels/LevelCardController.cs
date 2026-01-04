using System;
using System.Collections.Generic;
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
        [SerializeField] private SaveLevel _saveLevel;
        [SerializeField] private LevelRenamePanel _levelRenamePanel;
        [SerializeField] private DeleteLevelPanel _deleteLevelPanel;
        [Space]
        [SerializeField] private GameObject canvasCreateLevel;
        
        private List<LevelCard> _levelCards = new List<LevelCard>();
        
        private GameEventBus _gameEventBus;
        private DiContainer _container;

        [Inject]
        private void Construct(GameEventBus eventBus, DiContainer container)
        {
            _gameEventBus = eventBus;
            _container = container;
        }

        private void Start()
        {
            UpdateCards();
        }

        public void UpdateCards()
        {
            string levelsPath = Path.Combine(Application.persistentDataPath, "Levels");

            foreach (var level in _levelCards)
            {
                Destroy(level.gameObject);
            }
            
            _levelCards.Clear();

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
                    // print(jsonContent);

                    LevelBaseInfo levelData = JsonUtility.FromJson<LevelBaseInfo>(jsonContent);
                    // print(levelData);

                    CreateLevelCard(levelData);
                    // print("CreateLevelCard");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Ошибка при загрузке уровня: {e}");
                }
            }
        }

        private void CreateLevelCard(LevelBaseInfo data)
        {
            LevelCard card = _container.InstantiatePrefab(levelCardPrefab, _content).GetComponent<LevelCard>();

            card.Setup(data.levelName, () =>
            {
                _gameEventBus.Raise(new OpenEditorEvent(data));
                _saveLevel.Load(data);
                canvasCreateLevel.gameObject.SetActive(false);
            }, () =>
            {
                LevelActions.Copy(data.levelName);
                UpdateCards();
            }, () =>
            {
                //renameAction
                _levelRenamePanel.Open(data.levelName, UpdateCards);
            }, () =>
            {
                //deleteAction
                _deleteLevelPanel.Open(data.levelName, UpdateCards);
            });
            
            _levelCards.Add(card);
        }
    }
}