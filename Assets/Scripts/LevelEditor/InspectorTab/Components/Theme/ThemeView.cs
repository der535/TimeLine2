using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class ThemeView : MonoBehaviour
    {
        public List<Image> primaryColor;
        public List<Image> secondaryColor;
        public List<Image> icons;
        public List<Image> currentTimeMarker;
        public List<Image> tabs;
        public List<M_InputFieldPaint> inputFields;
        public List<M_ButtonsPaint> buttons;
        public List<M_DropDownPaint> dropDowns;
        public List<M_SliderPaint> sliderPaints;
        public List<TextMeshProUGUI> texts;
        [Space]
        public Image iconPlay;
        public SpriteRenderer sceneBackground;
        public GridGenerator grid;
    }
}
