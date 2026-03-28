using System;
using EventBus;
using TimeLine.LevelEditor.Save;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class EditingCompositionController : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPlayMode;
        [SerializeField] private GameObject saveButton;
        [SerializeField] private TextMeshProUGUI textEditionComposition;
        [SerializeField] private GameObject endEditComposition;

        private GameEventBus _gameEventBus;
        public string EditionCompositionID;
        
        [Inject]
        private void Construct(GameEventBus eventBus)
        {
            _gameEventBus = eventBus;
        }
        
        private void Start()
        {
            _gameEventBus.SubscribeTo((ref StartCompositionEdit composition) => StartEdit(composition.Group));
            _gameEventBus.SubscribeTo((ref EndCompositionEdit _) => EndEdit());

            EndEdit();
        }

        private void StartEdit(GroupGameObjectSaveData compositionEdit)
        {
            EditionCompositionID = compositionEdit.compositionID;
            buttonPlayMode.gameObject.SetActive(false);
            saveButton.gameObject.SetActive(false);
            
            endEditComposition.SetActive(true);
            textEditionComposition.text = $"Edition: {compositionEdit.gameObjectName}";
            textEditionComposition.gameObject.SetActive(true);
        }

        private void EndEdit()
        {
            EditionCompositionID = String.Empty;
            buttonPlayMode.gameObject.SetActive(true);
            saveButton.gameObject.SetActive(true);
            
            endEditComposition.gameObject.SetActive(false);
            textEditionComposition.gameObject.SetActive(false);
        }
    }
}
