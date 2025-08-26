using System.Collections.Generic;
using UnityEngine;

public class TreeNode
{
    public string Name { get; }
    public string Path { get; }
    public List<TreeNode> Children { get; } = new List<TreeNode>();
    public object UserData { get; set; }  // Поле для пользовательских данных

    public TreeNode(string name, string path)
    {
        Name = name;
        Path = path;
        // Debug.Log(path);
    }
    
    
    // Метод глубокого копирования
    public TreeNode DeepCopy(Dictionary<TreeNode, TreeNode> mapping)
    {
        // Создаем копию узла
        var copy = new TreeNode(Name, Path);
        copy.UserData = UserData; // Поверхностная копия (для глубокой нужна доработка)
        
        // Регистрируем в словаре
        mapping[this] = copy;
        
        // Рекурсивно копируем детей
        foreach (var child in Children)
        {
            copy.Children.Add(child.DeepCopy(mapping));
        }
        return copy;
    }

    public TreeNode AddChild(string childName, string childPath)
    {
        var child = new TreeNode(childName, childPath);
        Children.Add(child);
        Debug.Log(childPath);
        return child;
    }
}