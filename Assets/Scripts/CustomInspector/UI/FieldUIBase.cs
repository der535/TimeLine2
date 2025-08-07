using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public abstract class FieldUIBase<T> : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button keyframe;

        public virtual void Setup(IField<T> field, Action onChangeCustomInspector)
        {
            text.text = field.Name;
        }
    }
}