using System;
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
        // Nodes.Add(Root);
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
    
    public void RemoveNode(TreeNode nodeToRemove)
    {
        if (nodeToRemove == null) return;
    
        // Запрещаем удаление корневого узла
        if (nodeToRemove == Root)
        {
            Debug.LogWarning("[Branch] Нельзя удалить корневой узел ветки.");
            return;
        }

        // 1. Находим родителя ноды
        // В TreeNode обычно полезно хранить ссылку на Parent, 
        // но если её нет, ищем родителя по пути
        string parentPath = GetParentPathFromPath(nodeToRemove.Path);
        TreeNode parentNode = FindNode(parentPath);

        if (parentNode != null)
        {
            // Удаляем узел из списка детей родителя
            parentNode.Children.Remove(nodeToRemove);
        }

        // 2. Рекурсивно удаляем саму ноду и всех её потомков из общего списка Nodes
        RemoveNodeFromListRecursive(nodeToRemove);
    
        Debug.Log($"[Branch] Узел '{nodeToRemove.Name}' и его потомки удалены.");
    }

    private void RemoveNodeFromListRecursive(TreeNode node)
    {
        // Сначала проходим по всем детям
        foreach (var child in node.Children)
        {
            RemoveNodeFromListRecursive(child);
        }

        // Удаляем текущий узел из плоского списка Nodes
        Nodes.Remove(node);
    }
    
    public TreeNode FindNode(string path)
    {
        // Debug.Log($"[FindNode] Начало поиска по пути: '{path}'");

        if (string.IsNullOrEmpty(path))
        {
            // Debug.Log("[FindNode] Путь пуст, возвращаю Root.");
            return Root;
        }

        string[] parts = path.Split('/');
        TreeNode currentNode = Root;

        foreach (string part in parts)
        {
            // Пропускаем пустые части, если путь начинается с '/' или содержит '//'
            if (string.IsNullOrEmpty(part)) continue;

            bool found = false;
            // Debug.Log($"[FindNode] Ищу дочерний узел '{part}' в узле '{currentNode.Name}'");

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
            {
                // Debug.LogWarning($"[FindNode] Ошибка: Узел '{part}' не найден в '{currentNode.Name}'. Поиск прерван.");
                return null;
            }
        }

        // Debug.Log($"[FindNode] Успешно найден узел: '{currentNode.Name}' по пути '{path}'");
        return currentNode;
    }
    
    public TreeNode AddNode(string path)
    {
        // Объединяем путь и имя, чтобы корректно обработать все '/'
        // Учитываем пустой путь, чтобы не плодить лишние слэши в начале
        string fullPath = path;
    
        // StringSplitOptions.RemoveEmptyEntries уберет пустые части при случайных double slash "//"
        string[] parts = fullPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
    
        TreeNode currentNode = Root;
        string currentCumulativePath = "";

        for (int i = 0; i < parts.Length; i++)
        {
            string partName = parts[i];
        
            // Формируем путь для текущего узла (нужен для конструктора AddChild)
            currentCumulativePath = i == 0 ? partName : $"{currentCumulativePath}/{partName}";
        
            TreeNode nextNode = null;
            foreach (var child in currentNode.Children)
            {
                if (child.Name == partName)
                {
                    nextNode = child;
                    break;
                }
            }

            if (nextNode == null)
            {
                // Создаем узел, если его нет. Путь передаем накопленный до этого момента.
                nextNode = currentNode.AddChild(partName, currentCumulativePath);
                Nodes.Add(nextNode);
            }

            currentNode = nextNode;
        }

        return currentNode; // Возвращаем последний созданный или найденный узел
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
            Debug.Log($"Path: {node.Path}");
            if(node.Path == node.Name) continue;
            saveData.Nodes.Add(new TreeNodeSaveData
            {
                Path = node.Path
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