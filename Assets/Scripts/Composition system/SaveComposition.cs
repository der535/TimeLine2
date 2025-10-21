using System;
using System.Collections.Generic;
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
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [Space] 
        [SerializeField] private CompositionCard compositionCard;
        [SerializeField] private RectTransform cardContainer;
        [Space]
        [SerializeField] private RenameComposition renameComposition;

        private readonly List<GroupGameObjectSaveData> _groupObjects = new();
        private readonly List<CompositionCard> _cards = new();

        private List<CompositionData> _compositionData = new();

        public void AddComposition(TrackObjectGroup group)
        {
            if(HasCompositionWithId(group.compositionID)) return;
            
            var data = saveLevel.SaveGroup(group);
            _groupObjects.Add(data);
            _compositionData.Add(new CompositionData()
            {
                compositionID = group.compositionID,
                groupObjects = data
            });
            CompositionCard card = Instantiate(compositionCard, cardContainer);

            card.Setup(() => spawner.LoadGroup(data, group.compositionID), null, () =>
            {
                renameComposition.RenameCompositionPanel.gameObject.SetActive(true);
                renameComposition.Setup(group.compositionID);
            }, null, data.gameObjectName);
            _cards.Add(card);
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
        
        public CompositionData FindCompositionDataById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            return _compositionData.FirstOrDefault(data => data.compositionID == id);
        }

        internal void Rename(string changedName, string compositionID)
        {
            CompositionData data = FindCompositionDataById(compositionID);
            print($"Rename {data.groupObjects.gameObjectName} --> {changedName}");
            data.groupObjects.gameObjectName = changedName;
        }
    }

    [Serializable]
    public class CompositionData
    {
        public string compositionID;
        public GroupGameObjectSaveData groupObjects;
    }
}