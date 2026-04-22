using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.line
{
    public class AnimationFieldLine : MonoBehaviour
    {
        [SerializeField] private Button expandButton;
        [SerializeField] private Image icon;
        [FormerlySerializedAs("name")] [SerializeField] private TextMeshProUGUI textMeshProName;
        [SerializeField] private RectTransform rect;
        [SerializeField] private RectTransform rectLevel;
        [SerializeField] private float levelSpace;
        [SerializeField] private SelectFieldLine selectFieldLine;

        public FieldLineData FieldLineData { get; private set; }

        public void Setup(string str, float height, int level, TreeNode treeNode, Sprite sprite = null)
        {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            textMeshProName.text = str;
            icon.sprite = sprite;
            rectLevel.offsetMin = new Vector2(levelSpace * level, rectLevel.offsetMin.y);

            FieldLineData = new FieldLineData(rect, treeNode, this, selectFieldLine);
        }
    }
}
