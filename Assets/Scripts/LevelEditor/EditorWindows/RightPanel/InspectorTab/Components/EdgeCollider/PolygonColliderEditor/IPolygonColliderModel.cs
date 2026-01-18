using System.Collections.Generic;
using UnityEngine;

namespace TimeLine.EdgeColliderEditor
{
    public interface IPolygonColliderModel
    {
        List<Vector2> Points { get; set; }
        void BeginUpdate();
        void EndUpdate();
    }
}