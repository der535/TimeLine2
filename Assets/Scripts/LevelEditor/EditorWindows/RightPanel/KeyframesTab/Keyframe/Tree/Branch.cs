using System;
using System.Collections.Generic;
using System.Text;
using TimeLine;
using TimeLine.LevelEditor.Save;
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
        if (nodeToRemove == null || nodeToRemove == Root)
        {
            Debug.LogWarning("[Branch] Нельзя удалить корень или null.");
            return;
        }

        // 1. Находим родителя
        string parentPath = GetParentPathFromPath(nodeToRemove.Path);
        TreeNode parentNode = FindNode(parentPath).node;

        if (parentNode != null)
        {
            // Удаляем сам узел и всех его потомков из общего списка Nodes
            RemoveFromNodesRecursive(nodeToRemove);
        
            // Удаляем узел из детей родителя
            parentNode.Children.Remove(nodeToRemove);

            // 2. Если у родителя больше нет детей, и это не корень — удаляем и родителя
            if (parentNode.Children.Count == 0 && parentNode != Root)
            {
                RemoveRecursiveUp(parentNode);
            }
        }
    }

    private void RemoveRecursiveUp(TreeNode node)
    {
        if (node == Root) return;

        string parentPath = GetParentPathFromPath(node.Path);
        TreeNode parentNode = FindNode(parentPath).node;

        if (parentNode != null)
        {
            // Убираем из списка Nodes
            Nodes.Remove(node);
            // Убираем из списка детей
            parentNode.Children.Remove(node);

            // Рекурсивно идем выше, если родитель опустел
            if (parentNode.Children.Count == 0 && parentNode != Root)
            {
                RemoveRecursiveUp(parentNode);
            }
        }
    }

// Вспомогательный метод для очистки списка Nodes от дочерних элементов
    private void RemoveFromNodesRecursive(TreeNode node)
    {
        foreach (var child in node.Children)
        {
            RemoveFromNodesRecursive(child);
        }
        Nodes.Remove(node);
    }
    private void RemovePath(TreeNode node)
    {
        string parentPath = GetParentPathFromPath(node.Path);
        Debug.Log(parentPath);
        Debug.Log(node.Path);
        if (!string.IsNullOrEmpty(parentPath))
        {
            var node3 = FindNode(parentPath).node;
            node3.Children.Remove(node);
            Nodes.Remove(node);
            RemovePath(FindNode(parentPath).node);
        }

        PrintTree();
        Debug.Log(node.Path);
        Debug.Log(Nodes.Count);
        foreach (var VARIABLE in Nodes)
        {
            Debug.Log(VARIABLE.Path);
        }
        Nodes.Remove(node);
    }

    public (TreeNode node, int childCount) FindNode(string path)
    {
        // Debug.Log($"[FindNode] Начало поиска по пути: '{path}'");

        if (string.IsNullOrEmpty(path))
        {
            // Debug.Log("[FindNode] Путь пуст, возвращаю Root.");
            return (Root, 0);
        }

        string[] parts = path.Split('/');
        TreeNode currentNode = Root;
        int childCount = 0;

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
                    childCount = currentNode.Children.Count;
                    currentNode = child;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // Debug.LogWarning($"[FindNode] Ошибка: Узел '{part}' не найден в '{currentNode.Name}'. Поиск прерван.");
                return (null, 0);
            }
        }

        // Debug.Log($"[FindNode] Успешно найден узел: '{currentNode.Name}' по пути '{path}'");
        return (currentNode, childCount);
    }

    /// <summary>
    /// Добовляе ноду, можно указать путь и он создас все недостоющие ноды, если нода уже существует возвращет существующию
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
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
            // Debug.Log($"Path: {node.Path}");
            if (node.Path == node.Name) continue;
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
        return lastSlash > 0 ? fullPath.Substring(0, lastSlash) : string.Empty;
    }
}