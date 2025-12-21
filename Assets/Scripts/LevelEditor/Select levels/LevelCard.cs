using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class LevelCard : MonoBehaviour
    {
        [SerializeField] private Button compositionButton;
        [SerializeField] private Button duplicateButton;
        [SerializeField] private Button renameButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private TextMeshProUGUI _text;

        internal void Setup(string text, Action createAction, Action duplicateAction, Action renameAction, Action deleteAction)
        {
            compositionButton.onClick.AddListener(createAction.Invoke);
            if(duplicateAction != null) duplicateButton.onClick.AddListener(duplicateAction.Invoke);
            if(renameAction != null) renameButton.onClick.AddListener(renameAction.Invoke);
            if(deleteAction != null) deleteButton.onClick.AddListener(deleteAction.Invoke);
            _text.text = text;
        }
    }
}
