using System;
using System.Collections.Generic;
using TimeLine.Keyframe;
using UnityEngine;

namespace TimeLine
{
    public class TintComponent : MonoBehaviour, IFieldProvider
    {
        [SerializeField] private IntField _field1 = new("Field1", 0);
        [SerializeField] private Vector2Field _field2 = new("Field2", Vector2.zero);

        public Action OnValueChanged { get; set; }
    
        public IEnumerable<IField> GetFields()
        {
            yield return _field1;
            yield return _field2;
        }

        private void Start()
        {
            OnValueChanged = () => transform.position = _field2.Value;
        }
    }
}
