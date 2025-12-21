using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.LevelEditor.Tabs.SettingTab.Current_time_type;
using TMPro;
using UnityEngine;
using Zenject;

public class CurrentTimeDisplay : MonoBehaviour
{
    [SerializeField] private SettingDisplayCurrentTime settingDisplayCurrentTime;
    
    [SerializeField] private TMP_InputField inputField;
    
    private GameEventBus _gameEventBus;
    private Main _main;

    [Inject]
    void Construct(GameEventBus gameEventBus, Main main)
    {
        _gameEventBus = gameEventBus;
    }

    public void Awake()
    {
        _gameEventBus.SubscribeTo<TickExactTimeEvent>(UpdateText);
    }

    public void UpdateText(ref TickExactTimeEvent timeEvent)
    {
        double currentTimeInTicks = timeEvent.Time;
        
        inputField.text = settingDisplayCurrentTime.ConvertTicksToFormat(currentTimeInTicks);
    }
}