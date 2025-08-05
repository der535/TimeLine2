using System.Collections.Generic;

public class TreeNode
{
    public string Name { get; }
    public List<TreeNode> Children { get; } = new List<TreeNode>();
    public object UserData { get; set; }  // Поле для пользовательских данных

    public TreeNode(string name)
    {
        Name = name;
    }

    public TreeNode AddChild(string childName)
    {
        var child = new TreeNode(childName);
        Children.Add(child);
        return child;
    }
}