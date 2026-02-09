using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TimeLine
{
    [CreateAssetMenu(fileName = "Default", menuName = "ScriptableObjects/EditorTheme", order = 1)]
    public class ThemeSO : ScriptableObject
    {
        public Color primary = new(0.2431373f, 0.2352941f, 0.2352941f, 1);

        public Color secondary = new(0.1647059f, 0.1647059f, 0.1647059f, 1);
        
        [Space] 
        public Color inputFieldBackground = Color.white;
        public Color inputFieldText = Color.white;
        public Color inputFieldTextPlaceholder = Color.white;
        [Space] 
        public Color currentTimeLine = Color.red;
        [Space] 
        public Color icons = Color.white;
        [Space] 
        public Color keyframeColor = Color.white;
        public Color selectedKeyframeColor = Color.yellow;
        [Space] 
        public Color bacroundButtons = Color.white;
        public Color textButtons = Color.black;
        [Space] 
        public Color iconPlay = Color.white;
        [Space] 
        public Color backgroundSceneColor = new(0.8f, 0.8f, 0.8f, 1f);
        public Color gridSceneColor = new(0.8f, 0.8f, 0.8f, 1f);
        [Space]
        public Color textColor = Color.white;
        [Space]
        public Color sliderBacround = Color.gray;
        public Color sliderHandle = Color.white;
        [Space]
        public Color tabs = Color.black;
        [Space]
        public Color circleBeatActive = Color.black;
        public Color circleBeatInActive = new(0.2431373f, 0.2352941f, 0.2352941f, 1);
        [Space]
        public Color dropDownBackground = Color.white;
        public Color dropDownText = Color.black;
        public Color dropDownArrow = Color.black;
        public Color dropDownItemBackground = Color.white;
        public Color dropDownItemCheckmark = Color.black;
        public Color dropDownItemLabel = Color.gray;
        [Space]
        public Color timeMarkerPrimary = Color.white;
        public Color timeMarkerSecond = Color.gray;
        public Color timeMarkerText = Color.white;
        [Space]
        public Color waveForm = new(1, 1, 1, 0.15f);
    }
}