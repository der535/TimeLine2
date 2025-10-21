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
            compositionButton.onClick.AddListener(editAction.Invoke);
            compositionButton.onClick.AddListener(renameAction.Invoke);
            compositionButton.onClick.AddListener(deleteAction.Invoke);
            _text.text = text;
        }
    }
}
