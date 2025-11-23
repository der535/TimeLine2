using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class KeyframeTypeButtonsActive : MonoBehaviour
    {
        [SerializeField] private List<Button> buttons;
        
        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        

        private void Start()
        {
            foreach (var button in buttons)
            {
                button.enabled = false;
            }
            
            _gameEventBus.SubscribeTo((ref SelectObjectEvent ev) =>
            {
                foreach (var button in buttons)
                {
                    button.enabled = true;
                }
            });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent ev) =>
            {
                foreach (var button in buttons)
                {
                    button.enabled = false;
                }
            });
        }
    }
}
