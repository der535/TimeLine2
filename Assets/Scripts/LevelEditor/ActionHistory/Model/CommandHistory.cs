using System.Collections.Generic;

namespace TimeLine.LevelEditor.ActionHistory
{
    /// <summary>
    /// Клас хранящий историю действий
    /// </summary>
    public static class CommandHistory
    {
        // Стек команд для отмены (Undo)
        private static Stack<ICommand> _undoStack = new();

        // Стек команд для повтора (Redo)
        private static Stack<ICommand> _redoStack = new();

        // Флаг, указывающий, записываются ли команды в историю
        public static bool IsRecording = true;

        /// <summary>
        /// Выполняет команду и записывает её в историю, если запись включена
        /// </summary>
        /// <param name="command">Команда для выполнения</param>
        public static void AddCommand(ICommand command, bool execute)
        {
            // Выполняем команду
            if(execute) command.Execute();

            // Если запись в историю включена, сохраняем команду
            if (IsRecording)
            {
                _undoStack.Push(command);
                _redoStack.Clear(); // При новом действии история Redo очищается
            }
        }

        /// <summary>
        /// Отменяет последнюю выполненную команду
        /// </summary>
        public static void Undo()
        {
            if (_undoStack.Count > 0)
            {
                ICommand command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
            }
        }

        /// <summary>
        /// Повторяет последнюю отмененную команду
        /// </summary>
        public static void Redo()
        {
            if (_redoStack.Count > 0)
            {
                ICommand command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
            }
        }
    }
}