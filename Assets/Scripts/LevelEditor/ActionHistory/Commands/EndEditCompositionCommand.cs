namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class EndEditCompositionCommand : ICommand
    {
        private string _groupIP;
        
        private readonly string _description;
        
        private CompositionEdit _composition;
        private SaveComposition _save;
        

        public EndEditCompositionCommand(CompositionEdit compositionEdit, SaveComposition _saveComposition, string editingCompositionID, string description)
        {
            _composition = compositionEdit;
            _description = description;
            _groupIP = editingCompositionID;
            _save = _saveComposition;
        }

        public string Description() => _description;

        public void Execute()
        {
            _composition.EndEdit();
        }

        public void Undo()
        {
            _composition.Edit(_save.FindCompositionDataById(_groupIP));
        }
    }
}