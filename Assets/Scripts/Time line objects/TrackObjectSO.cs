using System;
using UnityEngine;

namespace TimeLine
{
    [CreateAssetMenu(fileName = "Default", menuName = "ScriptableObjects/TrackObjec")]
    public class TrackObjectSO : ScriptableObject
    {
        public Sprite sprite;
        public string name;
        public float startLiveTime;

        private void OnValidate()
        {
            name = sprite.name;
        }
    }
}
