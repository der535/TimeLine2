using System.Collections.Generic;
using UnityEngine;

namespace TimeLine.EdgeColliderEditor
{
    public interface IEdgeColliderModel
    {
        List<Vector2> Points { get; set; }
        float EdgeRadius { get; }
        void BeginUpdate();
        void EndUpdate();
    }
}