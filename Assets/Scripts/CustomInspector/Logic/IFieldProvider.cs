using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeLine
{
    public interface IFieldProvider
    {
        IEnumerable<IField> GetFields();
        public Action OnValueChanged { get; set; }
    }
}