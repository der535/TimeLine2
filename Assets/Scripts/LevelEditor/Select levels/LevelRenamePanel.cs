using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TimeLine
{
    public class LevelRenamePanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_InputField inputField;
        [Space]
        [SerializeField] private Button ok;
        [SerializeField] private Button cancel;

        private Action _onRename;
        private string _originalName;

        private void Start()
        {
            ok.onClick.AddListener(Rename);
            cancel.onClick.AddListener(() => panel.SetActive(false));
        }
        
        internal void Open(string newName, Action onRename = null)
        {
            _onRename = onRename;
            inputField.text = newName;
            _originalName = newName;
            panel.SetActive(true);
        }

        private void Rename()
        {
            if (inputField.text == _originalName || Directory.Exists($"{Application.persistentDataPath}/Levels/{inputField.text}")) return;
            LevelActions.RenameLevel(_originalName, inputField.text);
            panel.SetActive(false);
            _onRename.Invoke();
        }
    }
}