using System.Collections.Generic;
using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects
{
    public class TrackObjectVisual : MonoBehaviour
    {
        [SerializeField] private List<GameObject> trackTransform;
        internal void SetActive(bool active)
        {
            foreach (var obj in trackTransform)
            {
                obj.SetActive(active);
            }
        }
    }
}