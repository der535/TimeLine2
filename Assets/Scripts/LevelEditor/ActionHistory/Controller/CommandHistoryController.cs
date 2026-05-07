using System;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ActionHistory
{
    /// <summary>
    /// Контроллер для обработки ввода пользователя и управления историей команд
    /// </summary>
    public class CommandHistoryController : MonoBehaviour
    {
        ActionMap _actionMap;
        [Inject]
        private void Construct(ActionMap actionMap)
        {
            _actionMap = actionMap;
        }

        private void Start()
        {
            _actionMap.Editor.Z.started += context =>
            {
                if (_actionMap.Editor.LeftCtrl.IsPressed())
                {
                    // Временное отключение записи команд в историю
                    CommandHistory.IsRecording = false;

                    // Выполнение операции отмены
                    CommandHistory.Undo();

                    // Восстановление записи команд
                    CommandHistory.IsRecording = true;
                }
            };
            
            _actionMap.Editor.Y.started += context =>
            {
                if (_actionMap.Editor.LeftCtrl.IsPressed())
                {
                    // Временное отключение записи команд в историю
                    CommandHistory.IsRecording = false;

                    // Выполнение операции повтора
                    CommandHistory.Redo();

                    // Восстановление записи команд
                    CommandHistory.IsRecording = true;
                }
            };
        }
    }
}