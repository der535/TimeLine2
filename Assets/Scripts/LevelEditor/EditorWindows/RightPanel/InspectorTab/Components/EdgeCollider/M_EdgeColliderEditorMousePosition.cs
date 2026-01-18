using UnityEngine;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.EdgeCollider
{
    public class M_EdgeColliderEditorMousePosition
    {
        private SceneToRawImageConverter _sceneToRawImageConverter;
        internal M_EdgeColliderEditorMousePosition(SceneToRawImageConverter sceneToRawImageConverter)
        {
            _sceneToRawImageConverter = sceneToRawImageConverter;
        }
        
        internal Vector3 GetMouseWorldPosition()
        {
            return _sceneToRawImageConverter.ScreenPointToWorldScene(UnityEngine.Input.mousePosition);
        }
    }
}