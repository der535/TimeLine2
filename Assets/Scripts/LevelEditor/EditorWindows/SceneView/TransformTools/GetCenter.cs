using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TimeLine
{
    public static class GetCenter
    {
        public static Vector2 GetSelectionCenter(List<Transform> selection)
        {
            Vector2 center = Vector2.zero;
            foreach (Transform pos in selection)
                center += (Vector2)pos.position;
            center /= selection.Count;
            return center;
        }

        public static Vector2 GetSelectionCenter(List<TransformComponent> selection)
        {
            Vector2 center = Vector2.zero;
            foreach (TransformComponent pos in selection)
                center += new Vector2(pos.XPosition.Value, pos.YPosition.Value);
            center /= selection.Count;
            return center;
        }
    }
}