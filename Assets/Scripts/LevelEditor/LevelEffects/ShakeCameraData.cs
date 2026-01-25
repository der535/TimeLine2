using DG.Tweening;
using UnityEngine;

namespace TimeLine
{
    public class ShakeCameraData
    {
        public Tween ShakeTween;
        public Vector3 OriginalPos;
        public Vector3 CurrentOffset;

        internal void SaveStartPosition(Transform cameraTransform)
        {
            OriginalPos = cameraTransform.position;
            OriginalPos.z = -10;
        }
    }
}