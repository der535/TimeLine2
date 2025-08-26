using TMPro;
using UnityEngine;

namespace TimeLine
{
    public class ComponentUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform rootObject;

        public RectTransform RootObject => rootObject;
        
        public void SetName(string name)
        {
            text.text = name;
        }
    }
}
