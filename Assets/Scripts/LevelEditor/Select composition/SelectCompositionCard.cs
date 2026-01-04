using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class SelectCompositionCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button button;

        private GroupGameObjectSaveData _data;

        internal void Setup(GroupGameObjectSaveData data, Action select)
        {
            _data = data;
            this.text.text = _data.gameObjectName;
            button.onClick.AddListener(select.Invoke);
        }
    }
}