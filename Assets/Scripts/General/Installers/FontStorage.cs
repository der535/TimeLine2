using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TimeLine.General.Installers
{
    public enum FontNames
    {
        Standard,
        GameName,
        Arrows
    }
    public class FontStorage : MonoBehaviour
    {
     [SerializeField]   List<FontItem> fontItems = new();

        public TMP_FontAsset GetFont(FontNames fontName)
        {
            return fontItems.Find(x => x.FontName == fontName).Asset;
        }
    }

    [Serializable]
    public class FontItem
    {
        public FontNames FontName;
        public TMP_FontAsset Asset;
    }
}
