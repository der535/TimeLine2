using EventBus;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class GridController : MonoBehaviour
    {
        [FormerlySerializedAs("_grid")]
        [SerializeField] private GameObject grid;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus eventBus)
        {
            _gameEventBus = eventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref TurnToPlayModeEvent _) => { grid.SetActive(false); });
            _gameEventBus.SubscribeTo((ref ExitPlayEvent _) => { grid.SetActive(true); });
        }
    }
}