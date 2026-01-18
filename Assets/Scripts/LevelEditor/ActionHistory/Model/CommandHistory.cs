using System.Collections.Generic;

namespace TimeLine.LevelEditor.ActionHistory
{
    public static class CommandHistory
    {
        private static Stack<ICommand> _undoStack = new();
        private static Stack<ICommand> _redoStack = new();

        public static bool isRecording = true;

        public static void ExecuteCommand(ICommand command)
        {
            command.Execute();
            if (isRecording)
            {
                _undoStack.Push(command);
                _redoStack.Clear(); // При новом действии история Redo очищается
            }
        }

        public static void Undo()
        {
            if (_undoStack.Count > 0)
            {
                ICommand command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
            }
        }

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