using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory
{
    public class CommandHistoryController : MonoBehaviour
    {
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F2))
            {
                CommandHistory.isRecording = false;
                CommandHistory.Undo();
                CommandHistory.isRecording = true;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.F3))
            {
                CommandHistory.isRecording = false;
                CommandHistory.Redo();
                CommandHistory.isRecording = true;
            }
        }
    }
}