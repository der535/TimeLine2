using System;
using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class GameEventBusSingletone : MonoBehaviour
    {
        public static GameEventBusSingletone Instance;
        public GameEventBus GameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            this.GameEventBus = gameEventBus;
        }
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
