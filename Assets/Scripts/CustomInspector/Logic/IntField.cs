using System;
using UnityEngine;

namespace TimeLine
{
    [System.Serializable]
    public class IntField : IField<int>
    {
        [SerializeField] private string name;
        [SerializeField] private int value;

        public IntField(string name, int value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name => name;
        public int Value
        {
            get => value;
            set => this.value = value;
        }
    
        public object ValueAsObject
        {
            get => Value;
            set => Value = (int)value;
        }
    }
}