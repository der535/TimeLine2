using System;
using UnityEngine;

namespace TimeLine
{
    public class KeyframeTypeActive : MonoBehaviour
    {
        [SerializeField] private KeyfeameVizualizer keyfeameVizualizer;
        [SerializeField] private BezierController bezierController;

        private void Start()
        {
            SetType(false);
        }

        public void SetType(bool isBezier)
        {
            keyfeameVizualizer.ActiveKeyframes(!isBezier);
            bezierController.ActiveKeyframes(isBezier);
        } 
    }
}
