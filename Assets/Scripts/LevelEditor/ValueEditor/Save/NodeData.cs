using System.Collections.Generic;
using TimeLine.LevelEditor.ValueEditor;
using UnityEngine;

[System.Serializable]
public class GraphSaveData
{
    public GraphSaveData(DataType outputType, List<NodeSaveEntry> nodes, List<ConnectionSaveEntry> connections )
    {
        OutputType = outputType;
        Nodes = nodes;
        Connections = connections;
    }
    
    public DataType OutputType;
    public List<NodeSaveEntry> Nodes = new();
    public List<ConnectionSaveEntry> Connections = new();
}

[System.Serializable]
public class NodeSaveEntry
{
    public string Id;
    public string TypeFullName; // Для рефлексии
    public Vector2 Position;
    public Dictionary<int, object> ManualValues; 
    public Dictionary<string, object> AdditionalData = new();
}

[System.Serializable]
public class ConnectionSaveEntry
{
    public string OutNodeId;
    public int OutIndex;
    public string InNodeId;
    public int InIndex;
}