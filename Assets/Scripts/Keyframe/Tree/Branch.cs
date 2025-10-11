using System.Collections.Generic;
using System.Text;
using TimeLine;
using UnityEngine;

[System.Serializable]
public class Branch
{
    public string Name { get; private set; }
    public string ID { get; }
    public TreeNode Root { get; set; }
    public List<TreeNode> Nodes { get; } = new();
    
    public Branch(string id, string name)
    {
        Name = name;
        ID = id;
        Root = new TreeNode(name, name);
        Nodes.Add(Root);
    }
    
    public Branch(Branch original, string id)
    {
        if (original == null) return;

        // Копируем примитивные свойства
        Name = original.Name;
        ID = id;

        // Словарь для связи оригиналов и копий
        var mapping = new Dictionary<TreeNode, TreeNode>();
        
        // Глубокое копирование дерева
        Root = original.Root?.DeepCopy(mapping);
        
        // Восстанавливаем список Nodes
        Nodes = new List<TreeNode>(original.Nodes.Count);
        foreach (var node in original.Nodes)
        {
            if (mapping.TryGetValue(node, out var copiedNode))
            {
                Nodes.Add(copiedNode);
            }
        }
    }

    internal void Rename(string name)
    {
        Name = name;
    }
    
    public void PrintTree()
    {
        if (Root == null)
        {
            Debug.Log("(empty tree)");
            return;
        }

        var sb = new StringBuilder();
        BuildTreeString(sb, Root, new List<bool>());
        Debug.Log(sb.ToString());
    }

    private void BuildTreeString(StringBuilder sb, TreeNode node, List<bool> parentLastFlags)
    {
        // Добавляем префиксы для текущего узла
        for (int i = 0; i < parentLastFlags.Count - 1; i++)
        {
            sb.Append(parentLastFlags[i] ? "    " : "│   ");
        }
    
        if (parentLastFlags.Count > 0)
        {
            sb.Append(parentLastFlags[^1] ? "└── " : "├── ");
        }
    
        sb.AppendLine(node.Name);

        // Обработка дочерних элементов
        int childCount = node.Children.Count;
        for (int i = 0; i < childCount; i++)
        {
            var flags = new List<bool>(parentLastFlags) { i == childCount - 1 };
            BuildTreeString(sb, node.Children[i], flags);
        }
    }
    
    public TreeNode FindNode(string path)
    {
        if (string.IsNullOrEmpty(path))
            return Root;

        string[] parts = path.Split('/');
        TreeNode currentNode = Root;

        foreach (string part in parts)
        {
            bool found = false;
            foreach (TreeNode child in currentNode.Children)
            {
                if (child.Name == part)
                {
                    currentNode = child;
                    found = true;
                    break;
                }
            }

            if (!found)
                return null;
        }

        return currentNode;
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
                //todo Возможно надо пофиксить но учитывается что больше одного недостающего элемента не будет
                var newNode = currentNode.AddChild(parts[i], parts[i]);
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
        
        Debug.Log($"{path}/{nodeName}");
        // Добавление конечного узла
        var finalNode = currentNode.AddChild(nodeName, $"{path}/{nodeName}");
        Nodes.Add(finalNode);
        return finalNode;
    }
    
    public BranchSaveData ToSaveData()
    {
        var saveData = new BranchSaveData
        {
            ID = ID,
            Name = Name
        };

        foreach (var node in Nodes)
        {
            saveData.Nodes.Add(new TreeNodeSaveData
            {
                Path = node.Path,
                Name = node.Name
            });
        }

        return saveData;
    }

// Вспомогательный метод: извлекает путь родителя из полного пути
    private static string GetParentPathFromPath(string fullPath)
    {
        int lastSlash = fullPath.LastIndexOf('/');
        return lastSlash > 0 ? fullPath.Substring(0, lastSlash) : "";
    }
}