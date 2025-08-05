using System.Globalization;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    public class TimeMarker : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        
        public void Set(float t)
        {
            text.text = t.ToString(CultureInfo.InvariantCulture);
        }
    }
}
