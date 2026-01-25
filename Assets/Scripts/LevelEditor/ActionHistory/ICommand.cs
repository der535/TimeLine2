namespace TimeLine.LevelEditor.ActionHistory
{
    /// <summary>
    /// Интерфейс команды
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Выполняет команду
        /// </summary>
        void Execute();

        /// <summary>
        /// Отменяет выполненную команду
        /// </summary>
        void Undo();
    }
}