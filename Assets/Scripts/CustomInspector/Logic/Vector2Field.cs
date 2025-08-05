using System;
using UnityEngine;

namespace TimeLine
{
    [System.Serializable]
    public class Vector2Field : IField<Vector2>
    {
        [SerializeField] private string name;
        [SerializeField] private Vector2 value;
        
        public Vector2Field(string name, Vector2 value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name => name;
        public Vector2 Value
        {
            get => value;
            set => this.value = value;
        }
    
        public object ValueAsObject
        {
            get => Value;
            set => Value = (Vector2)value;
        }
    }
}