using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    public class ComponentUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform rootObject;
        [Space]
        [SerializeField] private RectTransform componentTransform;

        [ShowNonSerializedField] private float _height;

        public RectTransform RootObject => rootObject;

        [Button]
        private void showValue()
        {
            print(componentTransform.sizeDelta.y);
        }

        private void Awake()
        {
            _height += text.rectTransform.sizeDelta.y;
        }
        
        public void SetName(string name)
        {
            text.text = name;
        }

        public void AddHeight(float height)
        {
            _height += height;
            componentTransform.sizeDelta = new Vector2(componentTransform.sizeDelta.x, _height); 
            print(_height);
        }
    }
}
