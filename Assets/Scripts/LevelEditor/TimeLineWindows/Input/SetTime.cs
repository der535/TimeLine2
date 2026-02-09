using TimeLine.LevelEditor.Tabs.SettingTab.Current_time_type;
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
        [SerializeField] private SettingDisplayCurrentTime settingDisplayCurrentTime;
    
        private Main _main;

        [Inject]
        private void Construct(Main main)
        {
            _main = main;
        }

        private void Start()
        {
            inputField.onEndEdit.AddListener(time =>
            {
                _main.SetTimeInTicks((float)settingDisplayCurrentTime.ConvertFromFormatToTicks(time), true);
            });
        }
    }
}
