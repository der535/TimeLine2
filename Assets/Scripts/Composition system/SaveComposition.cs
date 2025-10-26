using System;
using System.Collections.Generic;
using System.IO;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using EventBus;
using Zenject;

namespace TimeLine
{
    public class SaveComposition : MonoBehaviour
    {
        [SerializeField] private SaveLevel saveLevel;
        [SerializeField] private TrackObjectSpawner spawner;
        [SerializeField] private TrackObjectRemover remover;
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private CompositionEdit compositionEdit;
        [SerializeField] private CompositionUpdater compositionUpdater;
        [Space] 
        [SerializeField] private CompositionCard compositionCard;
        [SerializeField] private RectTransform cardContainer;
        [Space] 
        [SerializeField] private RenameComposition renameComposition;

        private List<GroupGameObjectSaveData> _compositionData = new();
        private readonly List<CompositionCard> _cards = new();

        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        [Button]
        public void Save()
        {
            string path =
                $"{Application.persistentDataPath}/Levels/{saveLevel.LevelBaseInfo.levelName}/Compositions.json";
            string json = JsonConvert.SerializeObject(_compositionData, Formatting.Indented);
            File.WriteAllText(path, json);
            print(json);
        }

        [Button]
        public void Load()
        {
            string path = $"{Application.persistentDataPath}/Levels/{saveLevel.LevelBaseInfo.levelName}/Compositions.json";

            if (!File.Exists(path))
                return;

            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
                return;

            var deserialized = JsonConvert.DeserializeObject<List<GroupGameObjectSaveData>>(json);
            if (deserialized == null)
                return;

            _compositionData = deserialized;
            foreach (var data in _compositionData)
            {
                if (data != null)
                    CreateCard(data);
            }
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref StartCompositionEdit _) =>
            {
                CheckCards(_.Group);
                LockCompositionCard();
            });
            _gameEventBus.SubscribeTo((ref EndCompositionEdit _) =>
            {
                UnlockCompositionCard();
            });
        }

        private void CheckCards(GroupGameObjectSaveData group)
        {
            foreach (var card in _cards)
            {
                GroupGameObjectSaveData cardData = FindCompositionDataById(card.GetID());
                if (cardData.compositionID == group.compositionID || CheckGroup(cardData.children, group.compositionID))
                {
                    card.LockSpawn();
                }
            }
        }

        private bool CheckGroup(List<GameObjectSaveData> gameObjectSaveData, string id)
        {
            foreach (var obj in gameObjectSaveData)
            {
                if (obj is GroupGameObjectSaveData group)
                {
                    if (id == group.compositionID)
                    {
                        return true;
                    }
                    else
                    {
                        return CheckGroup(group.children, id);   
                    }
                }
            }
            return false;
        }

        public void LockCompositionCard()
        {
            foreach (var card in _cards)
            {
                card.LockEditButton();
            }
        }

        public void UnlockCompositionCard()
        {
            foreach (var card in _cards)
            {
                card.UnlockEditButton();
                card.UnlockSpawn();
            }
        }

        public void AddComposition(TrackObjectGroup group)
        {
            if (HasCompositionWithId(group.compositionID)) return;

            var data = saveLevel.SaveGroup(group);
            print(JsonConvert.SerializeObject(data, Formatting.Indented));
            data.compositionID = group.compositionID;
            _compositionData.Add(data);

            CreateCard(data);
        }

        private void CreateCard(GroupGameObjectSaveData data)
        {
            CompositionCard card = Instantiate(compositionCard, cardContainer);

            card.Setup(this,
                () => { spawner.LoadGroupNew(FindCompositionDataById(data.compositionID), data.compositionID); },
                () => { compositionEdit.Edit(FindCompositionDataById(data.compositionID)); }, () =>
                {
                    renameComposition.RenameCompositionPanel.gameObject.SetActive(true);
                    renameComposition.Setup(data.compositionID);
                }, () => { DeleteComposition(FindCompositionDataById(data.compositionID)); },
                () => { DuplicateComposition(FindCompositionDataById(data.compositionID)); }, data.compositionID);

            _cards.Add(card);
        }

        public void DeleteComposition(GroupGameObjectSaveData group)
        {
            _compositionData.Remove(group);
            foreach (var card in _cards.ToList())
            {
                if (card.GetID() == group.compositionID)
                {
                    _cards.Remove(card);
                    Destroy(card.gameObject);
                    break;
                }
            }

            foreach (var trackObjectGroups in trackObjectStorage.TrackObjectGroups.ToList())
            {
                if (trackObjectGroups.compositionID == group.compositionID)
                {
                    remover.SingleRemove(trackObjectGroups);
                }
            }

            compositionUpdater.UpdateCompositions();
        }

        public void AddComposition(GroupGameObjectSaveData group)
        {
            if (HasCompositionWithId(group.compositionID)) return;

            _compositionData.Add(group);
            CompositionCard card = Instantiate(compositionCard, cardContainer);

            card.Setup(this,
                () => { spawner.LoadGroupNew(FindCompositionDataById(group.compositionID), group.compositionID); },
                () => { compositionEdit.Edit(FindCompositionDataById(group.compositionID)); }, () =>
                {
                    renameComposition.RenameCompositionPanel.gameObject.SetActive(true);
                    renameComposition.Setup(group.compositionID);
                }, () => { DeleteComposition(FindCompositionDataById(group.compositionID)); },
                () => { DuplicateComposition(FindCompositionDataById(group.compositionID)); }, group.compositionID);

            _cards.Add(card);
        }

        internal void UpdateCompositionCards()
        {
            foreach (var card in _cards)
            {
                card.UpdateText();
            }
        }

        internal void DuplicateComposition(GroupGameObjectSaveData data)
        {
            var copy = data.DuplicateComposition();
            AddComposition(copy);
        }

        public void EditComposition(GroupGameObjectSaveData compositionData, string compositionID)
        {
            // print(compositionID);
            if (string.IsNullOrEmpty(compositionID))
                return;

            for (int i = 0; i < _compositionData.Count; i++)
            {
                if (_compositionData[i].compositionID == compositionID)
                {
                    // Обновляем только нужные поля, сохраняя ID
                    _compositionData[i] = compositionData;
                    _compositionData[i].compositionID = compositionID; // на случай, если он был перезаписан
                    return;
                }
            }

            // Опционально: если композиция не найдена, можно добавить логирование или выбросить исключение
            Debug.LogWarning($"Composition with ID '{compositionID}' not found for editing.");
        }


        public bool HasCompositionWithId(string id)
        {
            return !string.IsNullOrEmpty(id) && _compositionData.Any(data => data.compositionID == id);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        public GroupGameObjectSaveData FindCompositionDataById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            return _compositionData.FirstOrDefault(data => data.compositionID == id);
        }

        internal void Rename(string changedName, string compositionID)
        {
            GroupGameObjectSaveData data = FindCompositionDataById(compositionID);
            print($"Rename {data.gameObjectName} --> {changedName}");
            data.gameObjectName = changedName;
        }
    }
}