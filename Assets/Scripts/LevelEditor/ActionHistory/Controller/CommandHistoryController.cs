using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory
{
    /// <summary>
    /// Контроллер для обработки ввода пользователя и управления историей команд
    /// </summary>
    public class CommandHistoryController : MonoBehaviour
    {
        private void Update()
        {
            // Обработка горячей клавиши для отмены (Undo) - F2
            if (UnityEngine.Input.GetKeyDown(KeyCode.F2))
            {
                // Временное отключение записи команд в историю
                CommandHistory.IsRecording = false;

                // Выполнение операции отмены
                CommandHistory.Undo();

                // Восстановление записи команд
                CommandHistory.IsRecording = true;
            }

            // Обработка горячей клавиши для повтора (Redo) - F3
            if (UnityEngine.Input.GetKeyDown(KeyCode.F3))
            {
                // Временное отключение записи команд в историю
                CommandHistory.IsRecording = false;

                // Выполнение операции повтора
                CommandHistory.Redo();

                // Восстановление записи команд
                CommandHistory.IsRecording = true;
            }
        }
    }
}