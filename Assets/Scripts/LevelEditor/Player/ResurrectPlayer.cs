using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Player
{
    public class ResurrectPlayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer; // Спрайт игрока
        [SerializeField] private GameObject player; // Объект игрока

        private GameEventBus _gameEventBus;
        private ActionMap _actionMap; // Input System
        private Main _main; // Главный контроллер (не используется)

        // Внедрение зависимостей
        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap, Main main)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _main = main; // ВНЕДРЕН, НО НЕ ИСПОЛЬЗУЕТСЯ
        }

        private void Start()
        {
            // Подписка на события рестарта и перехода в игровой режим
            _gameEventBus.SubscribeTo((ref RestartGameEvent _) => Resurrect());
            _gameEventBus.SubscribeTo((ref TurnToPlayModeEvent _) => Resurrect());
            // НЕТ ОТПИСКИ ОТ СОБЫТИЙ!
        }

        private void Resurrect()
        {
            // Сброс позиции на начало
            player.transform.position = new Vector3(0, 0, 0);
            spriteRenderer.enabled = true; // Показываем спрайт
            _actionMap.Player.Enable(); // Включаем управление
        }
    }
}