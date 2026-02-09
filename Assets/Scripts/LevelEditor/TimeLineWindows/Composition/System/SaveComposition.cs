using System;
using System.Collections.Generic;
using System.IO;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using EventBus;
using TimeLine.LevelEditor;
using TimeLine.LevelEditor.LevelJson;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using Zenject;

namespace TimeLine
{
    public class SaveComposition : MonoBehaviour
    {
        [SerializeField] private SaveLevel saveLevel;
        [SerializeField] private FacadeObjectSpawner spawner;
        [SerializeField] private TrackObjectRemover remover;
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private CompositionEdit compositionEdit;
        [SerializeField] private CompositionUpdater compositionUpdater;
        [Space] [SerializeField] private CompositionCard compositionCard;
        [SerializeField] private RectTransform cardContainer;
        [Space] [SerializeField] private RenameComposition renameComposition;

        private List<GroupGameObjectSaveData> _compositionData = new();
        private readonly List<CompositionCard> _cards = new();

        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        private SavePathController _savePathController;

        [Inject]
        private void Construct(GameEventBus gameEventBus, ActionMap actionMap, SavePathController savePathController)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _savePathController = savePathController;
        }

        internal List<GroupGameObjectSaveData> GetCompositionData() => _compositionData;

        [Button]
        public void Save()
        {
            string json = JsonConvert.SerializeObject(_compositionData, Formatting.Indented);
            File.WriteAllText(_savePathController.GetJsonPath(LevelJsonStorage.Compositions), json);
            print(json);
        }

        [Button]
        public void Load()
        {
            string path =
                _savePathController.GetJsonPath(LevelJsonStorage.Compositions);

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
            _gameEventBus.SubscribeTo((ref EndCompositionEdit _) => { UnlockCompositionCard(); });
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
            data.compositionID = group.compositionID;
            _compositionData.Add(data);

            CreateCard(data);
        }

        private void CreateCard(GroupGameObjectSaveData data)
        {
            CompositionCard card = Instantiate(compositionCard, cardContainer);

            card.Setup(this,
                () => { spawner.LoadComposition(FindCompositionDataById(data.compositionID), data.compositionID, true); },
                () => { compositionEdit.Edit(FindCompositionDataById(data.compositionID)); }, () =>
                {
                    renameComposition.RenameCompositionPanel.gameObject.SetActive(true);
                    renameComposition.Setup(data.compositionID, data.branch.Name);
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

            compositionUpdater.UpdateCompositions(group.compositionID);
        }

        public void AddComposition(GroupGameObjectSaveData group)
        {
            if (HasCompositionWithId(group.compositionID)) return;

            _compositionData.Add(group);
            CompositionCard card = Instantiate(compositionCard, cardContainer);

            card.Setup(this,
                () => { spawner.LoadComposition(FindCompositionDataById(group.compositionID), group.compositionID, true); },
                () => { compositionEdit.Edit(FindCompositionDataById(group.compositionID)); }, () =>
                {
                    renameComposition.RenameCompositionPanel.gameObject.SetActive(true);
                    renameComposition.Setup(group.compositionID, group.branch.Name);
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
            if (string.IsNullOrEmpty(compositionID))
                return;

            for (int i = 0; i < _compositionData.Count; i++)
            {
                if (_compositionData[i].compositionID == compositionID)
                {
                    var editedComposition = compositionData.DuplicateComposition();
                    var existing = _compositionData[i].DuplicateComposition();

                    print(JsonConvert.SerializeObject(editedComposition, Formatting.Indented));
                    
                    RemoveIDFromComposition(editedComposition);
                    RemoveIDFromComposition(existing);
                    
                    if (JsonConvert.SerializeObject(editedComposition, Formatting.Indented) !=
                        JsonConvert.SerializeObject(existing, Formatting.Indented))
                    {
                        print(false);
                        compositionData.lastEditID = Guid.NewGuid().ToString();
                        existing.lastEditID = compositionData.lastEditID;
                    }
                    else
                    {
                        print(true);
                        compositionData.lastEditID = _compositionData[i].lastEditID;
                        existing.lastEditID = _compositionData[i].lastEditID;
                    }

                    // Обновляем только нужные поля, сохраняя ID
                    _compositionData[i] = compositionData;
                    _compositionData[i].compositionID = compositionID; // на случай, если он был перезаписан
                    print(JsonConvert.SerializeObject( _compositionData[i], Formatting.Indented));
                    return;
                }
            }

            // Опционально: если композиция не найдена, можно добавить логирование или выбросить исключение
            Debug.LogWarning($"Composition with ID '{compositionID}' not found for editing.");
        }

        private void RemoveIDFromComposition(GroupGameObjectSaveData compositionData)
        {
            compositionData.compositionID = string.Empty;
            compositionData.sceneObjectID = string.Empty;
            compositionData.parentObjectID = string.Empty;
            compositionData.branch.ID = string.Empty;
            foreach (var child in compositionData.children)
            {
                RemoveIDFromTrackObject(child);
            }
        }

        private void RemoveIDFromTrackObject(GameObjectSaveData trackObject)
        {
            trackObject.parentObjectID = string.Empty;
            trackObject.branch.ID = string.Empty;
            trackObject.sceneObjectID = string.Empty;
        }


        public bool HasCompositionWithId(string id)
        {
            return !string.IsNullOrEmpty(id) && _compositionData.Any(data => data.compositionID == id);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5))
            {
                _actionMap.Dispose();
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
            data.branch.Name = changedName;
        }
    }
}