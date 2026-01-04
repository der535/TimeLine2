using System;
using Newtonsoft.Json;
using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using UnityEngine;

namespace TimeLine
{
    public class SaveTest : MonoBehaviour
    {
        private void Start()
        {
            var data = new XOffsetData(123);
            JsonConvert.SerializeObject(data);
            print(JsonConvert.SerializeObject(data));
        }
    }
}
