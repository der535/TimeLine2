using System;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class DeleteLevelPanel : MonoBehaviour
    {
        [SerializeField] private Button delete;
        [SerializeField] private Button cancel;
        [SerializeField] private GameObject panel;
        
        private Action _onDelete;
        private string _originalName;
        private void Start()
        {
            delete.onClick.AddListener(Delete);
            cancel.onClick.AddListener(() => panel.SetActive(false));
        }

        internal void Open(string levelName, Action onDelete)
        {
            _onDelete = onDelete;
            _originalName = levelName;
            panel.SetActive(true);
        }

        private void Delete()
        {
            LevelActions.DeleteLevel(_originalName);
            panel.SetActive(false);
            _onDelete.Invoke();
        }
    }
}