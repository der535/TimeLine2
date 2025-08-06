using System;
using System.Collections.Generic;
using TimeLine.Keyframe;
using UnityEngine;

namespace TimeLine
{
    public class TintComponent : MonoBehaviour, IFieldProvider
    {
        [SerializeField] private IntField _field1 = new("Field1", 0);
        [SerializeField] private Vector2Field _field2 = new("Field2", new Vector2(0,0));

        public Action OnValueChanged { get; set; }
        public IEnumerable<IField> GetFields()
        {
            yield return _field1;
            yield return _field2;
        }
        
        public int Tint 
        { 
            get => _field1.Value; 
            set => _field1.Value = value; 
        }

        private void Start()
        {
            TintIntData t = new TintIntData(_field1.Value);
            
            
            OnValueChanged = () => transform.position = _field2.Value;
        }

        private void Update()
        {
            // Пример использования полей
            // Debug.Log($"Field1: {_field1.Value}, Field2: {_field2.Value}");
        }
    }
}