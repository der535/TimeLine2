using System.Collections.Generic;

[System.Serializable]
public class Branch
{
    public string Name { get; }
    public string ID { get; }
    public TreeNode Root { get; }
    public List<TreeNode> Nodes { get; } = new();
    
    public Branch(string id, string name)
    {
        Name = name;
        ID = id;
        Root = new TreeNode(name);
        Nodes.Add(Root);
    }

    public TreeNode AddNode(string path, string nodeName)
    {
        string[] parts = path.Split('/');
        TreeNode currentNode = Root;
        
        // Навигация по пути
        for (int i = 0; i < parts.Length; i++)
        {
            bool found = false;
            foreach (var child in currentNode.Children)
            {
                if (child.Name == parts[i])
                {
                    currentNode = child;
                    found = true;
                    break;
                }
            }
            
            // Создание отсутствующих узлов
            if (!found)
            {
                var newNode = currentNode.AddChild(parts[i]);
                Nodes.Add(newNode);
                currentNode = newNode;
            }
        }

        foreach (var c in currentNode.Children)
        {
            if (c.Name == nodeName)
            {
                return c;
            }
        }
        
        // Добавление конечного узла
        var finalNode = currentNode.AddChild(nodeName);
        Nodes.Add(finalNode);
        return finalNode;
    }
}