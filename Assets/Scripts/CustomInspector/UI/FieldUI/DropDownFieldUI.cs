using TimeLine.CustomInspector.UI.FieldUI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TimeLine
{
    public class DropDownFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private TextMeshProUGUI parameterName;
        [FormerlySerializedAs("inputField")] [SerializeField] private TMP_Dropdown dropdown;

        public TMP_Dropdown Setup(string name)
        {
            parameterName.text = name;
            print(dropdown);
            return dropdown;
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}