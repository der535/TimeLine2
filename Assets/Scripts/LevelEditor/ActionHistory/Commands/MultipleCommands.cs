using System.Collections.Generic;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class MultipleCommands : ICommand
    {
        private readonly List<ICommand> _commands;

        private readonly string _description;

        public MultipleCommands(List<ICommand> commands, string description)
        {
            _commands = commands;
            _description = description;
        }

        public string Description() => _description;

        public void Execute()
        {
            foreach (var command in _commands) command.Execute();
        }

        public void Undo()
        {
            foreach (var command in _commands) command.Undo();
        }
    }
}