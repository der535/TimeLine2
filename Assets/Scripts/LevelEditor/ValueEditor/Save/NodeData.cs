using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GraphSaveData
{
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
}

[System.Serializable]
public class ConnectionSaveEntry
{
    public string OutNodeId;
    public int OutIndex;
    public string InNodeId;
    public int InIndex;
}