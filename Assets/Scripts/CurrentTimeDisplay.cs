using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TMPro;
using UnityEngine;
using Zenject;

public class CurrentTimeDisplay : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    private GameEventBus _gameEventBus;

    [Inject]
    void Construct(GameEventBus gameEventBus)
    {
        _gameEventBus = gameEventBus;
    }

    private void Awake()
    {
        _gameEventBus.SubscribeTo<BeatEvent>(UpdateText);
    }

    public void UpdateText(ref BeatEvent beatEvent)
    {
        inputField.text = beatEvent.Beat.ToString();
    }
}