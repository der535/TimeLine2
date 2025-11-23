using System;
using UnityEngine;

namespace TimeLine
{
    public class KeyframeTypeActive : MonoBehaviour
    {
        [SerializeField] private KeyfeameVizualizer keyfeameVizualizer;
        [SerializeField] private BezierController bezierController;
        
        private bool _isBezier = false;

        public bool IsBezier() => _isBezier;
        
        private void Start()
        {
            SetType(false);
        }

        public void SetType(bool isBezier)
        {
            _isBezier = isBezier;
            keyfeameVizualizer.ActiveKeyframes(!isBezier);
            bezierController.ActiveKeyframes(isBezier);
        } 
    }
}
