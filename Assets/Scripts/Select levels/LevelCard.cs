using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class LevelCard : MonoBehaviour
    {
        [SerializeField] private Button compositionButton;
        [SerializeField] private Button editButton;
        [SerializeField] private Button renameButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private TextMeshProUGUI _text;

        internal void Setup(string text, Action createAction, Action editAction, Action renameAction, Action deleteAction)
        {
            compositionButton.onClick.AddListener(createAction.Invoke);
            if(editAction != null) editButton.onClick.AddListener(editAction.Invoke);
            if(renameAction != null) renameButton.onClick.AddListener(renameAction.Invoke);
            if(deleteAction != null) deleteButton.onClick.AddListener(deleteAction.Invoke);
            _text.text = text;
        }
    }
}
