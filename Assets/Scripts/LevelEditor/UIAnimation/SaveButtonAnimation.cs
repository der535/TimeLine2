using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine.LevelEditor.UIAnimation
{
    public class SaveButtonAnimation : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;
        [Space]
        [SerializeField] private Color startColor;
        [SerializeField] private string startText;
        [Space]
        [SerializeField] private Color savingColor;
        [SerializeField] private string savingText;
        [Space]
        [SerializeField] private Color savedColor;
        [SerializeField] private string savedText;
        
        private void Start()
        {
            image.color = startColor;
            textMeshProUGUI.text = startText;
        }

        internal void Saving()
        {
            image.color = savingColor;
            textMeshProUGUI.text = savingText;
        }
        
        internal void Saved()
        {
            image.color = savedColor;
            textMeshProUGUI.text = savedText;
            DOVirtual.DelayedCall(2, Start);
        }
    }
}