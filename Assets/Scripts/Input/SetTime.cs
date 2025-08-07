using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine.Input
{
    public class SetTime : MonoBehaviour
    {
        [SerializeField] private TimeLineSettings settings;
        [Space]
        [SerializeField] private TMP_InputField inputField;
    
        private Main _main;

        [Inject]
        private void Construct(Main main)
        {
            _main = main;
        }

        private void Start()
        {
            inputField.onEndEdit.AddListener(time => _main.SetTime(float.Parse(time)));
        }
    }
}
