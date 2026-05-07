namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class StartEditCompositionCommand : ICommand
    {
        private string _groupIP;
        
        private readonly string _description;
        
        private CompositionEdit _composition;
        private SaveComposition _save;
        

        public StartEditCompositionCommand(CompositionEdit compositionEdit, SaveComposition _saveComposition, string editingCompositionID, string description)
        {
            _composition = compositionEdit;
            _description = description;
            _groupIP = editingCompositionID;
            _save = _saveComposition;
        }

        public string Description() => _description;

        public void Execute()
        {
            _composition.Edit(_save.FindCompositionDataById(_groupIP));
        }

        public void Undo()
        {
            _composition.EndEdit();
        }
    }
}