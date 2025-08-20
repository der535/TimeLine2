using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
                var newNode = currentNode.AddChild(parts[i], $"{path}/{nodeName}");
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
        var finalNode = currentNode.AddChild(nodeName, $"{path}/{nodeName}");
        Nodes.Add(finalNode);
        return finalNode;
    }
}