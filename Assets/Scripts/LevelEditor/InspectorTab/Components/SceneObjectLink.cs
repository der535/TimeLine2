using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components
{
    public class SceneObjectLink : MonoBehaviour
    {
        [FormerlySerializedAs("trackObjectData")] public TrackObjectPacket trackObjectPacket;
    }
}