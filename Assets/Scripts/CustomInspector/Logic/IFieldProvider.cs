using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeLine
{
    public interface IFieldProvider
    {
        Action OnChangeCustomInspector {get; set;}
        IEnumerable<IField> GetFields();
    }
}