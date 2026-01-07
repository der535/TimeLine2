namespace TimeLine.LevelEditor.ActionHistory
{
    public interface ICommand
    {
        void Execute(); 
        void Undo();    
    }
}