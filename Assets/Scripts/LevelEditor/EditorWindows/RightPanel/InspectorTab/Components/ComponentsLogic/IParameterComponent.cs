using System.Collections.Generic;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.ComponentsLogic
{
    public interface IParameterComponent
    {
        // Теперь возвращает Dictionary<string, ParameterDataPacket>
        Dictionary<string, ParameterPacket> GetParameterData();
        void SetParameterData(Dictionary<string, ParameterPacket> data);
        string GetComponentTypeName();
    }
}