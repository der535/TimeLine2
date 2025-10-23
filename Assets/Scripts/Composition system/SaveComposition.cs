using System;
using System.Collections.Generic;
using System.IO;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

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

        [Button]
        public void Save()
        {
            string path = $"{Application.persistentDataPath}/Levels/{saveLevel.LevelBaseInfo.levelName}/Compositions.json";
            string json = JsonConvert.SerializeObject(_compositionData, Formatting.Indented);
            File.WriteAllText(path, json);
            print(json);
        }
        [Button]

        public void Load()
        {
            string path = $"{Application.persistentDataPath}/Levels/{saveLevel.LevelBaseInfo.levelName}/Compositions.json";
            string json = File.ReadAllText(path);
            _compositionData = JsonConvert.DeserializeObject<List<GroupGameObjectSaveData>>(json);
            foreach (var data in _compositionData)
            {
                CreateCard(data);
            }
            print(_compositionData.Count);
        }
        public void AddComposition(TrackObjectGroup group)
        {
            if(HasCompositionWithId(group.compositionID)) return;
            
            var data = saveLevel.SaveGroup(group);
            data.compositionID = group.compositionID;
            _compositionData.Add(data);

            CreateCard(data);
        }

        private void CreateCard(GroupGameObjectSaveData data)
        {
            CompositionCard card = Instantiate(compositionCard, cardContainer);

            card.Setup(this,() =>
                {
                    spawner.LoadGroup(FindCompositionDataById(data.compositionID), data.compositionID);
                }, ()=>
                {
                    compositionEdit.Edit(FindCompositionDataById(data.compositionID));
                }, () =>
                {
                    renameComposition.RenameCompositionPanel.gameObject.SetActive(true);
                    renameComposition.Setup(data.compositionID);
                }, () =>
                {
                    DeleteComposition(FindCompositionDataById(data.compositionID));
                }, 
                () =>
                {
                    DuplicateComposition(FindCompositionDataById(data.compositionID));
                },data.compositionID);
            
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
            if(HasCompositionWithId(group.compositionID)) return;
            
            _compositionData.Add(group);
            CompositionCard card = Instantiate(compositionCard, cardContainer);
            
            card.Setup(this,() =>
                {
                    spawner.LoadGroup(FindCompositionDataById(group.compositionID), group.compositionID);
                }, ()=>
                {
                    compositionEdit.Edit(FindCompositionDataById(group.compositionID));
                }, () =>
                {
                    renameComposition.RenameCompositionPanel.gameObject.SetActive(true);
                    renameComposition.Setup(group.compositionID);
                }, null, 
                () =>
                {
                    DuplicateComposition(FindCompositionDataById(group.compositionID));
                },group.compositionID);
            
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
            print($"Search: {id}");

            foreach (var data in _compositionData)
            {
                print(data.compositionID);
            }
            
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