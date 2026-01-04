using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TimeLine
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
        
        private FieldLineData _fieldLineData;
        public FieldLineData FieldLineData => _fieldLineData;

        public void Setup(string str, float height, int level, TreeNode treeNode, Sprite sprite = null)
        {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            textMeshProName.text = str;
            icon.sprite = sprite;
            rectLevel.offsetMin = new Vector2(levelSpace * level, rectLevel.offsetMin.y);

            _fieldLineData = new FieldLineData(rect, treeNode, this, selectFieldLine);
        }
    }
}
