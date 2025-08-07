using System;
using UnityEngine;

namespace TimeLine
{
    [System.Serializable]
    public class FloatField : IField<float>
    {
        [SerializeField] private string name;
        [SerializeField] private float value;

        public FloatField(string name, float value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name => name;
        public float Value
        {
            get => value;
            set => this.value = value;
        }
    
        public object ValueAsObject
        {
            get => Value;
            set => Value = (float)value;
        }
    }
}