using TMPro;
using UnityEngine;

namespace TimeLine
{
    public class DisplayGameVersion : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        void Start()
        {
            text.text = Application.version;
        }
    }
}
